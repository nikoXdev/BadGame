using System;
using DG.Tweening;
using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Audio;
using Sources.Runtime.Gameplay.Effects;
using TMPro;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Level
{
    public sealed class LevelTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private float           _duration        = 10f;
        [SerializeField] private float           _urgentThreshold = 10f;

        [Header("Color")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _urgentColor = new Color(1f, 0.25f, 0.25f, 1f);

        [Header("Size")]
        [SerializeField] private float _normalFontSize = 36f;
        [SerializeField] private float _urgentFontSize = 48f;

        [Header("Animation")]
        [SerializeField] private float _visualTweenDuration = 0.15f;
        [SerializeField] private float _punchScale          = 0.2f;
        [SerializeField] private float _punchDuration       = 0.2f;

        public event Action OnTimeUp;
        public bool IsFinished { get; private set; }

        private float    _remaining;
        private int      _lastTickSecond = -1;
        private bool     _isUrgent;
        private Sequence _visualSequence;
        private Tweener  _punchTween;

        private void Awake()
        {
            _remaining = _duration;
            IsFinished  = false;
            SetVisualInstant(false);
            enabled = false;
        }

        private void OnDestroy() => KillTweens();

        public void StartCounting()
        {
            _remaining      = _duration;
            _lastTickSecond = -1;
            _isUrgent       = false;
            IsFinished       = false;
            SetVisualInstant(false);
            UpdateLabel();
            enabled = true;
        }

        public void IncreaseDuration() => _duration *= 2;
        public void Restart()          => StartCounting();

        public void ForceFinish()
        {
            if (IsFinished) return;
            Finish();
        }

        private void Update()
        {
            if (IsFinished) return;

            _remaining -= Time.deltaTime;

            if (_remaining <= 0f)
            {
                _remaining = 0f;
                Finish();
                return;
            }

            bool urgent = _remaining <= _urgentThreshold;
            if (urgent != _isUrgent)
            {
                _isUrgent = urgent;
                AnimateVisual(_isUrgent);
            }

            int currentSecond = Mathf.CeilToInt(_remaining);
            if (currentSecond != _lastTickSecond)
            {
                _lastTickSecond = currentSecond;
                ServiceLocator.Get<AudioService>()?.PlayTimerTick();
                Punch();
            }

            UpdateLabel();
        }

        private void Finish()
        {
            IsFinished = true;
            UpdateLabel();
            ServiceLocator.Get<AudioService>()?.PlayTimerAlarm();
            ServiceLocator.Get<CameraShakeService>()?.ShakeHeavy();
            OnTimeUp?.Invoke();
        }

        private void AnimateVisual(bool urgent)
        {
            _visualSequence?.Kill();
            _visualSequence = DOTween.Sequence()
                .Join(DOTween.To(
                        () => _label.fontSize,
                        x  => _label.fontSize = x,
                        urgent ? _urgentFontSize : _normalFontSize,
                        _visualTweenDuration)
                    .SetEase(Ease.OutBack))
                .Join(DOTween.To(
                        () => _label.color,
                        x  => _label.color = x,
                        urgent ? _urgentColor : _normalColor,
                        _visualTweenDuration)
                    .SetEase(Ease.OutQuad));
        }

        private void Punch()
        {
            _punchTween?.Kill();
            _label.transform.localScale = Vector3.one;
            _punchTween = _label.transform
                .DOPunchScale(Vector3.one * _punchScale, _punchDuration, vibrato: 1, elasticity: 0.5f);
        }

        private void SetVisualInstant(bool urgent)
        {
            KillTweens();
            _label.color              = urgent ? _urgentColor    : _normalColor;
            _label.fontSize           = urgent ? _urgentFontSize : _normalFontSize;
            _label.transform.localScale = Vector3.one;
        }

        private void KillTweens()
        {
            _visualSequence?.Kill();
            _punchTween?.Kill();
        }

        private void UpdateLabel()
        {
            int minutes = Mathf.FloorToInt(_remaining / 60f);
            int seconds = Mathf.FloorToInt(_remaining % 60f);
            _label.text = $"{minutes:00}:{seconds:00}";
        }
    }
}