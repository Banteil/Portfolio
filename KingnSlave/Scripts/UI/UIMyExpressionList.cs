using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
{
    public class UIMyExpressionList : UIMyItemList
    {
        public override void SelectItem(PointerEventData data)
        {
            if (isDrag) return;
            base.SelectItem(data);
            GetParent<UIItemFrame>().SelectItem(index, Define.ItemType.Expression);
        }
    }
}
