using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIMyExpressionFrame : UIItemFrame
    {
        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            var scrollRect = GetScrollRect((int)UIItemFrameScrollRect.ItemScrollView) as InfinityScrollRect;
            var dataList = UserDataManager.Instance.GetItemTypeList(Define.ItemType.Expression);
            scrollRect.MaxCount = dataList.Count;
            scrollRect.CreatePoolingList<UIMyExpressionList>("MyExpressionListUI");
        }

        public override void SetListData(UIList list)
        {
            var expressionList = list as UIMyExpressionList;
            var index = expressionList.GetIndex();
            var dataList = UserDataManager.Instance.GetItemTypeList(Define.ItemType.Expression);
            expressionList.SetListData(dataList[index]);
        }
    }
}
