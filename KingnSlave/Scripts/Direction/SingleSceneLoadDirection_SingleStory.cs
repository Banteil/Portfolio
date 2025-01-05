using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class SingleSceneLoadDirection_SingleStory : BaseDirection
    {
        [SerializeField]
        private Image background;
        [SerializeField]
        private Image backgroundBottom;
        [SerializeField]
        private Image character;
        [SerializeField]
        private TMP_Text currentStageInfo;
        [SerializeField]
        private TMP_Text characterName;
        [SerializeField]
        private TMP_Text winProbabilityText;
        [SerializeField]
        private List<Image> loadingUIList;
        [SerializeField]
        private GameObject gems;

        protected override void OnAwake()
        {
            base.OnAwake();
            GameManager.Instance.SetSingleStageInfo();
            StageInfoData stageInfo = ResourceManager.Instance.GetStageInfoData(GameManager.Instance.ChallengingStageInCycle);
            background.sprite = stageInfo.LoadingBackground;
            backgroundBottom.sprite = stageInfo.LoadingBackgroundBottom;

            // 보상젬 갯수 표시
            var rewardGem = 0;
            if (GameManager.Instance.ChallengingStageInCycle == Define.FINAL_BOSS_STAGE)
                rewardGem = Define.FINAL_BOSS_REWARD_GEM;
            else if (GameManager.Instance.ChallengingStageInCycle == Define.MIDDLE_BOSS_STAGE)
                rewardGem = Define.MIDDLE_BOSS_REWARD_GEM;
            for (int i = 0; i < Mathf.Min(gems.transform.childCount, rewardGem); i++)
                gems.transform.GetChild(i).gameObject.SetActive(true);

            character.sprite = stageInfo.Character;
            currentStageInfo.text = $"{GameManager.Instance.ChallengingCycle} - {GameManager.Instance.ChallengingStageInCycle}";
            characterName.text = Util.GetLocalizationTableString(Define.CharacterLocalizationTable, stageInfo.CharacterNameTextKey);
            winProbabilityText.text = $"{GameManager.Instance.SingleGameWinProbPercent.ToString("F2")}%";
            foreach (var ui in loadingUIList)
                ui.sprite = stageInfo.LoadingUI;
        }
    }
}