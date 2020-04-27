using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour { 
    
    public Sound[] sounds;
    public Transform soundPosition;
    private List<Sound> inGameThemesList = new List<Sound>();
    void Awake()
    {
        foreach(Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();

            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.loop = false;
            sound.source.playOnAwake = false;
        }
    }

    void Start()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.name.Contains("inGameTheme"))
            {
                inGameThemesList.Add(sound);
            }
        }
    }

    /// <summary>
    /// Playing a sound
    /// </summary>
    /// <param name="name">Name of the sound</param>
    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);   
        s.source.Play();
    }

    public void MuteSound()
    {
        foreach (Sound sound in sounds)
        {
            sound.source.volume = 0;           
        }
    }

    public bool IsThisPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s.source.isPlaying)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ResumeSound()
    {
        foreach (Sound sound in sounds)
        {
            sound.source.volume = sound.volume;
        }
    }

    public void StopSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();
    }

    public void StopWithFadeAtEnd(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        StartCoroutine(FadeOut(s.source, 0.1f));
    }

    public void PlayRandomInGameTheme()
    {
        int r = UnityEngine.Random.Range(0, 3);
        inGameThemesList[r].source.Play();
    }

    public void StopTheRandomInGameTheme()
    {
        foreach(Sound sound in inGameThemesList)
        {
            sound.source.Stop();
        }
    }

    public void StopAllSounds()
    {
        foreach (Sound sound in sounds)
        {
            sound.source.Stop();
        }
    } 

    public void StopAllSoundFx()
    {
        foreach (Sound sound in sounds)
        {
            if (!sound.name.Contains("Theme"))
            {
                sound.source.Stop();
            }
        }
    }

    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
