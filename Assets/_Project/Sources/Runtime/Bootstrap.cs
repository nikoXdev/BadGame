using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sources
{
    public class Bootstrap : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene(1);
        }
    }
}
