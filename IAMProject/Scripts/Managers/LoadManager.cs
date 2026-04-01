using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneNumber
{
    public static int main = 0;
    public static int load = 1;
    public static int town = 2;
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
        asyncLoad.allowSceneActivation = true;
    }
}
