using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sources.Runtime.Core.MVP.View;
using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Audio;
using Sources.Runtime.Gameplay.Effects;
using Sources.Runtime.Gameplay.Entities;
using Sources.Runtime.Gameplay.UI;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Level
{
    public sealed class LevelResult : IDisposable
    {
        private readonly LevelTimer            _timer;
        private readonly Lever                 _lever;
        private readonly IReadOnlyList<Slot>   _leftSlots;
        private readonly IReadOnlyList<Slot>   _rightSlots;
        private readonly IReadOnlyList<Person> _persons;
        private readonly FadePanel            _endPanel;
        private readonly MarmaladeCounter      _counter;
        private readonly FloatingMarmaladeText _floatingTextPrefab;
        private readonly RectTransform         _floatingTextParent;
        private readonly Canvas                _canvas;
        private readonly CurtainScreen _curtainScreen;

        private readonly UniTaskCompletionSource _tcs = new();

        public LevelResult(LevelTimer timer, Lever lever,
            IReadOnlyList<Slot> leftSlots, IReadOnlyList<Slot> rightSlots,
            IReadOnlyList<Person> persons, FadePanel endPanel,
            MarmaladeCounter counter,
            FloatingMarmaladeText floatingTextPrefab,
            RectTransform floatingTextParent,
            Canvas canvas, CurtainScreen curtainScreen)
        {
            _timer              = timer;
            _lever              = lever;
            _leftSlots          = leftSlots;
            _rightSlots         = rightSlots;
            _persons            = persons;
            _endPanel           = endPanel;
            _counter            = counter;
            _floatingTextPrefab = floatingTextPrefab;
            _floatingTextParent = floatingTextParent;
            _canvas             = canvas;
            _curtainScreen = curtainScreen;

            _timer.OnTimeUp += HandleTimeUp;
        }

        public UniTask WaitAsync() => _tcs.Task;

        public void Dispose() => _timer.OnTimeUp -= HandleTimeUp;

        private async void HandleTimeUp()
        {
            LockAll();
            
            await _endPanel.ShowAsync();
            
            ShowResultAsync().Forget();
        }

        private async UniTaskVoid ShowResultAsync()
        {
            await _curtainScreen.WaitShowAndHideAsync();
            
            var audio = ServiceLocator.Get<AudioService>();
            
            audio?.PlayTrainApproach();

            await UniTask.WaitForSeconds(0.5f);
            
            var slots   = _lever.IsRight ? _rightSlots : _leftSlots;
            float running = _counter.Total;

            var entries = new List<(Vector2 anchoredPos, float delta, Person person)>();

            foreach (var slot in slots)
            {
                if (slot.IsFree) continue;

                var person      = slot.CurrentPerson;
                var personRect  = person.GetComponent<RectTransform>();
                var anchoredPos = GetAnchoredPosition(personRect);
                var worldPos    = GetWorldPosition(personRect);

                float before = running;
                running      = person.Data.Apply(running);
                float delta  = running - before;

                entries.Add((anchoredPos, delta, person));
                BloodSpawner.Instance.SpawnBlood(worldPos);
                audio?.PlayBloodSplat();
            }

            await UniTask.Delay(600);

            foreach (var person in _persons)
                if (person != null)
                    UnityEngine.Object.Destroy(person.gameObject);

            await UniTask.Delay(1400);

            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var (anchoredPos, delta, _) = entries[i];
                SpawnFloatingText(delta, anchoredPos);
                audio?.PlayFloatingPop();
                _counter.Add(delta);
                audio?.PlayMarmaladeCount();
                await UniTask.Delay(350);
            }

            await UniTask.Delay(1200);

            audio?.PlayRoundEnd();
            BloodSpawner.Instance.ClearAllSplats(0.7f);

            await UniTask.Delay(800);

            _tcs.TrySetResult();
        }

        private void SpawnFloatingText(float delta, Vector2 anchoredPos)
        {
            if (_floatingTextPrefab == null || _floatingTextParent == null) return;

            var text = UnityEngine.Object.Instantiate(_floatingTextPrefab, _floatingTextParent);
            text.Initialize(delta, anchoredPos);
        }

        private void LockAll()
        {
            foreach (var person in _persons)
                person.SetInteractable(false);
        }

        private Vector2 GetAnchoredPosition(RectTransform rt)
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, rt.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _floatingTextParent, screenPoint, _canvas.worldCamera, out var localPoint);
            return localPoint;
        }

        private static Vector2 GetWorldPosition(RectTransform rt)
        {
            var canvas = rt.GetComponentInParent<Canvas>();
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rt,
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.position),
                canvas.worldCamera,
                out var worldPos);
            return worldPos;
        }
    }
}