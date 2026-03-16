using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Febucci.UI;
using Sources.Runtime.Core.MVP.View;
using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sources.Runtime.Gameplay
{
    public class DialogueSystem : MonoBehaviour, IService
    {
        private const string SkipHint     = "<wiggle>Space To Skip</wiggle>";
        private const string ContinueHint = "<wiggle>Space To Continue</wiggle>";

        public event Action OnDialogueStarted;
        public event Action OnDialogueFinished;

        [SerializeField] private CanvasGroup        _canvasGroup;
        [SerializeField] private TextMeshProUGUI    _dialogueText;
        [SerializeField] private TextAnimator       _textAnimator;
        [SerializeField] private TextAnimatorPlayer _textAnimatorPlayer;
        [SerializeField] private TextMeshProUGUI    _skipText;
        [SerializeField] private TextAnimatorPlayer _skipTextAnimatorPlayer;
        [SerializeField] private float              _fadeDuration = 0.3f;
        [SerializeField] private CurtainScreen _curtainScreen;

        private bool _inputReceived;
        private bool _textCompleted;

        public async UniTask Initialize(DialogueData data, bool hide = true, CancellationToken externalToken = default)
        {
            using var linkedCts = CancellationTokenSource
                .CreateLinkedTokenSource(externalToken, destroyCancellationToken);

            var ct = linkedCts.Token;

            OnDialogueStarted?.Invoke();
            ServiceLocator.Get<AudioService>()?.PlayDialogueOpen();

            _canvasGroup.interactable   = true;
            _canvasGroup.blocksRaycasts = true;
            await FadeAsync(0f, 1f);

            _textAnimatorPlayer.onCharacterVisible.AddListener(OnCharacterVisible);

            foreach (var content in data.Contents)
            {
                ct.ThrowIfCancellationRequested();

                _inputReceived = false;
                _textCompleted = false;

                ShowSkipHint(SkipHint);

                _textAnimatorPlayer.onTextShowed.AddListener(OnTextShowed);
                _textAnimatorPlayer.ShowText(content);

                while (_textCompleted == false)
                {
                    ct.ThrowIfCancellationRequested();

                    if (_inputReceived == true)
                    {
                        _inputReceived = false;
                        _textAnimatorPlayer.SkipTypewriter();
                    }

                    await UniTask.Yield(ct);
                }

                _textAnimatorPlayer.onTextShowed.RemoveListener(OnTextShowed);

                _inputReceived = false;
                ShowSkipHint(ContinueHint);

                await UniTask.WaitUntil(() => _inputReceived, cancellationToken: ct);
                _inputReceived = false;
            }

            _textAnimatorPlayer.onCharacterVisible.RemoveListener(OnCharacterVisible);

            OnDialogueFinished?.Invoke();

            if(hide == true)
                await FadeAsync(1f, 0f);

            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;

            ServiceLocator.Get<AudioService>()?.PlayDialogueClose();
        }

        private async UniTask FadeAsync(float from, float to)
        {
            float elapsed = 0f;
            _canvasGroup.alpha = from;

            while (elapsed < _fadeDuration)
            {
                elapsed           += Time.deltaTime;
                _canvasGroup.alpha  = Mathf.Lerp(from, to, elapsed / _fadeDuration);
                _curtainScreen.HideAsync().Forget();
                await UniTask.Yield();
            }

            _canvasGroup.alpha = to;
        }

        private void OnCharacterVisible(char character)
        {
            if (char.IsWhiteSpace(character) || char.IsPunctuation(character))
                return;

            ServiceLocator.Get<AudioService>()?.PlayDialogueTick();
        }

        private void OnTextShowed() => _textCompleted = true;

        private void ShowSkipHint(string hint)
        {
            if (_skipTextAnimatorPlayer != null)
                _skipTextAnimatorPlayer.ShowText(hint);
            else
                _skipText.text = hint;
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                _inputReceived = true;
        }
    }
}