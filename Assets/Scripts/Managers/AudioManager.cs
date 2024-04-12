using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    private ComponentPooler<AudioSource> audioSourcePooler;

    public AudioManager(GameObject me)
    {
        audioSourcePooler = new AudioSourcePooler(me);
    }

    public AudioSource PlaySound(AudioClip clip)
    {
        if (!GameManager.GetSoundsOn()) return null;

        AudioSource audioSource = audioSourcePooler.GetComponent();
        audioSource.clip = clip;
        audioSource.PlayOneShot(clip);

        return audioSource;
    }

    public void StopSound(AudioSource audioSource)
    {
        if (audioSource == null) return;

        audioSource.Stop();
        audioSourcePooler.ReturnComponent(audioSource);
    }
}