using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIModifyUID : UIPopup
    {
        private UIUserProfile profile;

        enum ModifyUIDInputField
        {
            UIDInputField,
        }

        enum ModifyUIDButtons
        {
            ConfirmButton = 1,
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TMP_InputField>(typeof(ModifyUIDInputField));
            Get<TMP_InputField>((int)ModifyUIDInputField.UIDInputField).text = profile.ProfileUserData.uid;
            Bind<Button>(typeof(ModifyUIDButtons));
            var confirmButton = GetButton((int)ModifyUIDButtons.ConfirmButton);
            confirmButton.gameObject.BindEvent(ConfirmButton);
         }

        private void ConfirmButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            if (profile != null)
            {
                var sid = profile.ProfileUserData.sid;
                var uid = Get<TMP_InputField>((int)ModifyUIDInputField.UIDInputField).text;

                Util.UidExceptionProcessing(uid, (exception) =>
                {
                    if (exception == Define.UidExceptionHandling.uidNoissues)
                    {
                        UpdateUIDProcess(sid, uid);
                    }
                    else
                    {
                        var exceptionNames = Enum.GetNames(typeof(Define.UidExceptionHandling));
                        UIManager.Instance.ShowWarningUI(exceptionNames[(int)exception]);
                    }
                });               
            }            
        }

        async private void UpdateUIDProcess(string sid, string uid)
        {
            await CallAPI.APIUpdateUserUid(sid, uid, (returnCd) =>
            {
                if (returnCd != (int)Define.APIReturnCd.OK)
                {
                    Debug.LogError("유저 UID 업데이트에 실패하였습니다.");
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
    }
}
