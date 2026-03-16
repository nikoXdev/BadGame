using DG.Tweening;
using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Effects;
using TMPro;
using UnityEngine;

namespace Sources.Runtime.Gameplay.UI
{
    public sealed class FloatingMarmaladeText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private float _riseHeight = 130f;
        [SerializeField] private float _driftX = 60f;
        [SerializeField] private float _duration = 2.6f;

        public void Initialize(float delta, Vector2 anchoredPosition)
        {
            ServiceLocator.Get<CameraShakeService>()?.ShakeLight();
            
            var rt = (RectTransform)transform;
            rt.anchoredPosition = anchoredPosition;

            string color  = delta >= 0f ? "#88FF88" : "#FF4444";
            string prefix = delta >= 0f ? "+" : "";
            _label.text = $"<color={color}><b>{prefix}{delta:0.##}</b></color>";

            var c = _label.color; c.a = 0f; _label.color = c;

            float randomX = Random.Range(-_driftX, _driftX);
            var target = new Vector2(anchoredPosition.x + randomX, anchoredPosition.y + _riseHeight);

            DOTween.To(
                () => _label.color.a,
                a  => { var col = _label.color; col.a = a; _label.color = col; },
                1f, 0.2f);

            DOTween.To(
                () => rt.anchoredPosition,
                v  => rt.anchoredPosition = v,
                target,
                _duration)
                .SetEase(Ease.OutCubic);

            DOTween.To(
                () => _label.color.a,
                a  => { var col = _label.color; col.a = a; _label.color = col; },
                0f, _duration * 0.35f)
                .SetDelay(_duration * 0.65f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}