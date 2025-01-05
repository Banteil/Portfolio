using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class PausePopupUI : PopupUI
    {
        #region Cache
        private enum PauseButton
        {
            SettingButton,
            ContinueButton,
            RetryButton,
            ExitButton,
        }
        #endregion

        protected override void BindInitialization()
        {
            Bind<Button>(typeof(PauseButton));

            var settingButton = GetButton((int)PauseButton.SettingButton);
            settingButton.gameObject.BindEvent(OnSettingsButton);
            var continueButton = GetButton((int)PauseButton.ContinueButton);
            continueButton.gameObject.BindEvent(OnCloseButtonClicked);
            var retryButton = GetButton((int)PauseButton.RetryButton);
            retryButton.gameObject.BindEvent(OnRetryButton); 
            var exitButton = GetButton((int)PauseButton.ExitButton);
            exitButton.gameObject.BindEvent(OnExitButton);
        }

        #region BindEvnet   
        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            Util.UnPause();
            base.OnCloseButtonClicked(data);
        }

        private void OnSettingsButton(PointerEventData data)
        {
            Manager.UI.ShowPopupUI<SettingPopupUI>();
        }

        private void OnRetryButton(PointerEventData data)
        {
            Time.timeScale = 1.0f;
            Manager.Sound.PlayOneShotSFX("changeScene");
            Manager.Load.ReloadCurrentScene();
        }

        private void OnExitButton(PointerEventData data)
        {
            Time.timeScale = 1.0f;
            Manager.Sound.PlayOneShotSFX("changeScene");
            Manager.Load.SceneLoad(Define.LOBBY_SCENE_NAME, SceneLoadType.Async);
        }
        #endregion
    }
}
