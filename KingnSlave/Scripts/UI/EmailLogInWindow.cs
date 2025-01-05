using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class EmailLogInWindow : UIPopup
    {
        enum EmailLoginWindowInputField
        {
            UserIDInputField,
            PasswordInputField,
        }

        enum EmailLoginWindowButtons
        {
            LogInButton = 1,
            SignUpButton,
            GoBackButton,
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TMP_InputField>(typeof(EmailLoginWindowInputField));
            Bind<Button>(typeof(EmailLoginWindowButtons));
            var loginButton = GetButton((int)EmailLoginWindowButtons.LogInButton);
            loginButton.gameObject.BindEvent(LogIn);
            var signupButton = GetButton((int)EmailLoginWindowButtons.SignUpButton);
            signupButton.gameObject.BindEvent(SignUp);
            var gobackButton = GetButton((int)EmailLoginWindowButtons.GoBackButton);
            gobackButton.gameObject.BindEvent(GoBack);
        }

        async private void LogIn(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            Debug.Log($"Before LOGIN: {UserDataManager.Instance.MyData.sid}, {UserDataManager.Instance.MyData.uid}, {UserDataManager.Instance.MyData.nickname}");
            var uid = Get<TMP_InputField>((int)EmailLoginWindowInputField.UserIDInputField);
            var password = Get<TMP_InputField>((int)EmailLoginWindowInputField.PasswordInputField);
            await UserDataManager.Instance.LogIn(uid.text, password.text, (returnCd) =>
            {
                if (returnCd != 0)
                {
                    Debug.LogError("LOG-IN FAIL");
                    UIManager.Instance.ShowWarningUI("warningCheckIDAndPass");
                    return;
                }
                else
                {
                    Debug.Log("LOG-IN SUCCESS");
                    Debug.Log($"After LOGIN: {UserDataManager.Instance.MyData.sid}, {UserDataManager.Instance.MyData.uid}, {UserDataManager.Instance.MyData.nickname}");
                    var profile = FindObjectOfType<UIUserProfile>();
                    if(profile != null)
                    {
                        profile.SetUserData(UserDataManager.Instance.MyData);
                    }
                    UIManager.Instance.CloseRangeUI(2);
                    UIManager.Instance.ShowWarningUI("loginComplete");
                }
            });
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickCloseButton));
            UIManager.Instance.CloseRangeUI(2);
        }

        private void SignUp(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            if (!string.IsNullOrEmpty(UserDataManager.Instance.MyData.email))
            {
                UIManager.Instance.ShowWarningUI("alreadySignedUp");
                return;
            }
            UIManager.Instance.ShowUI<EmailSignUpWindow>("EmailSignUpUI");
        }

        private void GoBack(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            UIManager.Instance.CloseUI();
        }
    }
}