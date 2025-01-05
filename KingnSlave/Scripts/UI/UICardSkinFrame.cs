namespace starinc.io.kingnslave
{
    public class UICardSkinFrame : UIItemFrame
    {
        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            var scrollRect = GetScrollRect((int)UIItemFrameScrollRect.ItemScrollView) as InfinityScrollRect;
            var dataList = ShopManager.Instance.GetItemTypeList(Define.ItemType.CardSkin);
            scrollRect.MaxCount = dataList.Count;
            scrollRect.CreatePoolingList<UICardSkinList>("CardSkinListUI");
        }

        public override void SetListData(UIList list)
        {
            var cardSkinList = list as UICardSkinList;
            var index = cardSkinList.GetIndex();
            var dataList = ShopManager.Instance.GetItemTypeList(Define.ItemType.CardSkin);
            cardSkinList.SetListData(dataList[index]);
        }
    }
}
