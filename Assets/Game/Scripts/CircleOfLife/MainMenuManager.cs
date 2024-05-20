using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public SceneReference GameScene;
    public SceneReference System;
    public void LoadScene()
    {
        
    }

    public void QuiteGame()
    {
        Application.Quit();
    }
}
