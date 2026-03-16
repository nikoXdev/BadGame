using DG.Tweening;
using TMPro;
using UnityEngine;
using System;

namespace Sources.Runtime.Gameplay.UI
{
    public sealed class MarmaladeCounter : MonoBehaviour
    {
        public event Action OnGoalReached;

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private int _goal = 999;

        private float _actual;
        private float _displayed;
        private Tweener _tween;
        private bool _goalReached;

        public float Total => _actual;

        private void Awake() => UpdateLabel(_displayed);

        public void Add(float delta)
        {
            _actual += delta;

            _tween?.Kill();
            _tween = DOTween.To(
                () => _displayed,
                x  => { _displayed = x; UpdateLabel(x); },
                _actual,
                0.8f)
                .SetEase(Ease.OutCubic)
                .OnComplete(CheckGoal);
        }

        private void CheckGoal()
        {
            if (_goalReached) 
                return;
            
            if (_actual < _goal)
                return;

            _goalReached = true;
            OnGoalReached?.Invoke();
        }

        private void UpdateLabel(float value) =>
            _label.text = $"{Mathf.RoundToInt(value)} / {_goal}";
    }
}