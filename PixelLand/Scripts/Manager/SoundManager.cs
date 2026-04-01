using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static SoundManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    AudioSource bgmAudio;
    public AudioSource BGM { get { return bgmAudio; } }
    AudioSource sfxAudio;
    public AudioSource SFX { get { return sfxAudio; } }

    private void Start()
    {
        bgmAudio = transform.GetChild(0).GetComponent<AudioSource>();
        sfxAudio = transform.GetChild(1).GetComponent<AudioSource>();
    }

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
