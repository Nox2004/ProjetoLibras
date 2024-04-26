using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneManager
{
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        MusicManager.Instance.ChangeMusicBySceneName(sceneName); //!!!Change later (musicmanager logic should be cointained within itself)
    }

    public void RestartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}