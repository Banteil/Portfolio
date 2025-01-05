using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIItemFrame : UIFrame
    {
        private UIItemPopup itemPopup;

        protected enum UIItemFrameScrollRect
        {
            ItemScrollView,
        } 

        protected override void InitializedProcess()
        {
            itemPopup = Util.FindComponentInParents<UIItemPopup>(transform);
            Bind<ScrollRect>(typeof(UIItemFrameScrollRect));
            var scrollRect = GetScrollRect((int)UIItemFrameScrollRect.ItemScrollView) as InfinityScrollRect;
            UserDataManager.Instance.RecachingCallback += scrollRect.ResetListData;

            itemPopup.ActiveInfoCallback += ActiveInfo;
        }

        public override void SetListData(UIList list) { }

        public void SelectItem(int index, Define.ItemType itemType) => itemPopup.SelectItem(index, itemType);

        protected void ActiveInfo(bool isActive)
        {
            var scrollRect = GetScrollRect((int)UIItemFrameScrollRect.ItemScrollView) as InfinityScrollRect;
            scrollRect.viewport.offsetMin = isActive ? new Vector2(0, itemPopup.InfoHeight) : Vector2.zero;
        }

        public void OnDestroy()
        {
            if (UserDataManager.HasInstance)
            {
                var scrollRect = GetScrollRect((int)UIItemFrameScrollRect.ItemScrollView) as InfinityScrollRect;
                UserDataManager.Instance.RecachingCallback -= scrollRect.ResetListData;
            }
        }
    }
}
