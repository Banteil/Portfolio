using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField]
    AudioSource bgmAudio;
    public AudioSource BGM { get { return bgmAudio; } }
    [SerializeField]
    AudioSource sfxAudio;
    public AudioSource SFX { get { return sfxAudio; } }

    public void PlaySFX(AudioClip[] clips)
    {
        sfxAudio.clip = clips[Random.Range(0, clips.Length)];
        sfxAudio.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxAudio.clip = clip;
        sfxAudio.Play();
    }
}
