using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIModifyEmail : UIPopup
    {
        private UIUserProfile profile;
        //private bool isVerified, isSendCode;
        //private bool IsVerified
        //{
        //    set
        //    {
        //        isVerified = value;
        //        GetButton((int)ModifyEmailButtons.ConfirmButton).interactable = value;
        //        Get<TMP_InputField>((int)ModifyEmailInputField.VerificationCodeInputField).interactable = !value;
        //    }
        //}

        //private bool IsSendCode
        //{
        //    set
        //    {
        //        isSendCode = value;
        //        Get<TMP_InputField>((int)ModifyEmailInputField.EmailInputField).interactable = !value;
        //        GetButton((int)ModifyEmailButtons.CheckCodeButton).interactable = value;

        //        var sendCodeButton = GetButton((int)ModifyEmailButtons.SendCodeButton);
        //        var sendCodeButtonText = GetText((int)ModifyEmailText.SendCodeButtonText);
        //        if (value)
        //        {
        //            sendCodeButton.gameObject.ClearEvent();
        //            sendCodeButton.gameObject.BindEvent(ReEnterButton);
        //            sendCodeButtonText.text = "ReEnter";
        //        }
        //        else
        //        {
        //            Get<TMP_InputField>((int)ModifyEmailInputField.VerificationCodeInputField).text = "";
        //            sendCodeButton.gameObject.ClearEvent();
        //            sendCodeButton.gameObject.BindEvent(SendCodeButton);
        //            sendCodeButtonText.text = "Send Code";
        //        }
        //    }
        //}

        private bool isVerifiedUserEmail = false;
        private Image verifiedUserEmailUI;
        private TMP_Text timerText;
        private List<UIBehaviour> verificationInputUI = new List<UIBehaviour>();
        private Coroutine timerCoroutine;

        public bool IsVerifiedUserEmail
        {
            get { return isVerifiedUserEmail; }
            private set
            {
                isVerifiedUserEmail = value;
                verifiedUserEmailUI.gameObject.SetActive(isVerifiedUserEmail);
            }
        }

        enum ModifyEmailInputField
        {
            EmailInputField,
            VerificationCodeInputField
        }

        enum ModifyEmailButtons
        {
            ConfirmButton = 1,
            SendCodeButton,
            EmailVerifyButton,
        }

		enum ModifyEmailImages
		{
			VerifiedUserEmailUI
		}

		enum ModifyEmailText
        {
            VerificationTimer
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TMP_InputField>(typeof(ModifyEmailInputField));
            Bind<TextMeshProUGUI>(typeof(ModifyEmailText));
            Bind<Button>(typeof(ModifyEmailButtons));
			Bind<Image>(typeof(ModifyEmailImages));

			verifiedUserEmailUI = GetImage((int)ModifyEmailImages.VerifiedUserEmailUI);
			timerText = GetText((int)ModifyEmailText.VerificationTimer);
			var confirmButton = GetButton((int)ModifyEmailButtons.ConfirmButton);
            var sendCodeButton = GetButton((int)ModifyEmailButtons.SendCodeButton);
            var checkCodeButton = GetButton((int)ModifyEmailButtons.EmailVerifyButton);
            Get<TMP_InputField>((int)ModifyEmailInputField.EmailInputField).text = profile.ProfileUserData.email;

            confirmButton.gameObject.BindEvent(ConfirmButton);
            sendCodeButton.gameObject.BindEvent(SendEmailVerificationCode);
            checkCodeButton.gameObject.BindEvent(VerifyEmailVerificationCode);
            Get<TMP_InputField>((int)ModifyEmailInputField.EmailInputField).onValueChanged.AddPersistentListener(OnEmailTextChanged);

            verificationInputUI.Add(Get<TMP_InputField>((int)ModifyEmailInputField.VerificationCodeInputField));
            verificationInputUI.Add(timerText);
            verificationInputUI.Add(checkCodeButton);
            HideVerificationInputUI();
        }

        private async void SendEmailVerificationCode(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            var email = Get<TMP_InputField>((int)ModifyEmailInputField.EmailInputField);
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
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            IsVerifiedUserEmail = false;
            var email = Get<TMP_InputField>((int)ModifyEmailInputField.EmailInputField);
            var verificationCode = Get<TMP_InputField>((int)ModifyEmailInputField.VerificationCodeInputField);

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

        private void ConfirmButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            // 이메일 인증된 상태인지 체크
            if (!IsVerifiedUserEmail)
            {
                UIManager.Instance.ShowWarningUI("warningNotVerificationCodeChecked");
                return;
            }
            if (profile != null)
            {
                var sid = profile.ProfileUserData.sid;
                var email = Get<TMP_InputField>((int)ModifyEmailInputField.EmailInputField).text;
                UpdateEmailProcess(sid, email);
            }
        }

        async private void UpdateEmailProcess(string sid, string email)
        {
            await CallAPI.APIUpdateUserEmail(sid, email, (returnCd) =>
            {
                if (returnCd != (int)Define.APIReturnCd.OK)
                {
                    Debug.LogError("유저 이메일 업데이트에 실패하였습니다.");
                    return;
                }
            });

            var userData = new UserData();
            await CallAPI.APISelectUser(sid, sid, (data) =>
            {
                userData = data;
                UserDataManager.Instance.MyData = userData;
                profile.SetUserData(userData);
                UIManager.Instance.CloseUI();
            });
        }

        public void SetProfile(UIUserProfile profile) => this.profile = profile;

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
        }

		private bool CheckEmail()
		{
			bool isCorrectEmailFormat = false;
			var email = Get<TMP_InputField>((int)ModifyEmailInputField.EmailInputField);
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

        /// <summary>
        /// 이메일 텍스트 변경 시 허용, 인증 해제
        /// </summary>
        private void OnEmailTextChanged(string changedText)
        {
            IsVerifiedUserEmail = false;
            HideVerificationInputUI();
        }
    }
}
