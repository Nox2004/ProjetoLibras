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

    public AudioSource PlaySound(AudioClip clip, bool random_pitch = false, float base_pitch = 1f)
    {
        if (!GameManager.GetSoundsOn()) return null;

        AudioSource audioSource = audioSourcePooler.GetComponent();

        audioSource.clip = clip;
        if (random_pitch) audioSource.pitch = Random.Range(0.95f, 1.05f) * base_pitch;
        else audioSource.pitch = base_pitch;
        audioSource.PlayOneShot(clip);

        return audioSource;
    }

    public AudioSource PlayRandomSound(AudioClip[] possible_clip, bool random_pitch = false,  float base_pitch = 1f)
    {
        if (!GameManager.GetSoundsOn()) return null;

        int randomIndex = Random.Range(0, possible_clip.Length);
        return PlaySound(possible_clip[randomIndex], random_pitch, base_pitch);
    }

    public void PauseAllSounds()
    {
        foreach (AudioSource audioSource in audioSourcePooler.GetActiveComponents())
        {
            audioSource.Pause();
        }
    }

    public void ResumeAllSounds()
    {
        foreach (AudioSource audioSource in audioSourcePooler.GetActiveComponents())
        {
            audioSource.UnPause();
        }
    }

    public void StopSound(AudioSource audioSource)
    {
        if (audioSource == null) return;

        audioSource.Stop();
        audioSourcePooler.ReturnComponent(audioSource);
    }
}