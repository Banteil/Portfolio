using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class SceneNumber
{
    public static int main = 0;
    public static int load = 1;
    public static int lobby = 2;
    public static int battle = 3;
}


public class LoadManager : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI TipText;
    [SerializeField]
    Image progressImage;
    [SerializeField]
    TextMeshProUGUI progressPercentText;

    private void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadAsyncProcess(GameManager.Instance.MovingSceneIndex));
    }

    IEnumerator LoadAsyncProcess(int sceneNum)
    {
        GameManager.Instance.IsLoading = true;
        SoundManager.Instance.BGMPlayer.Stop();
        SoundManager.Instance.SFXPlayer.Stop();
        yield return new WaitForSeconds(0.1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNum);
        asyncLoad.allowSceneActivation = false;
        float timer = 0.0f;
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
                }
            }
            else
            {
                progressImage.fillAmount = Mathf.Lerp(progressImage.fillAmount, 1f, timer);
                if (progressImage.fillAmount == 1.0f)
                {
                    break;
                }
            }
            progressPercentText.text = (asyncLoad.progress * 100).ToString("0") + "%";
            yield return null;
        }

        progressPercentText.text = "100%";
        GameManager.Instance.IsLoading = false;        
        asyncLoad.allowSceneActivation = true;
    }
}
