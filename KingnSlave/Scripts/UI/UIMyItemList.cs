using TMPro;

namespace starinc.io.kingnslave
{
    public class UIMyItemList : UIItemList
    {
        protected bool isUse;

        protected enum MyItemText
        {
            IsUseText = 1,
        }

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<TextMeshProUGUI>(typeof(MyItemText));
        }

        public override void SetListData(ItemData data)
        {
            isUse = data.in_use == 1;
            GetText((int)MyItemText.IsUseText).enabled = isUse;
            base.SetListData(data);
        }
    }
}
