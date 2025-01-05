namespace starinc.io.kingnslave
{
    public class UIItemProfileImageFrame : UIItemFrame
    { 
        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            var scrollRect = GetScrollRect((int)UIItemFrameScrollRect.ItemScrollView) as InfinityScrollRect;
            var dataList = ShopManager.Instance.GetItemTypeList(Define.ItemType.ProfileImage);
            scrollRect.MaxCount = dataList.Count;
            scrollRect.CreatePoolingList<UIItemProfileImageList>("ItemProfileImageListUI");
        }

        public override void SetListData(UIList list)
        {
            var itemProfileImageList = list as UIItemProfileImageList;
            var index = itemProfileImageList.GetIndex();
            var dataList = ShopManager.Instance.GetItemTypeList(Define.ItemType.ProfileImage);
            itemProfileImageList.SetListData(dataList[index]);
        }
    }
}
