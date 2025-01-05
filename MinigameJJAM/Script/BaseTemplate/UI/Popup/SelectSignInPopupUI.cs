using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class SelectSignInPopupUI : PopupUI
    {
        #region Cache
        private enum SignInButton
        {
            BackButton,
            EmailButton,
            GoogleButton,
            AppleButton,
        }
        #endregion

        protected override void BindInitialization()
        {
            Bind<Button>(typeof(SignInButton));

            var backButton = GetButton((int)SignInButton.BackButton);
            backButton.gameObject.BindEvent(OnCloseButtonClicked);
            var emailButton = GetButton((int)SignInButton.EmailButton);
            emailButton.gameObject.BindEvent(OnEmailButton);
            var googleButton = GetButton((int)SignInButton.GoogleButton);
            googleButton.gameObject.BindEvent(OnGoogleButton);
            var appleButton = GetButton((int)SignInButton.AppleButton);
            appleButton.gameObject.BindEvent(OnAppleButton);

#if UNITY_ANDROID
            appleButton.gameObject.SetActive(false);
#endif
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        #region BindEvnet
        private void OnEmailButton(PointerEventData data)
        {
            Debug.Log("Login Email!");
            //Manager.User.SignInWithEmail();
        }

        private void OnGoogleButton(PointerEventData data)
        {
            Debug.Log("Login Google!");
            //Manager.User.SignInWithGoogle();
        }
        private void OnAppleButton(PointerEventData data)
        {
            Debug.Log("Login Apple!");
            //Manager.User.SignInWithApple();
        }
        #endregion
    }
}
