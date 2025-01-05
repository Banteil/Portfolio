using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace starinc.io.kingnslave
{
    public class SingleOpponentProfile : PlayerProfile
    {
        protected override void Start()
        {
            profileUIOwner = Define.UserDataType.Opponent;
            if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory)
            {
                nickname.text = Util.GetLocalizationTableString(Define.CharacterLocalizationTable, ResourceManager.Instance.GetStageInfoData(GameManager.Instance.ChallengingStageInCycle).CharacterNameTextKey);
                profileImage.texture = ResourceManager.Instance.GetStageInfoData(GameManager.Instance.ChallengingStageInCycle).CharacterProfile;
                LocalizationSettings.SelectedLocaleChanged += LocalizeCharacterName;
            }
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= LocalizeCharacterName;
        }
        private void LocalizeCharacterName(Locale locale)
        {
            if (nickname == null)
                return;
            nickname.text = Util.GetLocalizationTableString(Define.CharacterLocalizationTable, ResourceManager.Instance.GetStageInfoData(GameManager.Instance.ChallengingStageInCycle).CharacterNameTextKey);
        }
    }
}