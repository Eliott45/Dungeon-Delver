using UnityEngine;
using UnityEngine.SceneManagement;

namespace __Scripts
{
    public class Menu : MonoBehaviour
    {
        [Header("Set in Inspector")]
        [SerializeField] private GameObject UIMenu;
        [SerializeField] private GameObject UIOptions;


        public void StartGame()
        {
            SceneManager.LoadScene("_Scene_Main");
        }

        public void ShowOptions()
        {
            UIMenu.SetActive(false);
            UIOptions.SetActive(true);
        }

        public void HideOptions()
        {
            UIMenu.SetActive(true);
            UIOptions.SetActive(false);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        public void ShowCredits()
        {
            SceneManager.LoadScene("Credits");
        }

        public void Exit()
        {
            Application.Quit();
        }

        public void Restart()
        {
            SceneManager.LoadScene("_Scene_Main");
        }


        public void ToMenu()
        {
            SceneManager.LoadScene("Menu");
        }

    }
}
