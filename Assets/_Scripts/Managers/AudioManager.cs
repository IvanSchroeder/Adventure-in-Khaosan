using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExtensionMethods;

[Serializable]
public class Sound {
    public string name;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour {
    [Header("General References")]
    [Space(5)]
    public static AudioManager instance;

    public Sound[] musicSounds;
    public Sound[] sfxSounds;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public IntSO MusicVolume;
    public IntSO SFXVolume;

    private void OnEnable() {
        MusicVolume.OnValueChange += ChangeMusicVolume;
        SFXVolume.OnValueChange += ChangeSFXVolume;
    }

    private void OnDisable() {
        MusicVolume.OnValueChange -= ChangeMusicVolume;
        SFXVolume.OnValueChange -= ChangeSFXVolume;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(string name) {
        Sound sound = Array.Find(musicSounds, x => x.name == name);

        if (sound.IsNull()) Debug.Log("Sound not found");
        else {
            musicSource.clip = sound.clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(string name) {
        Sound sound = Array.Find(sfxSounds, x => x.name == name);

        if (sound.IsNull()) Debug.Log("Sound not found");
        else sfxSource.PlayOneShot(sound.clip);
    }

    public void PlayClip(AudioSource source, AudioClip clip, bool oneShot) {
        if (oneShot) {
            source.PlayOneShot(clip);
        }
        else {
            source.clip = clip;
            source.Play();
        }
    }

    public void StopMusic() {
        musicSource.Stop();
    }

    public void TogglePauseMusic(bool on) {
        if (on) musicSource.UnPause();
        else musicSource.Pause();
    }

    public void ToggleMusic() {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX() {
        sfxSource.mute = !sfxSource.mute;
    }

    public void ChangeMusicVolume(ValueSO<int> volume) {
        musicSource.volume = volume.Value / 10f;
    }

    public void ChangeSFXVolume(ValueSO<int> volume) {
        sfxSource.volume = volume.Value / 10f;
    }
}
