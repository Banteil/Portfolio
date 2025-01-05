using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.TMP_InputField;

namespace starinc.io.kingnslave
{
    public class EmailSignUpWindow : UIPopup
    {
        enum EmailSignUpWindowTexts
        {
            VerificationTimer
        }

        enum EmailSignUpWindowInputFields
        {
            UserIDInputField,
            PasswordInputField,
            EmailInputField,
            VerificationCodeInputField
        }

        enum EmailSignUpWindowButtons
        {
            SignUpButton = 1,
            GoBackButton,
            UserIDDuplicationCheckButton,
            SendCodeButton,
            EmailVerifyButton,
            PrivacyPolicyButton,
            TermsOfServiceButton,
        }

        enum EmailSignUpWindowImages
        {
            AllowedUserIdUI,
            VerifiedUserEmailUI
        }

        enum EmailSignUpWindowToggles
        {
            TermsOfUseAgreeToggle,
            PrivacyPolicyToggle,
        }

        private bool isAllowedUserId = false;
        public bool IsAllowedUserId
        {
            get { return isAllowedUserId; }
            private set
            {
                isAllowedUserId = value;
                allowedUserIdUI.gameObject.SetActive(isAllowedUserId);
            }
        }
        private bool isAllowedUserPassword = false;
        private bool isVerifiedUserEmail = false;
        public bool IsVerifiedUserEmail
        {
            get { return isVerifiedUserEmail; }
            private set
            {
                isVerifiedUserEmail = value;
                verifiedUserEmailUI.gameObject.SetActive(isVerifiedUserEmail);
            }
        }

        private Image allowedUserIdUI;
        private Image verifiedUserEmailUI;
        private TMP_Text timerText;
        private Toggle termsOfUseToggle;
        private Toggle privacyPolicyToggle;

        private List<UIBehaviour> verificationInputUI = new List<UIBehaviour>();
        private TMP_InputField verificationCodeText;
        private Coroutine timerCoroutine;

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            // Bind to Dictionary
            Bind<TMP_InputField>(typeof(EmailSignUpWindowInputFields));
            Bind<Button>(typeof(EmailSignUpWindowButtons));
            Bind<TextMeshProUGUI>(typeof(EmailSignUpWindowTexts));
            Bind<Image>(typeof(EmailSignUpWindowImages));
            Bind<Toggle>(typeof(EmailSignUpWindowToggles));

            // Get UI Objects
            allowedUserIdUI = GetImage((int)EmailSignUpWindowImages.AllowedUserIdUI);
            verifiedUserEmailUI = GetImage((int)EmailSignUpWindowImages.VerifiedUserEmailUI);
            verificationCodeText = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.VerificationCodeInputField);
            timerText = GetText((int)EmailSignUpWindowTexts.VerificationTimer);
            termsOfUseToggle = Get<Toggle>((int)EmailSignUpWindowToggles.TermsOfUseAgreeToggle);
            termsOfUseToggle.gameObject.BindEvent(OnToggleClicked);
            privacyPolicyToggle = Get<Toggle>((int)EmailSignUpWindowToggles.PrivacyPolicyToggle);
            privacyPolicyToggle.gameObject.BindEvent(OnToggleClicked);
            var signUpButton = GetButton((int)EmailSignUpWindowButtons.SignUpButton);
            var duplicationCheckButton = GetButton((int)EmailSignUpWindowButtons.UserIDDuplicationCheckButton);
            var sendCodeButton = GetButton((int)EmailSignUpWindowButtons.SendCodeButton);
            var emailVerifyButton = GetButton((int)EmailSignUpWindowButtons.EmailVerifyButton);
            var goBackButton = GetButton((int)EmailSignUpWindowButtons.GoBackButton);
            var privacyPolicyButton = GetButton((int)EmailSignUpWindowButtons.PrivacyPolicyButton);
            privacyPolicyButton.gameObject.BindEvent(ClickPrivacyPolicyButton);
            var termsOfServiceButton = GetButton((int)EmailSignUpWindowButtons.TermsOfServiceButton);
            termsOfServiceButton.gameObject.BindEvent(ClickTermsOfServiceButton);

            verificationInputUI.Add(verificationCodeText);
            verificationInputUI.Add(timerText);
            verificationInputUI.Add(emailVerifyButton);
            HideVerificationInputUI();

            // Bind UI and Event
            signUpButton.gameObject.BindEvent(ClickSignUpButton);
            duplicationCheckButton.gameObject.BindEvent(CheckUidButton);
            sendCodeButton.gameObject.BindEvent(SendEmailVerificationCode);
            emailVerifyButton.gameObject.BindEvent(VerifyEmailVerificationCode);
            goBackButton.gameObject.BindEvent(ClickGoBackButton);
            Get<TMP_InputField>((int)EmailSignUpWindowInputFields.UserIDInputField).onValueChanged.AddPersistentListener(OnUidTextChanged);
            Get<TMP_InputField>((int)EmailSignUpWindowInputFields.EmailInputField).onValueChanged.AddPersistentListener(OnEmailTextChanged);
        }

        /// <summary>
        /// User ID 텍스트 변경 시 허용 해제
        /// </summary>
        private void OnUidTextChanged(string changedText)
        {
            IsAllowedUserId = false;
        }

        /// <summary>
        /// 이메일 텍스트 변경 시 허용, 인증 해제
        /// </summary>
        private void OnEmailTextChanged(string changedText)
        {
            IsVerifiedUserEmail = false;
            HideVerificationInputUI();
        }

        private void CheckUidButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            var uid = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.UserIDInputField).text;
            Util.UidExceptionProcessing(uid, (exception) =>
            {
                if (exception == Define.UidExceptionHandling.uidNoissues)
                    IsAllowedUserId = true;
                else
                {
                    IsAllowedUserId = false;
                    var exceptionNames = Enum.GetNames(typeof(Define.UidExceptionHandling));
                    UIManager.Instance.ShowWarningUI(exceptionNames[(int)exception]);
                }
            });
        }

        private bool CheckPasswordFormat()
        {
            var password = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.PasswordInputField).text;
            Util.PasswordExceptionProcessing(password, (exception) =>
            {
                if(exception == Define.PasswordExceptionHandling.passwordNoissues)
                    isAllowedUserPassword = true;
                else
                {
                    isAllowedUserPassword = false;
                    var exceptionNames = Enum.GetNames(typeof(Define.PasswordExceptionHandling));
                    UIManager.Instance.ShowWarningUI(exceptionNames[(int)exception]);
                }
            });
            Debug.Log($"CheckPassword : {isAllowedUserPassword}");
            return isAllowedUserPassword;
        }

        private bool CheckEmail()
        {
            bool isCorrectEmailFormat = false;
            var email = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.EmailInputField);
            Util.EmailExceptionProcessing(email.text, (exception) =>
            {
                if (exception == Define.EmailExceptionHandling.emailNoissues)
                    isCorrectEmailFormat = true;
                else
                {
                    var exceptionNames = Enum.GetNames(typeof(Define.EmailExceptionHandling));
                    UIManager.Instance.ShowWarningUI(exceptionNames[(int)exception]);
                }
            });
            return isCorrectEmailFormat;
        }

        private async void SendEmailVerificationCode(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            var email = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.EmailInputField);

            if (!CheckEmail()) { return; }

            UIManager.Instance.ShowConnectingUI();
            await CallAPI.APISendEmailVerificationCode(UserDataManager.Instance.MyData.sid, email.text, (returnCd) =>
            {
                UIManager.Instance.CloseConnectingUI();
                if (returnCd == (int)Define.APIReturnCd.OK)
                {
                    UIManager.Instance.ShowWarningUI(Define.VerificationCodeExceptionHandling.sendEmailVerificationCodeSuccess.ToString());
                    ShowVerificationInputUI();
                }
                else
                {
                    UIManager.Instance.ShowWarningUI(Define.VerificationCodeExceptionHandling.sendEmailVerificationCodeFail.ToString());
                }
            });
        }

        private void VerifyEmailVerificationCode(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            IsVerifiedUserEmail = false;
            var email = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.EmailInputField);
            var verificationCode = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.VerificationCodeInputField);

            Util.VerificationCodeExceptionProcessing(verificationCode.text, async (exception) =>
            {
                if (exception == Define.VerificationCodeExceptionHandling.verificationCodeNoissues)
                {
                    await CallAPI.APIVerifyEmailVerificationCode(UserDataManager.Instance.MyData.sid, email.text, verificationCode.text, (returnCd) =>
                    {
                        if (returnCd == (int)Define.APIReturnCd.OK)
                        {
                            HideVerificationInputUI();
                            UIManager.Instance.ShowWarningUI(Define.VerificationCodeExceptionHandling.checkVerificationCodeSuccess.ToString());
                            IsVerifiedUserEmail = true;
                        }
                        else
                        {
                            UIManager.Instance.ShowWarningUI(Define.VerificationCodeExceptionHandling.checkVerificationCodeFail.ToString());
                        }
                    });
                }
                else
                {
                    UIManager.Instance.ShowWarningUI(Define.VerificationCodeExceptionHandling.checkVerificationCodeFail.ToString());
                }
            });
        }

        private void ClickPrivacyPolicyButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
#if UNITY_EDITOR
            Debug.Log("개인정보 처리방침 버튼");
#else
            CallWebView.ShowUrlFullScreen(Define.PrivacyPolicyURL, "privacyPolicyWebViewTitle");
#endif
        }

        private void ClickTermsOfServiceButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
#if UNITY_EDITOR
            Debug.Log("서비스 이용약관 버튼");
#else
            CallWebView.ShowUrlFullScreen(Define.TermsOfUseURL, "termsOfUseWebViewTitle");
#endif
        }

        private async void ClickSignUpButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            // ID 중복 확인된 상태인지 체크
            if (!IsAllowedUserId)
            {
                UIManager.Instance.ShowWarningUI("warningNotIDDuplicationChecked");
                return;
            }

            // PW 형식 체크
            if (!CheckPasswordFormat()) return;

            // 이메일 인증된 상태인지 체크
            if (!IsVerifiedUserEmail)
            {
                UIManager.Instance.ShowWarningUI("warningNotVerificationCodeChecked");
                return;
            }

            // 약관 동의 상태인지 체크
            if (!termsOfUseToggle.isOn)
            {
                UIManager.Instance.ShowWarningUI("warningNotTermsOfUseChecked");
                return;
            }

            if (!privacyPolicyToggle.isOn)
            {
                UIManager.Instance.ShowWarningUI("warningNotPrivacyPolicyChecked");
                return;
            }

            var userID = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.UserIDInputField);
            var password = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.PasswordInputField);
            var email = Get<TMP_InputField>((int)EmailSignUpWindowInputFields.EmailInputField);
            await CallAPI.APIUpdateUser(UserDataManager.Instance.MyData.sid, userID.text, Util.SHA512Hash(password.text), email.text, (int)Define.LoginType.Email, null);
            await UserDataManager.Instance.LogIn(userID.text, password.text, (returnCd) =>
            {
                if (returnCd == (int)Define.APIReturnCd.OK)
                {
                    Debug.Log("LOG-IN SUCCESS");
                    var profile = FindObjectOfType<UIUserProfile>();
                    if (profile != null)
                    {
                        profile.SetUserData(UserDataManager.Instance.MyData);
                    }
                    UIManager.Instance.CloseRangeUI(3);
                    UIManager.Instance.ShowWarningUI("loginComplete");
                }
                else
                {
                    Debug.LogError("LOG-IN FAIL");
                    UIManager.Instance.ShowWarningUI("warningFailedLogin");
                }
            });
        }

        private void ClickGoBackButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            UIManager.Instance.CloseUI();
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            UIManager.Instance.CloseRangeUI(3);
        }

        private void HideVerificationInputUI()
        {
            foreach (var ui in verificationInputUI)
            {
                ui.gameObject.SetActive(false);
            }
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }
            verificationCodeText.text = string.Empty;
        }

        private void ShowVerificationInputUI()
        {
            foreach (var ui in verificationInputUI)
            {
                ui.gameObject.SetActive(true);
            }
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
            }
            timerCoroutine = StartCoroutine(SpendVerificationCodeTimer());
        }

        private IEnumerator SpendVerificationCodeTimer()
        {
            const int CODE_EXPIRATION_TIME = 300;
            int timer = CODE_EXPIRATION_TIME;
            WaitForSecondsRealtime oneSecond = new WaitForSecondsRealtime(1f);
            while (true)
            {
                timerText.text = $"{timer / 60}:{(timer % 60).ToString("D2")}";
                yield return oneSecond;
                --timer;
            }
        }

        private void OnToggleClicked(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
        }
    }
}