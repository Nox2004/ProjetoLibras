using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSceneManager
{
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        MusicManager.Instance.PlayMusic(sceneName); //!!!Change later (musicmanager logic should be cointained within itself)
    }
}