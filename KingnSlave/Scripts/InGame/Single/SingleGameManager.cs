using TMPro;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class SingleGameManager : InGameManager
    {
        [SerializeField]
        private TMP_Text winProb;

        protected override void OnAwake()
        {
            base.OnAwake();
            if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory)
            {
                SingleWinProbability = GameManager.Instance.SingleGameWinProbPercent * 0.01f; // [0 ~ 1] (0% ~ 100%)
                WinConditionScore = ResourceManager.Instance.GetStageInfoData(GameManager.Instance.ChallengingStageInCycle).MaxScore;
            }
            else
            {
                SingleWinProbability = GameManager.Instance.PracticeGameWinProbPercent * 0.01f; // [0 ~ 1] (0% ~ 100%)
            }

            winProb.text = $"{(SingleWinProbability * 100).ToString("F2")}%";
        }

        public override void SetKingPlayer(bool youreKing)
        {
            You.SetFirstTurnCard(youreKing);
            Opponent.SetFirstTurnCard(!youreKing);
        }
    }
}