using UnityEngine;
using UnityEngine.UI;

namespace Sources
{
    [RequireComponent(typeof(Button))]
    public class QuitButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Quit);
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(Quit);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}