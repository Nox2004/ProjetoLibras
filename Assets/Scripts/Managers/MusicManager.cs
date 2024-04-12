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
    [SerializeField] private float crossfadeDuration;
    private AudioClip currentMusic;
    private AudioSource currentAudioSource, oldAudioSource;
    private float currentMusicVolume;

    void Start ()
    {
        oldAudioSource = gameObject.AddComponent<AudioSource>();
        currentAudioSource = gameObject.AddComponent<AudioSource>();
        
        PlayMusic(SceneManager.GetActiveScene().name);
    }

    public void PlayMusic(string sceneName)
    {
        AudioClip music = Array.Find(musicsByScene, x => x.sceneName == sceneName).music;

        if (music != null && music != currentMusic)
        {
            currentMusic = music;
            ChangeMusic(currentMusic);
        }
    }
    
    public void StopMusic()
    {
        currentAudioSource.Stop();
    }

    public void ChangeMusic(AudioClip newMusic)
    {
        //change music with crossfade
        AudioSource tmp = oldAudioSource;

        //set current audio source as the old one and vice versa
        oldAudioSource = currentAudioSource;
        currentAudioSource = tmp;

        currentAudioSource.clip = newMusic;
        currentAudioSource.volume = 0;
        currentAudioSource.Play();
    }

    void Update ()
    {
        if (oldAudioSource.isPlaying)
        {
            oldAudioSource.volume -= Time.deltaTime / crossfadeDuration;
            if (oldAudioSource.volume <= 0) oldAudioSource.Stop();
        }
        
        if (currentAudioSource.volume < currentMusicVolume && currentAudioSource.isPlaying)
        {
            currentAudioSource.volume += Time.deltaTime / crossfadeDuration;
        }
    }

    public void UpdateVolume(float volume)
    {
        currentMusicVolume = volume;

        if (currentAudioSource != null) currentAudioSource.volume = volume;
        if (oldAudioSource != null) oldAudioSource.volume = volume;
    }
}