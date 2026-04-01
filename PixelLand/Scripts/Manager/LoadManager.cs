using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneNumber
{
    public static int main = 0;
    public static int load = 1;
    public static int village = 2;
    public static int dungeon = 3;    
}


public class LoadManager : MonoBehaviour
{
    [SerializeField]
    GameObject loadingUI;
    [SerializeField]
    Image progressImage;

    private void Start()
    {
        StartCoroutine(LoadAsyncProcess(GameManager.Instance.MovingSceneIndex));
    }

    IEnumerator LoadAsyncProcess(int sceneNum)
    {
        GameManager.Instance.IsLoading = true;
        if (!GameManager.Instance.PortalMove)
        {
            SoundManager.Instance.BGM.Stop();
            SoundManager.Instance.SFX.Stop();
            loadingUI.SetActive(true);
            yield return new WaitForSeconds(0.1f);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNum);
            asyncLoad.allowSceneActivation = false;
            float timer = 0.0f;

            Debug.Log("로딩 세팅");
            // Scene을 불러오는 것이 완료되면, AsyncOperation은 isDone 상태가 된다.
            while (!asyncLoad.isDone)
            {
                timer += Time.deltaTime;
                if (asyncLoad.progress < 0.9f)
                {
                    progressImage.fillAmount = Mathf.Lerp(progressImage.fillAmount, asyncLoad.progress, timer);
                    if (progressImage.fillAmount >= asyncLoad.progress)
                    {
                        timer = 0f;
                        Debug.Log("프로그레스 세팅 중");
                    }
                }
                else
                {
                    progressImage.fillAmount = Mathf.Lerp(progressImage.fillAmount, 1f, timer);
                    if (progressImage.fillAmount == 1.0f)
                    {
                        Debug.Log("프로그레스 완료");
                        break;
                    }
                }
                yield return null;
            }
            
            GameManager.Instance.IsLoading = false;
            if (sceneNum.Equals(SceneNumber.main))
            {
                //UIManager.Instance..SetActive(false);
                //UIManager.Instance.MiddleUI.SetActive(false);
                //UIManager.Instance.TopUI.SetActive(false);
            }
            else
            {
                //UIManager.Instance.BottomUI.SetActive(true);
                //UIManager.Instance.MiddleUI.SetActive(true);
                //UIManager.Instance.TopUI.SetActive(true);
            }

            asyncLoad.allowSceneActivation = true;
        }
        else
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNum);
            asyncLoad.allowSceneActivation = false;

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(UIManager.Instance.FadeOut());
            while (asyncLoad.progress < 0.9f) { yield return null; }

            GameManager.Instance.IsLoading = false;
            if (sceneNum.Equals(SceneNumber.main))
            {
                //UIManager.Instance.BottomUI.SetActive(false);
                //UIManager.Instance.MiddleUI.SetActive(false);
                //UIManager.Instance.TopUI.SetActive(false);
            }
            else
            {
                //UIManager.Instance.BottomUI.SetActive(true);
                //UIManager.Instance.MiddleUI.SetActive(true);
                //UIManager.Instance.TopUI.SetActive(true);
            }

            GameManager.Instance.Player.OutPortal();
            StartCoroutine(UIManager.Instance.FadeIn());
            asyncLoad.allowSceneActivation = true;            
        }
    }
}
