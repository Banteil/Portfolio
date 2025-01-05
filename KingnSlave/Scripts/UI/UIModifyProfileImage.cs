using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIModifyProfileImage : UIPopup
    {
        private UIUserProfile profile;
        public int SelectedIndex { get; set; } = -1;

        enum ModifyProfileImageScrollView
        {
            ProfileImageScrollView,
        }

        enum ModifyProfileImageButtons
        {
            ConfirmButton = 1,
        }

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<ScrollRect>(typeof(ModifyProfileImageScrollView));
            Bind<Button>(typeof(ModifyProfileImageButtons));
            GetButton((int)ModifyProfileImageButtons.ConfirmButton).gameObject.BindEvent(ConfirmButton);

            var profileList = ShopManager.Instance.GetItemTypeList(Define.ItemType.ProfileImage);
            var scrollRect = GetScrollRect((int)ModifyProfileImageScrollView.ProfileImageScrollView) as InfinityScrollRect;
            scrollRect.MaxCount = profileList.Count;
            scrollRect.CreatePoolingList<UIProfileImageList>("ProfileImageListUI");

            var findIndex = profileList.FindIndex((data) => data.image_url == UserDataManager.Instance.MyData.profile_image);
            SelectedIndex = findIndex;
            if (SelectedIndex != -1)
            {
                var list = scrollRect.GetList<UIProfileImageList>(SelectedIndex);
                if (list != null)
                    list.GetToggle().isOn = true;
            }

            UserDataManager.Instance.RecachingCallback += scrollRect.ResetListData;
        }

        public override void SetListData(UIList list)
        {
            var profileImageList = list as UIProfileImageList;
            var index = profileImageList.GetIndex();
            profileImageList.SetListData(ShopManager.Instance.GetItemTypeList(Define.ItemType.ProfileImage)[index], profile.ProfileUserData);
            profileImageList.GetToggle().isOn = index == SelectedIndex;
        }

        public ItemData GetData(int index) => ShopManager.Instance.GetItemTypeList(Define.ItemType.ProfileImage)[index];

        async private void ConfirmButton(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            var profileList = ShopManager.Instance.GetItemTypeList(Define.ItemType.ProfileImage);
            if (profileList.Count <= 0 || SelectedIndex < 0 || SelectedIndex >= profileList.Count)
            {
                UIManager.Instance.CloseUI();
                return;
            }

            if (profile != null)
            {
                var sid = profile.ProfileUserData.sid;
                var itemData = profileList[SelectedIndex];
                var myData = UserDataManager.Instance.GetItemTypeList(Define.ItemType.ProfileImage).Find((data) => data.item_seq == itemData.seq);
                if (profile.ProfileUserData.profile_image != myData.image_url)
                {
                    UIManager.Instance.ShowConnectingUI();
                    var exceptionNames = Enum.GetNames(typeof(Define.ProfileImageExceptionHandling));

                    var i18n = Util.GetLocalei18n(LocalizationSettings.SelectedLocale);
                    await CallAPI.APIUpdateItemUserInUse(sid, myData.seq, myData.item_seq, myData.type, i18n, (data) =>
                    {
                        if (data == null)
                        {
                            UIManager.Instance.CloseConnectingUI();
                            UIManager.Instance.ShowWarningUI(exceptionNames[(int)Define.ProfileImageExceptionHandling.profileImageModifyFail]);
                        }
                        else
                            UpdateProfileImageProcess(sid, data, myData.image_url);
                    });
                }
                else
                {
                    var exceptionNames = Enum.GetNames(typeof(Define.ProfileImageExceptionHandling));
                    UIManager.Instance.CloseUI();
                    UIManager.Instance.ShowWarningUI(exceptionNames[(int)Define.ProfileImageExceptionHandling.profileImageModifySuccess]);
                }
            }
        }

        async private void UpdateProfileImageProcess(string sid, List<ItemData> dataList, string imageURL)
        {
            var exceptionNames = Enum.GetNames(typeof(Define.ProfileImageExceptionHandling));
            await CallAPI.APISelectUser(sid, sid, (data) =>
            {
                UserDataManager.Instance.MyData = data;
                profile.SetUserData(data);
            });

            await NetworkManager.Instance.GetTextureTask((texture) =>
            {
                UserDataManager.Instance.MyProfileImage = texture;
            }, imageURL);

            OnDestroy();
            UserDataManager.Instance.LocalRecachingList(dataList);
            UIManager.Instance.CloseConnectingUI();
            UIManager.Instance.CloseUI();
            UIManager.Instance.ShowWarningUI(exceptionNames[(int)Define.ProfileImageExceptionHandling.profileImageModifySuccess]);
        }

        public void SetProfile(UIUserProfile profile) => this.profile = profile;

        public void ResetToggle(UIProfileImageList interactList)
        {
            var scrollRect = GetScrollRect((int)ModifyProfileImageScrollView.ProfileImageScrollView) as InfinityScrollRect;
            if (scrollRect != null)
            {
                var list = scrollRect.GetList<UIProfileImageList>(SelectedIndex);
                if(list != null)
                    list.GetToggle().isOn = true;
                else
                    interactList.GetToggle().isOn = false;
            }
        }

        private void OnDestroy()
        {
            if (UserDataManager.HasInstance)
            {
                var scrollRect = GetScrollRect((int)ModifyProfileImageScrollView.ProfileImageScrollView) as InfinityScrollRect;
                UserDataManager.Instance.RecachingCallback -= scrollRect.ResetListData;
            }
        }
    }
}
