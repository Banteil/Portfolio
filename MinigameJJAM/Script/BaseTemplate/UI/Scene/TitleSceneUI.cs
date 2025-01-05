using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class TitleSceneUI : SceneUI
    {
        #region Cache
        private const string ANDROID_VERSION = "android_version";
        private const string ANDROID_UPDATE_VERSION = "android_update_version";
        private const string IOS_VERSION = "ios_version";
        private const string IOS_UPDATE_VERSION = "ios_update_version";
        private enum TitleImage
        {
            Background,
        }

        private enum TitleText
        {
            TouchScreenText,
        }

        private bool versionCheckComplete;
        private Tween _blinkTween;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            CheckReleaseVersion();
        }

        private async void CheckReleaseVersion()
        {
            if(Application.platform == RuntimePlatform.WindowsEditor)
            {
                versionCheckComplete = true;
                return;
            }
            var marketVersion = "";
            var updateVersion = "";
            var currentVersion = Application.version;
#if UNITY_ANDROID
            marketVersion = await CallAPI.GetAPIKey(ANDROID_VERSION);
            updateVersion = await CallAPI.GetAPIKey(ANDROID_UPDATE_VERSION);
#elif UNITY_IOS
            marketVersion = await CallAPI.GetAPIKey(IOS_VERSION);
            updateVersion = await CallAPI.GetAPIKey(IOS_UPDATE_VERSION);
#else
            versionCheckComplete = true;
            return;
#endif
            if (currentVersion == marketVersion || currentVersion == updateVersion)
                versionCheckComplete = true;
            else
                Manager.UI.ShowPopupUI<OpenMarketPopupUI>();
        }

        protected override void OnStart()
        {
            Manager.Sound.PlayBGM("titlebgm");
        }

        protected override async void BindInitialization()
        {
            await UniTask.WaitUntil(() => versionCheckComplete);
            Bind<Image>(typeof(TitleImage));
            Bind<TextMeshProUGUI>(typeof(TitleText));

            var background = GetImage((int)TitleImage.Background);
            background.gameObject.BindEvent(GoToLobbyScene);

            var touchText = GetText((int)TitleText.TouchScreenText);
            _blinkTween = touchText.DOFade(0, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
        }

        protected override void EscapeAction()
        {
            var message = Util.GetLocalizedString(Define.LOCALIZATION_TABLE_MESSAGE, "quitGame");
            Manager.UI.ShowMessage(message, Util.QuitApplication);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_blinkTween != null)
                _blinkTween.Kill();
        }

        #region BindEvnet
        private void GoToLobbyScene(PointerEventData data)
        {
            if (!Util.CheckNetworkReachability()) return;

            Manager.Sound.PlayOneShotSFX("changeScene");
            Manager.Load.SceneLoad(Define.LOBBY_SCENE_NAME, SceneLoadType.Async);
        }
        #endregion
    }
}
