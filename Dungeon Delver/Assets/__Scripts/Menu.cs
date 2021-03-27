using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject UIMenu;
    public GameObject UIOptions;


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

}
