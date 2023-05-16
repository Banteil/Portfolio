using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

///<summary>
///게임 시작시 보여지는 타이틀 화면을 담당하는 클래스
///</summary>
public class TitleManager : Singleton<TitleManager>
{
    public GameObject quitPanel;
    public GameObject newGameButton, loadGameButton;
    public RawImage fadeImage;
    bool fileNeverWritten;

    void Awake()
    {
        Application.targetFrameRate = 30; //30프레임 고정
        Screen.SetResolution(2560, 1440, true); //스크린 해상도를 2560, 1440으로 맞춤
        fadeImage.color = new Color32(0, 0, 0, 255);
    }

    void Start()
    {
        StartCoroutine(EffectManager.Instance.FadeIn(fadeImage, 1f));
        StartCoroutine(GoogleLoginCheck());
        GooglePlayServiceManager.Instance.LoadFromCloud((string dataToLoad) =>
        {
            fileNeverWritten = (dataToLoad.Length.Equals(0) || dataToLoad.Equals(null));
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndGame();
        }
    }

    IEnumerator GoogleLoginCheck()
    {        
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            newGameButton.SetActive(true);
            loadGameButton.SetActive(true);
            yield break;
        }

        while (!GooglePlayServiceManager.Instance.isAuthenticated) yield return null;
        newGameButton.SetActive(true);
        if(!fileNeverWritten) loadGameButton.SetActive(true);
    }

    ///<summary>
    ///새로운 게임을 시작하는 함수
    ///</summary>
    public void NewGame()
    {
        StartCoroutine(SceneMove(0));
    }

    ///<summary>
    ///기존 데이터를 로드하여 게임을 이어서 진행하는 함수
    ///</summary>
    public void LoadGame()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            StartCoroutine(SceneMove(1));
            return;
        }

        GooglePlayServiceManager.Instance.LoadFromCloud((string dataToLoad) =>
        {
            PlayerState.Instance.LoadStateData(dataToLoad);
        });

        StartCoroutine(SceneMove(1));        
    }

    public void EndGame()
    {
        quitPanel.SetActive(true);
    }

    public void QuitButton()
    {
        StartCoroutine(SceneMove(2));
    }

    public void CancleButton()
    {
        quitPanel.SetActive(false);
    }

    IEnumerator SceneMove(int info)
    {
        yield return StartCoroutine(EffectManager.Instance.FadeOut(fadeImage, 1f));

        switch(info)
        {
            case 0:
                SceneManager.LoadScene("DungeonScene");
                break;
            case 1:
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    SceneManager.LoadScene("TownScene");
                    yield break;
                }

                while (GooglePlayServiceManager.Instance.isProcessing) yield return null;
                SceneManager.LoadScene("TownScene");
                break;
            case 2:
                Application.Quit();
                break;
        }
    }
}

