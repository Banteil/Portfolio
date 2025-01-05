using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace starinc.io.gallaryx
{
    public class MainSceneManager : Singleton<MainSceneManager>
    {
        #region Cache
        private const string _exhibitionName = "Exhibition_";
        private const string _playerName = "Player";
        private ExhibitionHall _exhibition;
        private CharacterController _player;
        private UIControlGuidePopup _guidePopup;
        #endregion

        public ExhibitionHall Exhibition { get { return _exhibition; } }
        public CharacterController Player { get { return _player; } }
        public Action ShowGuideCallback;

        protected void Start()
        {
            Initialize();
        }

        private async void Initialize()
        {
            await SpawnRequiredObjects();
            if (GameManager.Instance.IsLoading)
                LoadingSceneProcessor.Instance.LoadingFinished();
            InputManager.Instance.KeyInputEvent.InputUpCallback += ShowGuideUI;
        }

        public async UniTask SpawnRequiredObjects()
        {
            try
            {
                await SpawnExhibition();
                await ExhibitsSettingProcess();
                SpawnPlayer();
                ShowMainSceneUI();
                GameManager.Instance.RequireObjectsSpawnCallback?.Invoke();
                GameManager.Instance.RequireObjectsSpawnCallback = null;
                Debug.Log("End Spawn Required Objects!");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                GameManager.Instance.RequireObjectsSpawnCallback = null;
            }
        }

        private void ShowMainSceneUI()
        {
            UIManager.Instance.ShowSceneUI<UIExhibitionScene>("ExhibitionSceneUI");
            UIManager.Instance.ShowGlobalUI<UIGuideGlobal>("GuideUI");
            _guidePopup = UIManager.Instance.ShowPopupUI<UIControlGuidePopup>("ControlGuideUI");
            Debug.Log("Main Scene UI Setting Completed");
        }

        private async UniTask SpawnExhibition()
        {
            var exhibitionData = await CallAPI.GetExhibitionData(GameManager.Instance.Seq);
            GameManager.Instance.ExhibitionUID = exhibitionData != null ? exhibitionData.uid : "guest";
            GameManager.Instance.ExhibitionSeq = exhibitionData != null ? exhibitionData.exhibition_seq : 1;

            var assetType = Util.IsMobileWebPlatform ? "Mobile" : "PC";
            var exhibitionSeq = GameManager.Instance.ExhibitionSeq;
            var sceneKey = $"{assetType}{_exhibitionName}{exhibitionSeq}";

            try
            {
                var checkForUpdateHandle = Addressables.CheckForCatalogUpdates();
                await checkForUpdateHandle;
                if (checkForUpdateHandle.Status == AsyncOperationStatus.Succeeded && checkForUpdateHandle.Result.Count > 0)
                {
                    // 업데이트된 카탈로그가 있을 경우 업데이트 진행
                    Debug.Log("Updating catalog...");
                    var updateHandle = Addressables.UpdateCatalogs(checkForUpdateHandle.Result);
                    await updateHandle;

                    if (updateHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log("Catalog updated successfully.");
                    }
                    else
                    {
                        Debug.LogError("Catalog update failed.");
                    }
                }
                else
                {
                    Debug.Log("No catalog updates available.");
                }
            
                var loadOperation = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive);
                await loadOperation.Task;

                if (loadOperation.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new System.Exception("Scene load failed.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading scene: {ex.Message}");
            }

            var obj = GameObject.FindWithTag("Exhibition");
            if (obj != null)
            {
                _exhibition = obj.GetComponent<ExhibitionHall>();
                Util.ExcludingCloneName(_exhibition.gameObject);

                _exhibition.SetExhibitionData(exhibitionData);
                Debug.Log("Spawn Exhibition Completed");
            }
            else
            {
                Debug.LogError("Exhibition not found in the loaded scene");
            }
        }

        public async UniTask ExhibitsSettingProcess()
        {
            try
            {
                var mediaDatas = await CallAPI.GetMediaData(GameManager.Instance.Seq);
                await _exhibition.SetExhibitsData(mediaDatas);
                Debug.Log("Exhibits Setting Completed");
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void SpawnPlayer()
        {
            var obj = GameObject.FindWithTag("Player");
            if (obj == null)
                obj = ResourceManager.Instance.Instantiate($"Character/{_playerName}");
            if (obj == null)
            {
                Debug.LogError("플레이어 객체가 생성되지 않았습니다.");
                return;
            }

            _player = obj.GetComponent<CharacterController>();
            Util.ExcludingCloneName(_player.gameObject);
            _player.transform.SetPositionAndRotation(_exhibition.GetStartPos(), _exhibition.GetStartRot());
            _player.Type = CharacterType.Player;
            Debug.Log("Spawn Player Completed");
        }

        private void ShowGuideUI()
        {
            if (Util.IsUIFocusing) return;
            if (InputManager.Instance.KeyInputEvent.InputGuideUIKey() == InputState.Up)
            {
                if(_guidePopup == null)
                {
                    _guidePopup = UIManager.Instance.ShowPopupUI<UIControlGuidePopup>("ControlGuideUI");
                    ShowGuideCallback?.Invoke();
                }
                else
                {
                    _guidePopup.ClosePopup();
                    _guidePopup = null;
                }
            }
        }

        public void ShowGuide()
        {
            if (_guidePopup == null)
            {
                _guidePopup = UIManager.Instance.ShowPopupUI<UIControlGuidePopup>("ControlGuideUI");
                ShowGuideCallback?.Invoke();
            }
            else
            {
                _guidePopup.ClosePopup();
                _guidePopup = null;
            }
        }
    }
}