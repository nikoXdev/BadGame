using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sources.Runtime.Core.MVP.View;
using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Features.PersonDragger;
using Sources.Runtime.Gameplay.Entities;
using Sources.Runtime.Gameplay.Root;
using Sources.Runtime.Gameplay.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Sources.Runtime.Gameplay.Level
{
    public sealed class LevelState : IService, IDisposable
    {
        public LevelTimer Timer => _timer;
        
        private readonly GameData            _gameData;
        private readonly Slot                _slotPrefab;
        private readonly Person              _personPrefab;
        private readonly AllPeopleData       _allPeopleData;
        private readonly Canvas              _canvas;
        private readonly Transform           _leftSlotsContainer, _rightSlotsContainer;
        private readonly SlotSideCounter     _leftCounter, _rightCounter;
        private readonly LevelTimer          _timer;
        private readonly Lever               _lever;
        private readonly FadePanel          _endPanel;
        private readonly MarmaladeCounter    _marmaladeCounter;
        private readonly FloatingMarmaladeText _floatingTextPrefab;
        private readonly RectTransform           _floatingTextParent;
        private readonly CurtainScreen _curtainScreen;

        private List<Slot>   _leftSlots;
        private List<Slot>   _rightSlots;
        private List<Person> _persons;

        public LevelState(GameData gameData, Slot slotPrefab, Person personPrefab,
            AllPeopleData allPeopleData, Canvas canvas,
            Transform leftSlotsContainer, Transform rightSlotsContainer,
            SlotSideCounter leftCounter, SlotSideCounter rightCounter,
            LevelTimer timer, Lever lever, FadePanel endPanel,
            MarmaladeCounter marmaladeCounter,
            FloatingMarmaladeText floatingTextPrefab,
            RectTransform floatingTextParent, CurtainScreen curtainScreen)
        {
            _gameData            = gameData;
            _slotPrefab          = slotPrefab;
            _personPrefab        = personPrefab;
            _allPeopleData       = allPeopleData;
            _canvas              = canvas;
            _leftSlotsContainer  = leftSlotsContainer;
            _rightSlotsContainer = rightSlotsContainer;
            _leftCounter         = leftCounter;
            _rightCounter        = rightCounter;
            _timer               = timer;
            _lever               = lever;
            _endPanel            = endPanel;
            _marmaladeCounter    = marmaladeCounter;
            _floatingTextPrefab  = floatingTextPrefab;
            _floatingTextParent  = floatingTextParent;
            _curtainScreen = curtainScreen;
        }

        public async UniTask<LevelResult> InitializeAsync()
        {
            _leftSlots  = new List<Slot>(_gameData.SlotsCount);
            _rightSlots = new List<Slot>(_gameData.SlotsCount);
            _persons    = new List<Person>(_gameData.SlotsCount * 2);

            for (int i = 0; i < _gameData.SlotsCount; i++)
            {
                _leftSlots.Add(UnityEngine.Object.Instantiate(_slotPrefab, _leftSlotsContainer));
                _rightSlots.Add(UnityEngine.Object.Instantiate(_slotPrefab, _rightSlotsContainer));
            }

            _leftCounter.Initialize(_leftSlots);
            _rightCounter.Initialize(_rightSlots);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_leftSlotsContainer  as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rightSlotsContainer as RectTransform);

            await UniTask.NextFrame();
            await UniTask.NextFrame();

            SpawnPeopleRandomly();

            return new LevelResult(_timer, _lever, _leftSlots, _rightSlots, _persons, _endPanel,
                _marmaladeCounter, _floatingTextPrefab, _floatingTextParent, _canvas, _curtainScreen);
        }

        public void Dispose()
        {
            _leftCounter.Cleanup();
            _rightCounter.Cleanup();

            if (_persons != null)
            {
                foreach (var p in _persons)
                    if (p != null) UnityEngine.Object.Destroy(p.gameObject);
                _persons.Clear();
            }

            if (_leftSlots != null)
            {
                foreach (var s in _leftSlots)
                    if (s != null) UnityEngine.Object.Destroy(s.gameObject);
                _leftSlots.Clear();
            }

            if (_rightSlots != null)
            {
                foreach (var s in _rightSlots)
                    if (s != null) UnityEngine.Object.Destroy(s.gameObject);
                _rightSlots.Clear();
            }
        }

        private void SpawnPeopleRandomly()
        {
            int count    = UnityEngine.Random.Range(_gameData.MinPeople, _gameData.MaxPeople + 1);
            var allSlots = new List<Slot>(_leftSlots.Count + _rightSlots.Count);
            allSlots.AddRange(_leftSlots);
            allSlots.AddRange(_rightSlots);
            Shuffle(allSlots);

            var people = _allPeopleData.Get();
            for (int i = 0; i < count && i < allSlots.Count; i++)
            {
                var data = people[UnityEngine.Random.Range(0, people.Count)];
                SpawnPerson(allSlots[i], data);
            }
        }

        private void SpawnPerson(Slot slot, PersonData data)
        {
            var person      = UnityEngine.Object.Instantiate(_personPrefab, _canvas.transform);
            var canvasGroup = person.GetComponent<CanvasGroup>();
            var dragger     = new PersonDragger(_canvas, person.GetComponent<RectTransform>(),
                canvasGroup, person, data.NormalSprite, _leftSlots, _rightSlots);

            person.Construct(dragger, data);
            slot.TryOccupy(person);
            person.RelocateTo(slot);
            _persons.Add(person);
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}