using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Injector
{
    public static ChangeSceneManager GetSceneManager()
    {
        return new ChangeSceneManager();
    }

    public static AudioManager GetAudioManager(GameObject me)
    {
        return new AudioManager(me);
    }
}