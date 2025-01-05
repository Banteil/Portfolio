using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io
{
    public class SelectLanguagePopupUI : PopupUI, IListUI
    {
        #region Cache
        private enum LanguageScrollRect
        {
            LanguageListScrollView,
        }

        private enum LanguageButton
        {
            BackButton,
        }

        public SettingData SettingData { get; set; }
        #endregion

        protected override void BindInitialization()
        {
            Bind<ScrollRect>(typeof(LanguageScrollRect));
            Bind<Button>(typeof(LanguageButton));
            var backButton = GetButton((int)LanguageButton.BackButton);
            backButton.gameObject.BindEvent(OnCloseButtonClicked);
        }

        protected override void OnStart()
        {
            base.OnStart();
            SettingLocaleList();
        }

        private void SettingLocaleList()
        {
            var availableLocales = LocalizationSettings.AvailableLocales.Locales;
            var scrollRect = GetScrollRect((int)LanguageScrollRect.LanguageListScrollView) as InfinityScrollRect;
            scrollRect.MaxCount = availableLocales.Count;
            scrollRect.CreatePoolingList<LanguageListUI>();
        }

        public void SetListData(ListUI listUI)
        {
            var languageList = listUI as LanguageListUI;
            var index = languageList.GetIndex();
            var availableLocales = LocalizationSettings.AvailableLocales.Locales;
            languageList.SetListData(availableLocales[index]);
        }

    }
}
