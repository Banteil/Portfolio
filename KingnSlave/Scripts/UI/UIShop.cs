using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIShop : UIItemPopup
    {
        protected enum ItemShopText
        {
            GemPriceInfoText = 3,
        }

        protected enum ItemShopButton
        {
            BuyButton = 2,
        }

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TextMeshProUGUI>(typeof(ItemShopText));
            Bind<Button>(typeof(ItemShopButton));
            var buyButton = GetButton((int)ItemShopButton.BuyButton);
            buyButton.gameObject.BindEvent(BuyItem);
        }

        async public override void SelectItem(int index, Define.ItemType itemType)
        {
            var selectedDataList = ShopManager.Instance.GetItemTypeList(itemType);
            if (selectedData == selectedDataList[index])
            {
                ActiveInfoUI(false);
                return;
            }
            selectedData = selectedDataList[index];
            GetText((int)ItemShopText.GemPriceInfoText).text = selectedData.price_gem.ToString();
            GetImage((int)ItemPopupImage.UsageImage).sprite = await NetworkManager.Instance.GetSpriteTask(selectedData.usage_image_url);
            ActiveInfoUI(true);
        }

        protected override void GetGemButton(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(18);
            base.GetGemButton(data);
        }

        public override void OpenFrame(int index)
        {
            switch ((ItemPopupFrame)index)
            {
                case ItemPopupFrame.ExpressionFrame:
                    LogManager.Instance.InsertActionLog(19);
                    break;
                case ItemPopupFrame.CardSkinFrame:
                    LogManager.Instance.InsertActionLog(20);
                    break;
                case ItemPopupFrame.ProfileImageFrame:
                    LogManager.Instance.InsertActionLog(21);
                    break;
            }
            base.OpenFrame(index);
        }


        async public void BuyItem(PointerEventData data)
        {            
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            if (selectedData == null) return;
            else if(selectedData.price_gem > UserDataManager.Instance.MyGem)
            {
                LogManager.Instance.InsertActionLog(22, selectedData.seq.ToString());
                UIManager.Instance.ShowWarningUI("notEnoughGems");
                return;
            }

            LogManager.Instance.InsertActionLog(22, selectedData.seq.ToString());
            UIManager.Instance.ShowConnectingUI();
            var i18n = Util.GetLocalei18n(LocalizationSettings.SelectedLocale);
            await CallAPI.APIBuyItem(UserDataManager.Instance.MySid, selectedData.seq, 1, i18n, (data) =>
            {
                if (data == null)
                {
                    UIManager.Instance.CloseConnectingUI();
                    UIManager.Instance.ShowWarningUI("purchaseFailed");
                }
                else
                    BuyProcess(data);
            });
        }

        async private void BuyProcess(UserData data)
        {
            UserDataManager.Instance.MyGem = data.gem_amount;
            await ShopManager.Instance.RecachingList();

            var itemUserList = Util.CastingJsonObject<List<ItemData>>(data.item_user_list);
            UserDataManager.Instance.LocalRecachingList(itemUserList);
            ActiveInfoUI(false);
            UIManager.Instance.CloseConnectingUI();
            UIManager.Instance.ShowWarningUI("purchaseSuccess");
        }
    }
}
