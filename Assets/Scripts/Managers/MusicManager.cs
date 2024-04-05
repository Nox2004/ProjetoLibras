using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class MusicManager : Singleton<MusicManager>
{
    [Serializable]
    private struct MusicByScene
    {
        public string sceneName;
        public AudioClip music;
    }

    [SerializeField] private MusicByScene[] musicsByScene;
    private AudioClip currentMusic;
    private AudioSource audioSource;    

    void Start ()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        PlayMusic(SceneManager.GetActiveScene().name);
    }

    public void PlayMusic(string sceneName)
    {
        AudioClip music = Array.Find(musicsByScene, x => x.sceneName == sceneName).music;

        if (music != null && music != currentMusic)
        {
            currentMusic = music;
            audioSource.clip = currentMusic;
            audioSource.Play();
        }
    }
    
    public void StopMusic()
    {
        audioSource.Stop();
    }
}