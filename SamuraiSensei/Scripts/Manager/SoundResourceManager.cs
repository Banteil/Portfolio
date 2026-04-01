using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundResourceManager : MonoBehaviour
{
    private static SoundResourceManager instance = null;
    public static SoundResourceManager Instance
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
            ResourceSetting();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    AudioClip gameClear;
    public AudioClip GameClear { get { return gameClear; } }
    AudioClip gameOver;
    public AudioClip GameOver { get { return gameOver; } }


    void ResourceSetting()
    {
        gameClear = Resources.Load<AudioClip>("Sounds/GameClear");
        gameOver = Resources.Load<AudioClip>("Sounds/GameOver");
    }
}
