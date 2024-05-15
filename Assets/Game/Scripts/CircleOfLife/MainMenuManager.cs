using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public SceneReference GameScene;

    public void LoadScene()
    {
        SceneManager.LoadScene(GameScene.SceneName, LoadSceneMode.Single);
    }

    public void QuiteGame()
    {
        Application.Quit();
    }
}
