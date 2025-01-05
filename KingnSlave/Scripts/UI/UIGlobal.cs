using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIGlobal : UIBase
    {
        private Canvas canvas;
        public int GlobalOrder { get { return canvas.sortingOrder; } }

        private enum GlobalButtons
        {
            SettingButton,
            NoticeButton,
        }

        private void Awake() => Initialized();

        protected override void InitializedProcess()
        {
            if (FindObjectsOfType<UIGlobal>().Length > 1)
            {
                Debug.LogWarning("글로벌 UI가 이미 존재합니다.");
                Destroy(gameObject);
                return;
            }
            Util.DontDestroyObject(gameObject);
            canvas = GetComponent<Canvas>();
            UIManager.Instance.SetGlobalUI(gameObject);

            Bind<Button>(typeof(GlobalButtons));
            var settingButton = GetButton((int)GlobalButtons.SettingButton);
            settingButton.gameObject.BindEvent(ShowSettingPopup);
            var noticeButton = GetButton((int)GlobalButtons.NoticeButton);
            noticeButton.gameObject.BindEvent(ShowNotice);

            SceneManager.sceneLoaded += SceneLoadedCallback;
        }

        /// <summary>
        /// 세팅 팝업 UI 오픈 함수
        /// </summary>
        private void ShowSettingPopup(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(29);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            UIManager.Instance.ShowUI<UISetting>("SettingPopupUI");
        } 
        
        private void ShowNotice(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(40);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
#if UNITY_EDITOR
            Debug.Log("공지사항 버튼");
#else
            CallWebView.ShowUrlFullScreen(Define.NoticeURL, "noticeWebViewTitle");
#endif
        }

        private void SceneLoadedCallback(Scene scene, LoadSceneMode loadSceneMode)
        {
            var isLobby = scene.name == Define.LobbySceneName;
            GetButton((int)GlobalButtons.NoticeButton).gameObject.SetActive(isLobby);
        }
    }
}
