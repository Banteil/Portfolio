using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIModifyNickName : UIPopup
    {
        private UIUserProfile profile;

        enum ModifyNickNameInputField
        {
            UserNickNameInputField,
        }

        enum ModifyNickNameButtons
        {
            ConfirmButton = 1,
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TMP_InputField>(typeof(ModifyNickNameInputField));
            Get<TMP_InputField>((int)ModifyNickNameInputField.UserNickNameInputField).text = profile.ProfileUserData.nickname;
            Bind<Button>(typeof(ModifyNickNameButtons));
            var confirmButton = GetButton((int)ModifyNickNameButtons.ConfirmButton);
            confirmButton.gameObject.BindEvent(ConfirmButton);
        }

        async private void ConfirmButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            if (profile != null)
            {
                var sid = profile.ProfileUserData.sid;

                bool exeption = await CheckExeption(sid);
                if (exeption) return;

                var userData = new UserData();
                await CallAPI.APISelectUser(sid, sid, (data) =>
                {
                    userData = data;
                    UserDataManager.Instance.MyData = userData;
                    profile.SetUserData(userData);
                    UIManager.Instance.CloseUI();
                });
            }
        }

        public void SetProfile(UIUserProfile profile) => this.profile = profile;

        async private UniTask<bool> CheckExeption(string sid)
        {
            var nickName = Get<TMP_InputField>((int)ModifyNickNameInputField.UserNickNameInputField).text;
            if (string.IsNullOrEmpty(nickName) || nickName.Length > 10)
            {
                UIManager.Instance.ShowWarningUI("nickNameLengthIssue");
                return true;
            }

            var updateReturn = -1;
            await CallAPI.APIUpdateUserNickName(sid, nickName, (returnCd) =>
            {
                updateReturn = returnCd;
            });
            switch (updateReturn)
            {
                case 1:
                    UIManager.Instance.ShowWarningUI("sameNickNameException");
                    return true;
                case 2:
                    UIManager.Instance.ShowWarningUI("modifyNickNameFail");
                    return true;
            }

            return false;
        }
    }
}
