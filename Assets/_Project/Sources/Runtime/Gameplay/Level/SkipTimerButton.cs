using Sources.Runtime.Core.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

namespace Sources.Runtime.Gameplay.Level
{
    [RequireComponent(typeof(Button))]
    public sealed class SkipTimerButton : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Skip);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(Skip);
        }

        private void Skip()
        {
            var timer = ServiceLocator.Get<LevelState>()?.Timer;
            timer?.ForceFinish();
        }
    }
}