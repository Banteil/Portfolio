using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class UIItemProfileImageList : UIShopItemList
    {
        public override void SelectItem(PointerEventData data)
        {
            if (!isPurchasable || isDrag) return;
            base.SelectItem(data);
            GetParent<UIItemFrame>().SelectItem(index, Define.ItemType.ProfileImage);
        }
    }
}
