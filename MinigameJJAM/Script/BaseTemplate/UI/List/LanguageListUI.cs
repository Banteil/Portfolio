using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace starinc.io
{
    public class LanguageListUI : ListUI
    {
        #region Cache
        private enum LanguageListText
        {
            LanguageText,
        }

        private Locale _locale;
        #endregion

        protected override void BindInitialization()
        {
            Bind<TextMeshProUGUI>(typeof(LanguageListText));
            gameObject.BindEvent(OnSelectList);
        }

        #region BindEvent
        private void OnSelectList(PointerEventData data)
        {
            if (_isDrag) return;
            LocalizationSettings.SelectedLocale = _locale;

            var index = Util.GetSelectedLocaleIndex();
            var languageUI = _parentUI as SelectLanguagePopupUI;
            languageUI.SettingData.LanguageIndex = index;
            Manager.Game.SaveData(languageUI.SettingData);

            Manager.UI.FindClosePopup(languageUI);
        }
        #endregion

        public void SetListData(Locale locale)
        {
            _locale = locale;
            var language = GetText((int)LanguageListText.LanguageText);
            language.text = locale.LocaleName;
        }
    }
}
