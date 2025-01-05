using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class OpenMarketPopupUI : PopupUI
    {
        #region Cache
        private const string GOOGLEPLAY_URL = "https://play.google.com/store/apps/details?id=io.starinc.jjam";
        private const string APPSTORE_URL = "https://apps.apple.com/us/app/minigame-jjam/id6737304262";

        private enum OpenMarketButton
        {
            ConfirmButton,
        }
        #endregion

        protected override void BindInitialization()
        {
            base.BindInitialization();
            Bind<Button>(typeof(OpenMarketButton));
            var confirmButton = GetButton((int)OpenMarketButton.ConfirmButton);
            confirmButton.gameObject.BindEvent(OnCloseButtonClicked);
        }

        private void OpenMarketPage()
        {
#if UNITY_ANDROID
            Application.OpenURL(GOOGLEPLAY_URL); // Google Play Store로 이동
#elif UNITY_IOS
            Application.OpenURL(APPSTORE_URL); // Apple App Store로 이동
#else
            Application.OpenURL(GOOGLEPLAY_URL);
#endif
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            OpenMarketPage();
            Util.QuitApplication();
        }
    }
}
