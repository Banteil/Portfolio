using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIMyItem : UIItemPopup
    {
        private enum MyItemButton
        {
            UseButton = 2,
        }

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<Button>(typeof(MyItemButton));
            var useButton = GetButton((int)MyItemButton.UseButton);
            useButton.gameObject.BindEvent(UseItem);
        }

        async public override void SelectItem(int index, Define.ItemType itemType)
        {
            var selectedDataList = UserDataManager.Instance.GetItemTypeList(itemType);
            if (selectedDataList[index].in_use == 1) return;

            if(selectedData == selectedDataList[index])
            {
                ActiveInfoUI(false);
                return;
            }
            selectedData = selectedDataList[index];
            GetImage((int)ItemPopupImage.UsageImage).sprite = await NetworkManager.Instance.GetSpriteTask(selectedData.usage_image_url);
            ActiveInfoUI(true);
        }

        protected override void GetGemButton(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(24);
            base.GetGemButton(data);
        }

        public override void OpenFrame(int index)
        {
            switch ((ItemPopupFrame)index)
            {
                case ItemPopupFrame.ExpressionFrame:
                    LogManager.Instance.InsertActionLog(25);
                    break;
                case ItemPopupFrame.CardSkinFrame:
                    LogManager.Instance.InsertActionLog(26);
                    break;
                case ItemPopupFrame.ProfileImageFrame:
                    LogManager.Instance.InsertActionLog(27);
                    break;
            }
            base.OpenFrame(index);
        }

        async public void UseItem(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            if (selectedData == null) return;
            
            UIManager.Instance.ShowConnectingUI();
            var i18n = Util.GetLocalei18n(LocalizationSettings.SelectedLocale);
            await CallAPI.APIUpdateItemUserInUse(UserDataManager.Instance.MySid, selectedData.seq, selectedData.item_seq, selectedData.type, i18n, (data) =>
            {
                if (data == null)
                {
                    UIManager.Instance.CloseConnectingUI();
                    UIManager.Instance.ShowWarningUI("itemUseFail");
                }
                else
                    UseProcess(data);
            });
        }

        async private void UseProcess(List<ItemData> data)
        {
            await CallAPI.APISelectUser(UserDataManager.Instance.MySid, UserDataManager.Instance.MySid, (data) =>
            {
                UserDataManager.Instance.MyData = data;
            });
            if (selectedData.type == (int)Define.ItemType.ProfileImage)
            {                
                await NetworkManager.Instance.GetTextureTask((texture) =>
                {
                    UserDataManager.Instance.MyProfileImage = texture;
                }, selectedData.image_url);
            }
            UserDataManager.Instance.LocalRecachingList(data);
            ActiveInfoUI(false);
            UIManager.Instance.CloseConnectingUI();
        }
    }
}
