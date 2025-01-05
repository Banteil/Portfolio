namespace starinc.io.kingnslave
{
    public class UIMyCardSkinFrame : UIItemFrame
    {
        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            var scrollRect = GetScrollRect((int)UIItemFrameScrollRect.ItemScrollView) as InfinityScrollRect;
            var dataList = UserDataManager.Instance.GetItemTypeList(Define.ItemType.CardSkin);
            scrollRect.MaxCount = dataList.Count;
            scrollRect.CreatePoolingList<UIMyCardSkinList>("MyCardSkinListUI");
        }

        public override void SetListData(UIList list)
        {
            var cardSkinList = list as UIMyCardSkinList;
            var index = cardSkinList.GetIndex();
            var dataList = UserDataManager.Instance.GetItemTypeList(Define.ItemType.CardSkin);
            cardSkinList.SetListData(dataList[index]);
        }
    }
}
