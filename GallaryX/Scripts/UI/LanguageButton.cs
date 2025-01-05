using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class LanguageButton : UIList
    {
        private Button _languageButton;
        private TextMeshProUGUI _languageText;
        private Image _selectedImage;

        private void Awake()
        {
            _languageButton = GetComponent<Button>();
            _languageText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _selectedImage = transform.GetChild(1).GetComponent<Image>();
            SetParent<UILanguageSubmenu>();
        }

        public override void SetIndex(int index)
        {
            base.SetIndex(index);
            var languageParent = GetParent<UILanguageSubmenu>();
            _languageButton.onClick.AddListener(() =>
            {
                languageParent.ChangeLanguageByIndex(_index);
            });

            var locales = LocalizationSettings.AvailableLocales.Locales;
            _languageText.text = locales[_index].LocaleName;

            var selectedIndex = locales.IndexOf(LocalizationSettings.SelectedLocale);
            _selectedImage.gameObject.SetActive(selectedIndex == _index);
        }

        public void SelectButton(bool isSelect) => _selectedImage.gameObject.SetActive(isSelect);
    }
}
