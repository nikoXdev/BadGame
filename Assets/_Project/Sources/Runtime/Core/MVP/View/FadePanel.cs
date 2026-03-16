using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Sources.Runtime.Core.MVP.View
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class FadePanel : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            SetVisible(false);
        }

        public async UniTask ShowAsync()
        {
            SetVisible(true);
            await FadeAsync(0f, 1f);
        }

        public async UniTask HideAsync()
        {
            await FadeAsync(1f, 0f);
            SetVisible(false);
        }

        private async UniTask FadeAsync(float from, float to)
        {
            float elapsed = 0f;
            _canvasGroup.alpha = from;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
                await UniTask.Yield();
            }

            _canvasGroup.alpha = to;
        }

        private void SetVisible(bool visible)
        {
            _canvasGroup.alpha          = visible ? 1f : 0f;
            _canvasGroup.interactable   = visible;
            _canvasGroup.blocksRaycasts = visible;
            gameObject.SetActive(visible);
        }
    }
}