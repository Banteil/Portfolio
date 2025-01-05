namespace starinc.io.kingnslave
{
    public class UIExpressionFrame : UIItemFrame
    {
        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            var scrollRect = GetScrollRect((int)UIItemFrameScrollRect.ItemScrollView) as InfinityScrollRect;
            var dataList = ShopManager.Instance.GetItemTypeList(Define.ItemType.Expression);
            scrollRect.MaxCount = dataList.Count;
            scrollRect.CreatePoolingList<UIExpressionList>("ExpressionListUI");
        }

        public override void SetListData(UIList list)
        {
            var expressionList = list as UIExpressionList;
            var index = expressionList.GetIndex();
            var dataList = ShopManager.Instance.GetItemTypeList(Define.ItemType.Expression);
            expressionList.SetListData(dataList[index]);
        }
    }
}
