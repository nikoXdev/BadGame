using DG.Tweening;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Effects
{
    public sealed class BloodSplat : MonoBehaviour
    {
        [SerializeField] private float lifetime        = 8f;
        [SerializeField] private float fadeOutDuration = 1.2f;

        private SpriteRenderer _sr;
        private Sequence       _sequence;

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        private void SetAlpha(float a)
        {
            var c = _sr.color; c.a = a; _sr.color = c;
        }

        public void Initialize(Sprite sprite)
        {
            _sr.sprite = sprite;

            float targetAlpha = Random.Range(0.7f, 1f);
            float targetScale = Random.Range(0.3f, 0.85f);

            SetAlpha(0f);
            transform.localScale = Vector3.zero;
            transform.rotation   = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            _sequence = DOTween.Sequence()
                .Append(transform.DOScale(targetScale, 0.12f).SetEase(Ease.OutBack, overshoot: 2f))
                .Join(DOVirtual.Float(0f, targetAlpha, 0.08f, SetAlpha))
                .AppendInterval(lifetime - fadeOutDuration)
                .Append(DOVirtual.Float(targetAlpha, 0f, fadeOutDuration, SetAlpha).SetEase(Ease.InQuad))
                .OnComplete(() => Destroy(gameObject));
        }

        public void FadeOutFast(float duration = 0.5f)
        {
            _sequence?.Kill();
            DOVirtual.Float(_sr.color.a, 0f, duration, SetAlpha)
                .SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}