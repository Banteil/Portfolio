using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundResource : MonoBehaviour
{
    private static SoundResource instance = null;

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

    public static SoundResource Instance
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

    AudioClip[] swordSwingSFX;
    public AudioClip[] SwordSwingSFX { get { return swordSwingSFX; } }

    private void Start()
    {
        swordSwingSFX = Resources.LoadAll<AudioClip>("Sounds/SFX/SwordSwing");
    }
}
