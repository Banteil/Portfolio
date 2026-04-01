using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance = null;
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

    [SerializeField]
    AudioSource bgmPlayer;
    public AudioSource BGMPlayer { get { return bgmPlayer; } }

    public float BGMVolume 
    { 
        get { return bgmPlayer.volume; } 
        set 
        {
            if (value < 0f) value = 0f;
            else if(value > 1f) value = 1f;

            bgmPlayer.volume = value;
        } 
    }

    [SerializeField]
    AudioSource sfxPlayer;
    public AudioSource SFXPlayer { get { return sfxPlayer; } }

    public float SFXVolume 
    {
        get { return sfxPlayer.volume; }
        set
        {
            if (value < 0f) value = 0f;
            else if (value > 1f) value = 1f;

            sfxPlayer.volume = value;
        }
    }

    public void BGMPlay(AudioClip clip)
    {
        bgmPlayer.clip = clip;
        bgmPlayer.Play();
    }
}
