using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class LabelToggle : Toggle
    {
        [SerializeField] private TextMeshProUGUI _label;
        public string IsOnTextKey, IsOffTextKey;

        protected override void Start()
        {
            base.Start();
            ChangeLabel(isOn);
            onValueChanged.AddListener(ChangeLabel);
            LocalizationSettings.SelectedLocaleChanged += LocalizeLabel;
        }

        private void ChangeLabel(bool isOn)
        {
            if (_label == null) return;
            _label.text = isOn ? Util.GetLocalizationTableString(Define.CommonLocalizationTable, IsOnTextKey) : Util.GetLocalizationTableString(Define.CommonLocalizationTable, IsOffTextKey);
        }

        private void LocalizeLabel(Locale locale)
        {
            if (_label == null) return;
            _label.text = isOn ? Util.GetLocalizationTableString(Define.CommonLocalizationTable, IsOnTextKey) : Util.GetLocalizationTableString(Define.CommonLocalizationTable, IsOffTextKey);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LocalizationSettings.SelectedLocaleChanged -= LocalizeLabel;
        }
    }
}
