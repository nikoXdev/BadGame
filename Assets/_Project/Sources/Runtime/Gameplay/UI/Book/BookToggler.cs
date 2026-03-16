using DG.Tweening;
using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Sources.Runtime.Gameplay.UI.Book
{
    public sealed class BookToggler : MonoBehaviour
    {
        [SerializeField] private Button _showButton;
        [SerializeField] private Button _hideButton;
        [SerializeField] private CanvasGroup _bookCanvasGroup;
        [SerializeField] private float _fadeDuration = 0.2f;

        private void Awake() => ApplyState(false);

        private void OnEnable()
        {
            _showButton.onClick.AddListener(Show);
            _hideButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            _showButton.onClick.RemoveListener(Show);
            _hideButton.onClick.RemoveListener(Hide);
        }

        private void Show()
        {
            ServiceLocator.Get<AudioService>()?.PlayBookOpen();
            ApplyState(true);
        }

        private void Hide()
        {
            ServiceLocator.Get<AudioService>()?.PlayButtonClick();
            ApplyState(false);
        }

        private void ApplyState(bool visible)
        {
            Debug.Log($"AppyState with {visible}");
            
            float target = visible ? 1f : 0f;

            _bookCanvasGroup.interactable = visible;
            _bookCanvasGroup.blocksRaycasts = visible;

            _bookCanvasGroup.alpha = target;
        }
    }
}