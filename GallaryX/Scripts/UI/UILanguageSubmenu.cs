using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace starinc.io.gallaryx
{
    public class UILanguageSubmenu : UISubmenu
    {
        [SerializeField]
        private List<LanguageButton> _languageButtons = new List<LanguageButton>();

        private void Start()
        {
            for (int i = 0; i < _languageButtons.Count; i++)
            {
                _languageButtons[i].SetIndex(i);
            }
        }

        public void ChangeLanguageByIndex(int index)
        {
            // AvailableLocales에서 로케일 배열을 가져옵니다.
            var locales = LocalizationSettings.AvailableLocales.Locales;

            if (index >= 0 && index < locales.Count)
            {
                // 유효한 인덱스인지 확인한 후 로케일을 설정합니다.
                LocalizationSettings.SelectedLocale = locales[index];                
                GameManager.Instance.ChangeLocaleCallback?.Invoke(LocalizationSettings.SelectedLocale.Identifier.Code);

                for (int i = 0; i < _languageButtons.Count; i++)
                {
                    _languageButtons[i].SelectButton(i == index);
                }
            }
            else
            {
                Debug.LogError("Invalid index value!");
            }
        }
    }
}
