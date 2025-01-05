using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIModifyPassword : UIPopup
    {
        private UIUserProfile profile;

        enum ModifyPasswordInputField
        {
            OldPasswordInputField,
            NewPasswordInputField,
        }

        enum ModifyPasswordButtons
        {
            ConfirmButton = 1,
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TMP_InputField>(typeof(ModifyPasswordInputField));
            Bind<Button>(typeof(ModifyPasswordButtons));
            var confirmButton = GetButton((int)ModifyPasswordButtons.ConfirmButton);
            confirmButton.gameObject.BindEvent(ConfirmButton);
        }

        private void ConfirmButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            if (profile != null)
            {
                var sid = profile.ProfileUserData.sid;
                var pwdOld = Get<TMP_InputField>((int)ModifyPasswordInputField.OldPasswordInputField).text;
                var pwdNewInputfield = Get<TMP_InputField>((int)ModifyPasswordInputField.NewPasswordInputField);
                var pwdNew = pwdNewInputfield.text;
                Util.PasswordExceptionProcessing(pwdNew, (exception) =>
                {
                    if (exception == Define.PasswordExceptionHandling.passwordNoissues)
                    {
                        UpdatePasswordProcess(sid, pwdOld, pwdNew);
                    }
                    else
                    {
                        var exceptionNames = Enum.GetNames(typeof(Define.PasswordExceptionHandling));
                        UIManager.Instance.ShowWarningUI(exceptionNames[(int)exception]);
                        pwdNewInputfield.ActivateInputField();
                        pwdNewInputfield.Select();
                    }
                });                
            }
        }

        async private void UpdatePasswordProcess(string sid, string pwdOld, string pwdNew)
        {
            await CallAPI.APIUpdateUserPassword(sid, Util.SHA512Hash(pwdOld), Util.SHA512Hash(pwdNew), (returnCd) =>
            {
                var exceptionNames = Enum.GetNames(typeof(Define.PasswordExceptionHandling));
                if (returnCd != (int)Define.APIReturnCd.OK)
                {                    
                    UIManager.Instance.ShowWarningUI(exceptionNames[(int)Define.PasswordExceptionHandling.passwordInvalidIssue]);
                    return;
                }
                else
                {
                    UIManager.Instance.CloseUI();
                    UIManager.Instance.ShowWarningUI(exceptionNames[(int)Define.PasswordExceptionHandling.passwordModifySuccess]);                    
                }
            });
        }

        public void SetProfile(UIUserProfile profile) => this.profile = profile;
    }
}
