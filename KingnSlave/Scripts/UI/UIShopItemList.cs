using TMPro;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIShopItemList : UIItemList
    {
        protected bool isPurchasable;

        protected enum ShopItemText
        {
            GemPriceText = 1,
        }

        protected enum ShopItemImage
        {
            SoldImage = 1,
        }

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TextMeshProUGUI>(typeof(ShopItemText));
            Bind<Image>(typeof(ShopItemImage));
        }

        public override void SetListData(ItemData data)
        {
            GetText((int)ShopItemText.GemPriceText).text = data.price_gem.ToString();

            isPurchasable = data.purchasable_yn == "Y";
            GetImage((int)ShopItemImage.SoldImage).enabled = !isPurchasable;
            itemListButton.interactable = isPurchasable;
            base.SetListData(data);
        }
    }
}
