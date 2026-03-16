using System.Collections.Generic;
using DG.Tweening;
using Sources.Runtime.Gameplay.Entities;
using TMPro;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Level
{
    public sealed class SlotSideCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;

        [Header("Size")]
        [SerializeField] private float _minFontSize  = 24f;
        [SerializeField] private float _maxFontSize  = 36f;

        [Header("Color")]
        [SerializeField] private Color _emptyColor = new Color(1f, 1f, 1f, 0.4f);
        [SerializeField] private Color _fullColor  = new Color(1f, 0.3f, 0.3f, 1f);

        [Header("Animation")]
        [SerializeField] private float _tweenDuration = 0.25f;

        private IReadOnlyList<Slot> _slots;
        private Sequence            _sequence;

        public void Initialize(IReadOnlyList<Slot> slots)
        {
            Cleanup();

            _slots = slots;
            foreach (var slot in _slots)
                slot.OnOccupancyChanged += Refresh;

            Refresh();
        }

        public void Cleanup()
        {
            if (_slots == null) return;
            foreach (var slot in _slots)
                if (slot != null) slot.OnOccupancyChanged -= Refresh;
            _slots = null;
        }

        private void Refresh()
        {
            int occupied = 0;
            foreach (var slot in _slots)
                if (!slot.IsFree) occupied++;

            float t = _slots.Count > 0 ? (float)occupied / _slots.Count : 0f;
            float targetSize  = Mathf.Lerp(_minFontSize, _maxFontSize, t);
            Color targetColor = Color.Lerp(_emptyColor, _fullColor, t);

            _label.text = $"{occupied} / {_slots.Count}";

            _sequence?.Kill();
            _sequence = DOTween.Sequence()
                .Join(DOTween.To(
                    () => _label.fontSize,
                    x  => _label.fontSize = x,
                    targetSize, _tweenDuration)
                    .SetEase(Ease.OutBack))
                .Join(DOTween.To(
                    () => _label.color,
                    x  => _label.color = x,
                    targetColor, _tweenDuration)
                    .SetEase(Ease.OutQuad));
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
            Cleanup();
        }
    }
}