using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;

namespace starinc.io.kingnslave
{
    public class UILocalizationList : UIList
    {
        enum LocalizationListText
        {
            LocalizationTitleText,
        }

        private void Awake() => Initialized();

        protected override void InitializedProcess()
        {
            SetParent<UISelectLocalization>();
            Bind<TextMeshProUGUI>(typeof(LocalizationListText));
            gameObject.BindEvent(SelectLocale);
        }

        public void SetLocalizationInfoText(string info)
        {
            //var info = (LocalizationInfo)index;
            GetText((int)LocalizationListText.LocalizationTitleText).text = info.ToString();
        }

        private void SelectLocale(PointerEventData data)
        {
            if (isDrag) return;
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(0));
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];            
            UIManager.Instance.CloseUI();
        }
    }
}
