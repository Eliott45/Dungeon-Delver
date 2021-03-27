using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject UIMenu;


    public void StartGame()
    {
        SceneManager.LoadScene("_Scene_Main");
    }
}
