using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace starinc.io.kingnslave
{
    public class LoadingSceneManager : Singleton<LoadingSceneManager>
    {
        private const float SINGLE_ENTRANCE_DELAY = 2f;
        private const float MAX_WAITING_TIME = 20f;

        [SerializeField] private GameObject titleSceneUI;
        [SerializeField] private SlicedFilledImage progressFill;

        private bool enableVersionCheck = true;

        public bool IsLoadingDone { get; set; } = false;
        private bool IsNetworkGameEntry
        {
            get
            {
                return GameManager.Instance.IsNetworkGameMode() && NetworkManager.Instance.MyRunner != null && !NetworkManager.Instance.MyRunner.IsShutdown;
            }
        }

        async private void Start()
        {
            if (!GameManager.HasInstance)
            {
                Debug.LogError("GameManager가 존재하지 않습니다.");
                await SceneManager.UnloadSceneAsync(Define.LoadingSceneName);
                return;
            }

            if (UserDataManager.Instance.MyData != null)
            {
                await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, Define.CDKey.enable_version_check.ToString(), (obj) =>
                {
                    if (obj != null)
                    {
                        var value = (Define.Boolean)Convert.ToInt32(obj);
                        enableVersionCheck = value == Define.Boolean.TRUE;
                        Debug.Log($"enableVersionCheck : {enableVersionCheck}");
                    }
                });
            }
            LoadingProcess();
        }

        async private void ShowMarketProcess()
        {
            var eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            eventSystem.AddComponent<BaseInput>();

            var yonUI = FindObjectOfType<UIYesOrNo>();
            await UniTask.WaitUntil(() => yonUI == null);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        async private UniTask<bool> CheckVersionProcess()
        {
            var currentVersion = Application.version;
            var sameVersion = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log("Running on Android!");
            var keyName = Define.CDKey.client_version_android.ToString();
            await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, keyName, (version) =>
            {
                sameVersion = currentVersion == (string)version;
            });
            if (!sameVersion)
            {
                var yonUI = UIManager.Instance.ShowYesOrNoUI(Util.GetLocalizationTableString(Define.InfomationLocalizationTable, "newVersionText"), () =>
                {
                    Application.OpenURL("market://details?id=io.starinc.kingnslave");
                    //Application.OpenURL("https://m.onestore.co.kr/mobilepoc/apps/appsDetail.omp?prodId=0000774329");
                });
                var canvas = yonUI.gameObject.GetComponent<Canvas>();
                canvas.sortingOrder = 9999;
                return false;
            }
#elif UNITY_IOS && !UNITY_EDITOR
            Debug.Log("Running on iOS!");
            var keyName = Define.CDKey.client_version_ios.ToString();
            await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, keyName, (version) =>
            {
                sameVersion = currentVersion == (string)version;
            });
            if (!sameVersion)
            {
                var yonUI = UIManager.Instance.ShowYesOrNoUI(Util.GetLocalizationTableString(Define.InfomationLocalizationTable, "newVersionText"), () =>
                {
                    Application.OpenURL("itms-apps://itunes.apple.com/app/id1234567890");
                });         
                var canvas = yonUI.gameObject.GetComponent<Canvas>();
                canvas.sortingOrder = 9999;
                return false;
            }
#else
            Debug.Log("Running on other platform!");
            var keyName = Define.CDKey.client_version_android.ToString();
            await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, keyName, (version) =>
            {
                sameVersion = currentVersion == (string)version;
            });
            //if (!sameVersion)
            //{
            //    var yonUI = UIManager.Instance.ShowYesOrNoUI(Util.GetLocalizationTableString(Define.InfomationLocalizationTable, "newVersionText"), () =>
            //    {
            //        Application.OpenURL("https://play.google.com/store/apps/details?id=io.starinc.kingnslave");
            //    });
            //    var canvas = yonUI.gameObject.GetComponent<Canvas>();
            //    canvas.sortingOrder = 9999;
            //    return false;
            //}
#endif 
            GameManager.Instance.CheckCurrentVersion = true;
            return true;
        }

        private async UniTask UnloadAllAdditiveScene()
        {
            var count = SceneManager.sceneCount;
            var sceneQueue = new Queue();
            for (int i = 0; i < count; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name.Equals(Define.LoadingSceneName))
                    continue;
                sceneQueue.Enqueue(scene);
            }

            while (sceneQueue.Count > 0)
            {
                var scene = (Scene)sceneQueue.Dequeue();
                await SceneManager.UnloadSceneAsync(scene);
            }
        }

        async private void LoadingProcess()
        {
            await UnloadAllAdditiveScene();

            if (enableVersionCheck && !GameManager.Instance.CheckCurrentVersion)
            {
                var check = await CheckVersionProcess();
                if (!check)
                {
                    ShowMarketProcess();
                    return;
                }
            }

            var async = SceneManager.LoadSceneAsync(GameManager.Instance.NextScene, LoadSceneMode.Additive);
            async.allowSceneActivation = false;
            var timer = 0f;
            while (!async.isDone)
            {
                await UniTask.Yield();
                timer += Time.deltaTime;
                if (async.progress < 0.9f)
                {
                    progressFill.fillAmount = Mathf.Lerp(progressFill.fillAmount, async.progress, timer);
                    if (progressFill.fillAmount >= async.progress)
                    {
                        timer = 0f;
                    }
                }
                else
                {
                    progressFill.fillAmount = Mathf.Lerp(progressFill.fillAmount, 1f, timer);
                    if (progressFill.fillAmount == 1.0f)
                    {
                        // 싱글 게임일 때 로딩 씬에서 잠시 대기
                        if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory)
                            await UniTask.WaitForSeconds(SINGLE_ENTRANCE_DELAY, true);

                        Debug.Log("로딩 프로세스 완료!");
                        async.allowSceneActivation = true;
                        break;
                    }
                }
            }
            await async;

            var loadingCompletedNormally = true;
            if (IsNetworkGameEntry)
                loadingCompletedNormally = await NetworkLoadingProcess();

            GameManager.Instance.IsLoading = false;
            if (loadingCompletedNormally)
            {
                await SceneManager.UnloadSceneAsync(Define.LoadingSceneName);
                ScreenTransitionManager.Instance.CloseDirection();
            }
            else
            {
                await NetworkManager.Instance.Shutdown();

                GameManager.Instance.ClearGame();
                ScreenTransitionManager.Instance.CloseDirection();
                SceneManager.LoadScene(Define.LobbySceneName);
            }
        }

        async private UniTask<bool> NetworkLoadingProcess()
        {
            float timer = MAX_WAITING_TIME;
            NetworkManager.RPC_LoadSceneDone(NetworkManager.Instance.MyRunner);
            while (!IsLoadingDone)
            {
                timer -= Time.deltaTime;
                if (timer <= 0 && !IsLoadingDone)
                {
                    Debug.Log($"Loading {IsLoadingDone}");
                    return false;
                }
                await UniTask.Yield();
            }
            Debug.Log($"Loading {IsLoadingDone}");
            return true;
        }
    }
}
