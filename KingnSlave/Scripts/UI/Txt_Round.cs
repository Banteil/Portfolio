using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace starinc.io.kingnslave
{
    public class Txt_Round : MonoBehaviour
    {
        private const string ROUND_TEXT_KEY = "roundText";
        private TMP_Text roundText;

        private void Awake()
        {
            roundText = GetComponent<TMP_Text>();
            roundText.text = $"{Util.GetLocalizationTableString(Define.CommonLocalizationTable, ROUND_TEXT_KEY)} {0}";
            LocalizationSettings.SelectedLocaleChanged += LocalizeRoundText;
        }

        public void UpdateRoundText(int round)
        {
            roundText.text = $"{Util.GetLocalizationTableString(Define.CommonLocalizationTable, ROUND_TEXT_KEY)} {round}";
        }

        public void LocalizeRoundText(Locale locale)
        {
            UpdateRoundText(InGameManager.Instance.Round);
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= LocalizeRoundText;
        }
    }
}