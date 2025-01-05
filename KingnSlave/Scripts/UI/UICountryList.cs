using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UICountryList : UIList
    {
        enum CountryListImage
        {
            CountryImage,
        }

        enum CountryListToggle
        {
            CountryListUI,
        }

        enum CountryListText
        {
            InfoText,
        }

        protected override void InitializedProcess()
        {
            SetParent<UIModifyCountry>();
            Bind<Image>(typeof(CountryListImage));
            Bind<Toggle>(typeof(CountryListToggle));
            Bind<TextMeshProUGUI>(typeof(CountryListText));
            var toggle = Get<Toggle>((int)CountryListToggle.CountryListUI);
            toggle.group = transform.parent.GetComponent<ToggleGroup>();
            toggle.gameObject.BindEvent(SelectCountry);
        }

        private void SelectCountry(PointerEventData data)
        {
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));

            var parent = GetParent<UIModifyCountry>();
            if (isDrag || parent.SelectedIndex == index)
            {
                parent.ResetToggle(this);
                return;
            }
            GetParent<UIModifyCountry>().SelectedIndex = index;
        }

        public void SetListData(Sprite countrySprite)
        {
            var resultName = countrySprite.name.Replace("-flag", "").Replace("_", "");
            GetText((int)CountryListText.InfoText).text = resultName;
            Get<Image>((int)CountryListImage.CountryImage).sprite = countrySprite;
        }

        public Toggle GetToggle() => Get<Toggle>((int)CountryListToggle.CountryListUI);
    }
}
