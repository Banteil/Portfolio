using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadManager : DontDestorySingleton<SceneLoadManager>
{
    [SerializeField]
    GameObject _loadingUI;
    [SerializeField]
    Image _progressBar;

    bool _isLoading;
    public bool IsLoading { get { return _isLoading; } }

    public void LoadScene(SceneData sceneData) => StartCoroutine(LoadAsyncProcess(sceneData));

    IEnumerator LoadAsyncProcess(SceneData sceneData)
    {
        _isLoading = true;
        _loadingUI?.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneData.SceneID);
        asyncLoad.allowSceneActivation = false;
        float timer = 0.0f;

        // Scene을 불러오는 것이 완료되면, AsyncOperation은 isDone 상태가 된다.
        while (!asyncLoad.isDone)
        {
            timer += Time.deltaTime;
            if (asyncLoad.progress < 0.9f)
            {
                _progressBar.fillAmount = Mathf.Lerp(_progressBar.fillAmount, asyncLoad.progress, timer);
                if (_progressBar.fillAmount >= asyncLoad.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                _progressBar.fillAmount = Mathf.Lerp(_progressBar.fillAmount, 1f, timer);
                if (_progressBar.fillAmount == 1.0f)
                {
                    break;
                }
            }
            yield return null;
        }

        //로딩 된 씬이 3D인지 아닌지 여부에 따라 렌더링 방식 변경
        UniversalAdditionalCameraData urpCamera = Camera.main.GetComponent<UniversalAdditionalCameraData>();
        if (sceneData.Is3D)
            urpCamera.SetRenderer(1);
        else
            urpCamera.SetRenderer(0);

        asyncLoad.allowSceneActivation = true;
        _isLoading = false;
        _loadingUI?.SetActive(false);
    }
}

