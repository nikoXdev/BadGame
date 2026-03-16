using System.Threading;
using Cysharp.Threading.Tasks;
using Sources.Runtime.Core.MVP.View;
using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Audio;
using Sources.Runtime.Gameplay.Effects;
using Sources.Runtime.Gameplay.Entities;
using Sources.Runtime.Gameplay.Level;
using Sources.Runtime.Gameplay.UI;
using Sources.Runtime.Gameplay.UI.Book;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sources.Runtime.Gameplay.Root
{
    public sealed class GameplayEntryPoint : MonoBehaviour
    {
        [Header("Dialogues")]
        [SerializeField] private DialogueSystem      _dialogueSystem;
        [SerializeField] private DialogueData        _startDialogue;
        [SerializeField] private DialogueData        _postKillDialogue;
        [SerializeField] private DialogueData        _dealDialogue;
        [SerializeField] private DialogueData        _itemTutorialDialogue;
        [SerializeField] private DialogueData        _victoryDialogue;
        [SerializeField] private GameObject          _book;
        [SerializeField] private Book          _bookView;

        [Header("Marmalade")]
        [SerializeField] private MarmaladeCounter    _marmaladeCounter;
        [SerializeField] private FloatingMarmaladeText _floatingTextPrefab;
        [SerializeField] private RectTransform           _floatingTextParent;

        [Header("Level")]
        [SerializeField] private GameData            _data;
        [SerializeField] private Slot                _slotPrefab;
        [SerializeField] private Person              _personPrefab;
        [SerializeField] private AllPeopleData       _allPeopleData;
        [SerializeField] private Canvas              _canvas;
        [SerializeField] private Transform           _leftSlotsContainer;
        [SerializeField] private Transform           _rightSlotsContainer;
        [SerializeField] private SlotSideCounter     _leftCounter;
        [SerializeField] private SlotSideCounter     _rightCounter;
        [SerializeField] private LevelTimer          _timer;
        [SerializeField] private Lever               _lever;
        [SerializeField] private FadePanel          _endPanel;
        [SerializeField] private AudioService        _audioService;
        [SerializeField] private Audio.AudioSettings       _audioSettings;
        [SerializeField] private AudioSettingsPanel  _audioSettingsPanel;
        [SerializeField] private CameraShakeService _cameraShakeService;
        [SerializeField] private CurtainScreen _curtainScreen;

        private GameplayServiceLocator _serviceLocator;
        private LevelState             _levelState;
        private CancellationTokenSource _gameplayCts;

        private void Awake()
        {
            ServiceLocator.SetSceneLocator(_serviceLocator = new GameplayServiceLocator());

            _levelState = new LevelState(_data, _slotPrefab, _personPrefab, _allPeopleData, _canvas,
                _leftSlotsContainer, _rightSlotsContainer,
                _leftCounter, _rightCounter, _timer, _lever, _endPanel,
                _marmaladeCounter, _floatingTextPrefab, _floatingTextParent, _curtainScreen);
            
            _serviceLocator.TryRegisterService(_cameraShakeService);
            _serviceLocator.TryRegisterService(_levelState);
            _serviceLocator.TryRegisterService(_dialogueSystem);
            _serviceLocator.TryRegisterService(_audioService);
            _serviceLocator.TryRegisterService(_audioSettings);
            
            _gameplayCts = CancellationTokenSource
                .CreateLinkedTokenSource(destroyCancellationToken);

            RunAsync().Forget();
            
            _marmaladeCounter.OnGoalReached += HandleGoalReached;
        }

        private async UniTask RunAsync()
        {
            var ct = _gameplayCts.Token;
            _timer.gameObject.SetActive(false);

            await _curtainScreen.WaitShowAndHideAsync();
            await _audioSettingsPanel.ShowAndWaitWhileConfirmed(ct);

            _curtainScreen.HideAsync().Forget();
            await _dialogueSystem.Initialize(_startDialogue, true, ct);
            
            _timer.gameObject.SetActive(true);
            _bookView.gameObject.SetActive(true);
            var firstResult = await _levelState.InitializeAsync();
            _timer.StartCounting();

            await firstResult.WaitAsync();
            firstResult.Dispose();
            _timer.gameObject.SetActive(false);

            await UniTask.Delay(1500, cancellationToken: ct);
            _endPanel.HideAsync().Forget();
            
            _timer.IncreaseDuration();

            await _curtainScreen.WaitShowAndHideAsync();
            await _dialogueSystem.Initialize(_postKillDialogue, false, ct);
            await _dialogueSystem.Initialize(_dealDialogue, false, ct);
            await _dialogueSystem.Initialize(_itemTutorialDialogue, true, ct);
            _book.SetActive(true);
            _bookView.gameObject.SetActive(false);
            _marmaladeCounter.gameObject.SetActive(true);

            while (!ct.IsCancellationRequested)
            {
                _curtainScreen.HideAsync().Forget();
                _endPanel.HideAsync().Forget();
                _levelState.Dispose();
                _timer.gameObject.SetActive(true);
                _bookView.gameObject.SetActive(true);
                var result = await _levelState.InitializeAsync();
                _timer.Restart(); 

                await result.WaitAsync();
                result.Dispose();

                _timer.gameObject.SetActive(false);

                await UniTask.Delay(2000, cancellationToken: ct);
                _bookView.gameObject.SetActive(false);
                _endPanel.HideAsync().Forget();
            }
        }
        
        private void HandleGoalReached()
        {
            ShowVictoryAsync().Forget();
        }
        
        private async UniTaskVoid ShowVictoryAsync()
        {
            _gameplayCts.Cancel();
            
            await UniTask.Delay(2500, cancellationToken: destroyCancellationToken);
            
            await _dialogueSystem.Initialize(_victoryDialogue, true, destroyCancellationToken);

            await _endPanel.ShowAsync();

            await _curtainScreen.ShowAsync();

            SceneManager.LoadScene(sceneBuildIndex: 2);
        }

        private void OnDestroy()
        {
            _marmaladeCounter.OnGoalReached -= HandleGoalReached;
            _gameplayCts?.Cancel();
            _gameplayCts?.Dispose();
            _levelState?.Dispose();
            _serviceLocator?.Dispose();
            ServiceLocator.ClearSceneLocator();
        }
    }
}