using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    private static MainManager instance = null;
    public static MainManager Instance
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
            SoundManager.Instance.BGMPlay(Resources.Load<AudioClip>("Sounds/MainBGM"));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        GameManager.Instance.CallLoadScene(SceneNumber.lobby);
    }
}
