using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace starinc.io
{
    public enum SceneLoadType
    {
        Async,
        AddressableAsync
    }

    public enum SceneLoadEffect
    {
        Fade,
    }

    public class SceneLoadManager : BaseManager
    {
        #region Cache
        private const int LOADING_ORDER = 3000;
        private const int MAX_LOADING_TEXT_COUNT = 3;

        public bool IsLoading { get; set; } = false;

        private PrefabObjectTable _loadingObjectTable;
        private AsyncOperationHandle _handle;
        private GameObject _loadingUI;
        #endregion

        #region Callback
        public event Action OnSceneLoadProcessStarted;
        public event Action OnNextSceneLoadCompleted;
        public event Action OnSceneLoadProcessCompleted;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            _loadingObjectTable = Resources.Load<PrefabObjectTable>("LoadingObjectTable");
            LoadingUIInitialization();
        }

        private void LoadingUIInitialization()
        {
            _loadingUI = new GameObject("LoadingUI");
            var canvas = Util.GetOrAddComponent<Canvas>(_loadingUI);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = LOADING_ORDER;

            var canvasScaler = Util.GetOrAddComponent<CanvasScaler>(_loadingUI);
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(Define.REFERENCE_WIDTH, Define.REFERENCE_HEIGHT);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            Util.GetOrAddComponent<GraphicRaycaster>(_loadingUI);
            _loadingUI.transform.SetParent(transform);
            _loadingUI.SetActive(false);
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        public async void SceneLoad(string sceneName, SceneLoadType sceneLoadType, SceneLoadEffect effect = SceneLoadEffect.Fade)
        {
            if (IsLoading)
            {
                Debug.LogWarning("Now Loading");
                return;
            }
            IsLoading = true;
            OnSceneLoadProcessStarted?.Invoke();
            try
            {               
                var prevScene = SceneManager.GetActiveScene();
                var prevHandle = _handle;

                var effectObject = _loadingObjectTable.GetPrefabObject($"{effect}UI", _loadingUI.transform);
                var transition = effectObject.GetComponent<LoadingTransition>();
                transition.SetLoadingText(Util.GetLocalizedString(Define.LOCALIZATION_TABLE_MESSAGE, $"loading_{UnityEngine.Random.Range(0, MAX_LOADING_TEXT_COUNT)}"));
                transition.OnLoadingEndComplete += LoadingEndComplete;
                transition.OnLoadingStartComplete += async () =>
                {
                    //임시 로딩 씬 호출
                    await SceneManager.LoadSceneAsync(Define.LOAD_SCENE_NAME, LoadSceneMode.Additive);
                    //이전 씬 언로드
                    if (prevHandle.IsValid())
                        await Addressables.UnloadSceneAsync(prevHandle).Task;
                    else
                        await SceneManager.UnloadSceneAsync(prevScene);

                    //다음 씬 로드
                    switch (sceneLoadType)
                    {
                        case SceneLoadType.Async:
                            {
                                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                            }
                            break;
                        case SceneLoadType.AddressableAsync:
                            {
                                var checkHandle = Addressables.LoadResourceLocationsAsync(sceneName, typeof(SceneInstance));
                                await checkHandle.Task;
                                if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
                                {
                                    _handle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                                    await _handle.Task;

                                    if (_handle.Status != AsyncOperationStatus.Succeeded)
                                    {
                                        Debug.LogError("Failed to load scene: " + sceneName);
                                        FailLoading(effectObject);
                                        return;
                                    }
                                }
                                else
                                {
                                    // 유효하지 않은 씬 이름
                                    Debug.LogError("Invalid scene name: " + sceneName);
                                    FailLoading(effectObject);
                                    return;
                                }
                            }
                            break;
                    }
                    OnNextSceneLoadCompleted?.Invoke();

                    await SceneManager.UnloadSceneAsync(Define.LOAD_SCENE_NAME);
                    await transition.PlayLoadingEndAsync();
                };
                _loadingUI.SetActive(true);
                await transition.PlayLoadingStartAsync();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                IsLoading = false;
            }
        }

        private void LoadingEndComplete()
        {            
            _loadingUI.SetActive(false);
            OnSceneLoadProcessCompleted?.Invoke();
            IsLoading = false;
        }

        private void FailLoading(GameObject effectObj)
        {
            Destroy(effectObj);
            _loadingUI.SetActive(false);
            IsLoading = false;
            ReloadCurrentScene();
        }

        public void ReloadCurrentScene()
        {
            var isAddressableScene = Manager.UI.SceneUI.IsAddressableScene;
            var sceneLoadType = isAddressableScene ? SceneLoadType.AddressableAsync : SceneLoadType.Async;
            var sceneName = SceneManager.GetActiveScene().name;
            SceneLoad(sceneName, sceneLoadType);
        }
    }    
}
