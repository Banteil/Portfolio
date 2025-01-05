using System.Collections;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class SingleOpponent : BasePlayer
    {
        private const float MAX_AI_SUBMITTING_DELAY = 2f;
        private const float MIN_AI_SUBMITTING_DELAY = 0.1f;

        private Coroutine gameLoop;
        private WaitUntil waitStartPhase;
        private WaitUntil waitMainPhase;
        private WaitUntil waitBattlePhase;
        private WaitUntil waitEndPhase;

        protected override void Start()
        {
            base.Start();

            waitStartPhase = new WaitUntil(() => Phase == Define.InGamePhase.EndPhase && InGameManager.Instance.GetOpponentPhase() == Define.InGamePhase.EndPhase);
            waitMainPhase = new WaitUntil(() => Phase == Define.InGamePhase.StartPhase && InGameManager.Instance.GetYourPhase() == Define.InGamePhase.StartPhase);
            waitBattlePhase = new WaitUntil(() => Phase == Define.InGamePhase.MainPhaseDone && InGameManager.Instance.GetYourPhase() == Define.InGamePhase.MainPhaseDone);
            waitEndPhase = new WaitUntil(() => Phase == Define.InGamePhase.EndPhase);

            InGameManager.Instance.MainPhaseStart.AddPersistentListener(MoveOnMainPhase);
            InGameManager.Instance.OpponentSubmitDone.AddPersistentListener(MoveOnMainPhaseEnd);
            InGameManager.Instance.OpponentSubmitDone.AddPersistentListener(RPC_SetSubmitedCard);
            InGameManager.Instance.BattlePhaseStart.AddPersistentListener(MoveOnBattlePhase);
            InGameManager.Instance.EndPhaseStart.AddPersistentListener(MoveOnEndPhase);
            InGameManager.Instance.GameOver.AddPersistentListener(ShutDown);

            gameLoop = StartCoroutine(GameLoop());
        }

        public override IEnumerator GameLoop()
        {
            BattleResult battleResult = new BattleResult(WinScore.None, BattleWinner.None);
            float randomTime;
            MoveOnEndPhase();

            while (true)
            {
                //Start Phase
                yield return waitStartPhase;
                MoveOnStartPhase();

                //Main Phase
                yield return waitMainPhase;
                yield return CoroutineUtilities.WaitForEndOfFrame;

                randomTime = Random.Range(MIN_AI_SUBMITTING_DELAY, MAX_AI_SUBMITTING_DELAY);
                yield return new WaitForSeconds(randomTime);
                InGameManager.Instance.AISelectAndSubmitCard?.Invoke();

                //Battle Phase
                yield return waitBattlePhase;
                yield return CoroutineUtilities.WaitForEndOfFrame;
                Battle(out battleResult, SubmitedCard, InGameManager.Instance.GetYourSubmitCardType());

                //End Phase
                yield return waitEndPhase;
                yield return CoroutineUtilities.WaitForEndOfFrame;
                if (Score >= InGameManager.Instance.WinConditionScore)
                {
                    InGameManager.Instance.GameOver?.Invoke(Define.GameResult.Defeat);
                    break;
                }
            }
        }

        public void ShutDown(Define.GameResult gameResult)
        {
            StopCoroutine(gameLoop);
        }
    }
}