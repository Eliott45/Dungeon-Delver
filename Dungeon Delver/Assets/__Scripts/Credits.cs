using UnityEngine;
using UnityEngine.SceneManagement;

namespace __Scripts
{
    public class Credits : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown("escape"))
            {
                SceneManager.LoadScene("Menu");
            }
        }
    }
}
