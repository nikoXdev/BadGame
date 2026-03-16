using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sources
{
    [RequireComponent(typeof(Button))]
    public class RestartButton : MonoBehaviour
    {
        private Button _button;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Restart);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(Restart);
        }

        private void Restart() => SceneManager.LoadScene(0);
    }
}