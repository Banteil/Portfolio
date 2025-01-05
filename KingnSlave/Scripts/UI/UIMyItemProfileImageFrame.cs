namespace starinc.io.kingnslave
{
    public class UIMyItemProfileImageFrame : UIItemFrame
    {
        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            var scrollRect = GetScrollRect((int)UIItemFrameScrollRect.ItemScrollView) as InfinityScrollRect;
            var dataList = UserDataManager.Instance.GetItemTypeList(Define.ItemType.ProfileImage);
            scrollRect.MaxCount = dataList.Count;
            scrollRect.CreatePoolingList<UIMyItemProfileImageList>("MyItemProfileImageListUI");
        }

        public override void SetListData(UIList list)
        {
            var itemProfileImageList = list as UIMyItemProfileImageList;
            var index = itemProfileImageList.GetIndex();
            var dataList = UserDataManager.Instance.GetItemTypeList(Define.ItemType.ProfileImage);
            itemProfileImageList.SetListData(dataList[index]);
        }
    }
}
