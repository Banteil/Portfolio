using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField]
    GameObject startButton;
    [SerializeField]
    Animation leftAnim, rightAnim;
    [SerializeField]
    AudioClip titleBGM;

    private void Start()
    {
        SoundManager.Instance.BGM.clip = titleBGM;
        SoundManager.Instance.BGM.Play();
    }

    public void StartGame()
    {
        StartCoroutine(StartGameDirection());
    }

    IEnumerator StartGameDirection()
    {
        startButton.gameObject.SetActive(false);
        leftAnim.Play();
        rightAnim.Play();
        while(leftAnim.isPlaying || rightAnim.isPlaying) { yield return null; }

        GameManager.Instance.CallLoadScene(SceneNumber.dungeon);        
    }
}
