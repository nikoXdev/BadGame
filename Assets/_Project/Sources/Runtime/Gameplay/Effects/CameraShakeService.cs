using DG.Tweening;
using Sources.Runtime.Core.ServiceLocator;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Effects
{
    public sealed class CameraShakeService : MonoBehaviour, IService
    {
        [SerializeField] private float _defaultDuration  = 0.25f;
        [SerializeField] private float _defaultMagnitude = 0.15f;

        private Vector3  _basePosition;
        private Tweener  _tween;

        private void Awake()
        {
            _basePosition = transform.position;
        }

        public void Shake(float duration, float magnitude)
        {
            _tween?.Kill(complete: true);
            transform.position = _basePosition;

            _tween = transform
                .DOShakePosition(duration, magnitude, vibrato: 20, randomness: 90, snapping: false, fadeOut: true)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => transform.position = _basePosition);
        }

        public void ShakeLight()  => Shake(_defaultDuration * 0.6f, _defaultMagnitude * 0.4f);
        public void ShakeMedium() => Shake(_defaultDuration, _defaultMagnitude);
        public void ShakeHeavy()  => Shake(_defaultDuration * 1.5f, _defaultMagnitude * 2.5f);
    }
}