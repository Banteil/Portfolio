using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class UIExpressionList : UIShopItemList
    {
        public override void SelectItem(PointerEventData data)
        {
            if (!isPurchasable || isDrag) return;
            base.SelectItem(data);
            GetParent<UIItemFrame>().SelectItem(index, Define.ItemType.Expression);
        }
    }
}
