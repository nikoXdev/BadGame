using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sources.Runtime.Core.ServiceLocator;
using System.Threading;
using Sources.Runtime.Core.MVP.View;
using UnityEngine;
using UnityEngine.UI;

namespace Sources.Runtime.Gameplay.Audio
{
    public sealed class AudioSettingsPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button      _confirmButton;
        [SerializeField] private float       _fadeDuration = 0.2f;

        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private CurtainScreen _curtainScreen;

        private AudioSettings _settings;
        private AudioService  _audio;
        private bool          _ready;

        private void Start()
        {
            _settings = ServiceLocator.Get<AudioSettings>();
            _audio    = ServiceLocator.Get<AudioService>();

            InitSlider(_masterSlider, _settings.MasterVolume, v => _settings.SetMaster(v));
            InitSlider(_musicSlider,  _settings.MusicVolume,  v => _settings.SetMusic(v));
            InitSlider(_sfxSlider,    _settings.SFXVolume,    v => _settings.SetSFX(v));

            _ready = true;
            SetVisible(false);
        }

        public async UniTask ShowAndWaitWhileConfirmed(CancellationToken ct = default)
        {
            if (!_ready)
                await UniTask.WaitUntil(() => _ready, cancellationToken: ct);

            var tcs = new UniTaskCompletionSource();

            _confirmButton.onClick.AddListener(OnConfirmClicked);
            SetVisible(true);

            await tcs.Task;

            void OnConfirmClicked()
            {
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);
                ConfirmAsync().Forget();
            }

            async UniTaskVoid ConfirmAsync()
            {
                await _curtainScreen.ShowAsync();
                _audio?.PlayButtonClick();
                PlayerPrefs.Save();
                SetVisible(false);
                tcs.TrySetResult();
            }
        }

        private void SetVisible(bool visible)
        {
            _canvasGroup.interactable   = visible;
            _canvasGroup.blocksRaycasts = visible;

            _canvasGroup.alpha = visible ? 1f : 0f;
        }

        private void InitSlider(Slider slider, float saved, System.Action<float> onChange)
        {
            if (slider == null) return;

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value    = saved;

            slider.onValueChanged.AddListener(v =>
            {
                if (_ready) onChange(v);
            });
        }

        private void InitMute(Toggle toggle, Slider linked, System.Action<float> onChange)
        {
            if (toggle == null) return;

            toggle.isOn = false;

            toggle.onValueChanged.AddListener(muted =>
            {
                if (!_ready) return;

                float value = muted ? 0f : (linked != null ? linked.value : 1f);
                onChange(value);

                if (linked != null)
                    linked.interactable = !muted;

                _audio?.PlayButtonClick();
            });
        }
    }
}