using DG.Tweening;
using TMPro;
using UnityEngine;
namespace Zeus
{
    public class PlayerPauseUI : MonoBehaviour
    {
        private CanvasGroup _canvas;

        [Header("MENU UI")]
        public VerticalPauseMenuUI PauseMenuUI;
        public VerticalPauseMenuUI LoadUI;
        public VerticalPauseMenuUI ControllerUI;
        internal VerticalPauseMenuUI FocusUI;

        private void Start()
        {
            _canvas = GetComponent<CanvasGroup>();
            //MenuUIInit();
        }

        //private void OnEnable()
        //{
        //    InputReader.Instance.CallExitUI += Toggle;
        //    InputReader.Instance.CallCancel += CancelFocusUI;

        //}
        //private void OnDisable()
        //{
        //    if (InputReader.ApplicationIsQuitting == false)
        //    {
        //        InputReader.Instance.CallExitUI -= Toggle;
        //        InputReader.Instance.CallCancel -= CancelFocusUI;
        //    }
        //}

        public void ToggleUI()
        {
            if (_canvas == null)
            {
                Debug.LogError("캔버스 그룹이 존재하지 않습니다");
                return;
            }
            if (_canvas.alpha == 0)
            {
                Init();
                MenuUIInit();
                GameTimeManager.Instance.Pause(true);
                var tween = _canvas.DOFade(1, 0.1f).Pause();
                tween.timeScale = DOTween.unscaledTimeScale / DOTween.timeScale;
                tween.Play();
                //InputReader.Instance.EnableMapUI(true, true);
                InputReader.Instance.EnableActionMap(TypeInputActionMap.UI);

                InputReader.Instance.CallExitUI += ToggleUI;
                InputReader.Instance.CallCancel += CancelFocusUI;
            }
            else
            {
                GameTimeManager.Instance.Pause(false);
                var tween = _canvas.DOFade(0, 0.1f).Pause();
                tween.timeScale = DOTween.unscaledTimeScale / DOTween.timeScale;
                tween.Play();
                //InputReader.Instance.EnableMapPlayerControls(true, false);
                //InputReader.Instance.EnableMapBattleMod(true, false);
                InputReader.Instance.EnableActionMap(TypeInputActionMap.BATTLE);
                PauseMenuUI.EnableUI(false);
                LoadUI.EnableUI(false);

                if (InputReader.ApplicationIsQuitting == false)
                {
                    InputReader.Instance.CallExitUI -= ToggleUI;
                    InputReader.Instance.CallCancel -= CancelFocusUI;
                }
            }
        }

        private void Init()
        {
            var playerData = TableManager.CurrentPlayerData;
            if (playerData == null)
            {
                Debug.LogError("플레이어 세이브 데이터가 없습니다");
                return;
            }

            var goldIU = PlayerUIManager.Get().GetUI<PlayerGoodsTypeUI>(TypePlayerUI.GOLD);
            if (goldIU != null)
            {
                goldIU.SetAmount(playerData.Coins);
            }
        }

        private void MenuUIInit()
        {
            if (PauseMenuUI == null)
            {
                return;
            }

            if (FocusUI != null && FocusUI != PauseMenuUI)
            {
                FocusUI.IsFocus = false;
                FocusUI.InitIndex();
                var canvas = FocusUI.GetComponent<CanvasGroup>();
                if (canvas != null)
                {
                    FocusUI.GetComponent<CanvasGroup>().alpha = 0;
                }
            }

            PauseMenuUI.IsFocus = true;
            PauseMenuUI.InitIndex();
            FocusUI = PauseMenuUI;
        }

        private void CancelFocus()
        {
            FocusUI.IsFocus = false;
            FocusUI.InitIndex();
            var canvas = FocusUI.GetComponent<CanvasGroup>();
            if (canvas != null)
            {
                FocusUI.GetComponent<CanvasGroup>().alpha = 0;
            }
            PauseMenuUI.IsFocus = true;
            FocusUI = PauseMenuUI;
        }

        private void CancelFocusUI()
        {
            //PAUSE UI
            if (FocusUI != PauseMenuUI)
            {
                CancelFocus();
            }
        }

        public void CheckPoint()
        {
            //Debug.Log("체크 포인트 시작");
            var bossUI = PlayerUIManager.Get().GetUI<PlayerUIType>(TypePlayerUI.BOSSHP);
            bossUI.SetVisible(false, 0.01f);
            PlayerUIManager.Get().OnCheckPointStart();
            ToggleUI();
            //InputReader.Instance.EnableMapPlayerControls(false, true);
            InputReader.Instance.EnableActionMap(TypeInputActionMap.NONE);
        }

        public void SaveGame()
        {
            TableManager.AutoSave();
        }

        public void KeyExplanation()
        {
            PauseMenuUI.IsFocus = false;
            ControllerUI.GetComponent<CanvasGroup>().alpha = 1;
            ControllerUI.IsFocus = true;

            FocusUI = ControllerUI;
        }

        public void LoadGame()
        {
            if (LoadUI == null) { return; }
            PauseMenuUI.IsFocus = false;
            LoadUI.GetComponent<CanvasGroup>().alpha = 1;
            LoadUI.IsFocus = true;

            FocusUI = LoadUI;
            LoadSaveData();
            SoundManager.Instance.BGMFade(true, 1f);
        }

        private void LoadSaveData()
        {
            //리스트 텍스트 초기화
            var lists = LoadUI.GetUIList();

            for (int i = 0; i < lists.Count; i++)
            {
                lists[i].SetText(100007);
                if (lists[i].DataInfoText != null)
                {
                    lists[i].DataInfoText.SetActive(false);
                }
            }

            var data = TableManager.GetSaveDatas().PlayerDatas;

            for (int i = 0; i < data.Count; i++)
            {
                var sceneName = TableManager.CurrentPlayerData.SaveZoneData.SceneName;
                var zoneName = SceneLoadManager.GetSceneNameID(sceneName);
                if (lists[i].DataInfoText != null)
                {
                    lists[i].DataInfoText.SetActive(true);
                }
                lists[i].SetText(zoneName);
            }
        }

        public void GoToTitle()
        {
            if (SceneLoadManager.Instance.LoadScene("TitleScene"))
            {
                ToggleUI();
                SoundManager.Instance.BGMFade(true, 1f);
            }
        }

        public void LoadGame(int index)
        {
            var loadSceneNamae = TableManager.CurrentPlayerData.SaveZoneData.SceneName;
            if (string.IsNullOrEmpty(loadSceneNamae))
            {
                Debug.LogError("TableManager.CurrentPlayerData.SaveZoneData.SceneName is Null");
                loadSceneNamae = "3.0_Scene";
            }

            if (SceneLoadManager.Instance.LoadScene(loadSceneNamae, null, 1, 1, false))
            {
                TableManager.IsNewGame = false;
                ToggleUI();
                SoundManager.Instance.BGMFade(true, 1f);
            }
        }
    }
}