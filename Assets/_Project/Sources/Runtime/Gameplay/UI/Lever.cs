using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Audio;
using Sources.Runtime.Gameplay.Effects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sources.Runtime.Gameplay.UI
{
    public sealed class Lever : MonoBehaviour
    {
        [SerializeField] private Image  _image;
        [SerializeField] private Sprite _leftSprite;
        [SerializeField] private Sprite _rightSprite;

        private bool _isRight = false;

        public bool IsRight => _isRight;

        private void Awake() => UpdateVisual();

        public void Toggle()
        {
            _isRight = !_isRight;
            ServiceLocator.Get<AudioService>()?.PlayLeverPull();
            UpdateVisual();
            ServiceLocator.Get<CameraShakeService>()?.ShakeMedium();
        }

        public void SetState(bool isRight)
        {
            _isRight = isRight;
            UpdateVisual();
        }

        private void UpdateVisual() =>
            _image.sprite = _isRight ? _rightSprite : _leftSprite;
    }
}