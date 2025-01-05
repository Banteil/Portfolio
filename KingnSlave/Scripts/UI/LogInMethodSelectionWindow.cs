using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class LogInMethodSelectionWindow : UIPopup
    {
        enum LogInMethodSelectionWindowButtons
        {
            LogInWithEmailButton = 1,
            LogInWithGoogleButton,
            LogInWithFacebookButton,
            LogInWithAppleButton,
        }

        private void Start() => Initialized(); 

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<Button>(typeof(LogInMethodSelectionWindowButtons));
            var emailButton = GetButton((int)LogInMethodSelectionWindowButtons.LogInWithEmailButton);
            emailButton.gameObject.BindEvent(ClickEmailButton);
            var googleButton = GetButton((int)LogInMethodSelectionWindowButtons.LogInWithGoogleButton);
            googleButton.gameObject.BindEvent(ClickGoogleButton);
            var facebookButton = GetButton((int)LogInMethodSelectionWindowButtons.LogInWithFacebookButton);
            facebookButton.gameObject.BindEvent(ClickFacebookButton);
            var appleButton = GetButton((int)LogInMethodSelectionWindowButtons.LogInWithAppleButton);
            appleButton.gameObject.BindEvent(ClickAppleButton);
            //if (Application.platform == RuntimePlatform.Android)
            //    appleButton.gameObject.SetActive(false);
            //Debug.Log("platform: " + Application.platform);

#if UNITY_ANDROID
            appleButton.gameObject.SetActive(false);
#elif UNITY_IOS
            googleButton.gameObject.SetActive(false);
#endif
        }

        private void ClickEmailButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            UIManager.Instance.ShowUI<EmailLogInWindow>("EmailLogInUI");
        }

        private void ClickGoogleButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            Debug.Log("Login Google!");
        }

        private void ClickFacebookButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            Debug.Log("Login Facebook!");
        }

        private void ClickAppleButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            Debug.Log("Login Apple!");
        }
    }
}