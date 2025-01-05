using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UISelectLocalization : UIPopup
    {
        [SerializeField] Transform content;

        private enum SelectLocalizationScrollView
        {
            LocalizationScrollView,
        }

        enum LocalizationInfo
        {
            ENGLISH,
            TÜRKÇE,
            FRANÇAIS,
            DEUTSCH,
            ITALIANO,
            ESPAÑOL,
            PORTUGUÊS,
            РУССКИЙ,
            日本語,
            한국어,
            العربية,
            NEDERLANDS,
            简体中文,
            繁體中文,
            POLSKI,
            BAHASAINDONESIA,
            УКРАЇНСЬКА,
            ROMÂNĂ,
            TIẾNGVIỆT,
            ไทย,
            DANSK,
            SVENSKA,
            ΕΛΛΗΝΙΚΆ,
            MAGYAR,
            עברית,
            ČEŠTINA,
            BAHASAMELAYU,
            AZƏRBAYCANDİLİ,
            فارسی,
            NORSK,
        }

        protected override void Awake()
        {
            base.Awake();
            Initialized();
        }

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            Bind<ScrollRect>(typeof(SelectLocalizationScrollView));
            SetLocalizationList();
        }

        public void SetLocalizationList()
        {
            var maxCount = LocalizationSettings.AvailableLocales.Locales.Count;
            var scrollRect = GetScrollRect((int)SelectLocalizationScrollView.LocalizationScrollView) as InfinityScrollRect;
            scrollRect.MaxCount = maxCount;
            scrollRect.CreatePoolingList<UILocalizationList>("LocalizationListUI");
        }

        public override void SetListData(UIList list)
        {
            var localizationList = list as UILocalizationList;
            var index = localizationList.GetIndex();
            var info = ((LocalizationInfo)index).ToString();
            localizationList.SetLocalizationInfoText(info);
        }
    }
}
