using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Zeus
{
    public class LoadingSceneManager : BaseObject<LoadingSceneManager>
    {
        const string _loadingSceneName = "LoadingScene";
        internal bool SyncComplete => _syncComplete;
        [Header("Loading UI")]
        [SerializeField] Transform _loadingCanvasTr;
        [SerializeField] Image _loadingBaseImage;
        [SerializeField] TextMeshProUGUI _processText;
        [SerializeField] EventSystem _eventSystem;

        bool _isDirecting = false;
        bool _syncComplete = false;

        private void Start()
        {
            if (!SceneLoadManager.HasInstance)
            {
                Debug.LogError("LoadingManager가 존재하지 않습니다.");
                SceneManager.UnloadSceneAsync(_loadingSceneName);
                return;
            }
            _syncComplete = false;
            StartCoroutine(LoadingProcess());
        }

        void CheckSplashDirection()
        {
            //Debug.Log($"Splash Directing : {(SceneLoadManager.SplashData == null ? "null" : SceneLoadManager.SplashData.Name)}");
            _isDirecting = SceneLoadManager.SplashData != null;
            _loadingBaseImage.gameObject.SetActive(!_isDirecting);
            if (_isDirecting)
            {
                _processText.gameObject.SetActive(SceneLoadManager.SplashData.DisplayProcess);

                UnityAction endDirectionHandler = null;
                endDirectionHandler = () =>
                {
                    _isDirecting = false;
                    SplashDirectionManager.Instance.OnEndDirectingEvent -= endDirectionHandler;
                };
                SplashDirectionManager.Instance.OnEndDirectingEvent += endDirectionHandler;
                SplashDirectionManager.Instance.StartDirection(SceneLoadManager.SplashData, _loadingCanvasTr);
            }
            else
                _processText.gameObject.SetActive(true);
        }

        IEnumerator UnloadAllAdditiveScene()
        {
            var count = SceneManager.sceneCount;
            for (int i = 0; i < count; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name.Equals(_loadingSceneName))
                    continue;
                SceneManager.UnloadSceneAsync(scene.name);
            }
            yield return new WaitUntil(() => SceneManager.sceneCount <= 1);
        }

        IEnumerator LoadingProcess()
        {
            //이전 씬 해제 완료까지 대기
            yield return UnloadAllAdditiveScene();

            if (_loadingCanvasTr.gameObject.activeSelf)
                CheckSplashDirection();

            yield return StartCoroutine(AsyncProcess());

            SceneLoadManager.Instance.LoadingSceneLoadComplete();
        }

        float SetPercentage(float value)
        {
            if (_processText.gameObject.activeSelf)
                _processText.text = value.ToString("0") + "%";
            if (value == 100)
                _syncComplete = true;
            return value;
        }

        IEnumerator AsyncProcess()
        {
            _syncComplete = false;
            AsyncOperation async = SceneManager.LoadSceneAsync(SceneLoadManager.NextScene, LoadSceneMode.Additive);
            async.allowSceneActivation = false;

            float percentage = 0;
            float pastTime = 0;
            while (!async.isDone)
            {
                pastTime += Time.deltaTime;

                if (percentage >= 90)
                {
                    percentage = SetPercentage(Mathf.Lerp(percentage, 100, pastTime));
                    if (percentage == 100 && !_isDirecting)
                    {
                        async.allowSceneActivation = true;
                        _eventSystem.gameObject.SetActive(false);
                    }
                }
                else
                {
                    percentage = SetPercentage(Mathf.Lerp(percentage, async.progress * 100f, pastTime));
                    if (percentage >= 90) pastTime = 0;
                }

                yield return null;
            }
        }
    }
}
