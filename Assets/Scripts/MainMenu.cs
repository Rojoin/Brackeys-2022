using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    //public void GameScene() 
    //{
    //    SceneManager.LoadScene("Level 1");
    //}

    public void LoadScene(string sceneName) 
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame() 
    {
        Application.Quit();
    }
}
