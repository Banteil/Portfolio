using System.Collections;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class SinglePlayer : BasePlayer
    {
        private Coroutine gameLoop;
        private WaitUntil waitStartPhase;
        private WaitUntil waitMainPhase;
        private WaitUntil waitBattlePhase;
        private WaitUntil waitEndPhase;

        protected override void Start()
        {
            base.Start();

            waitStartPhase = new WaitUntil(() => Phase == Define.InGamePhase.EndPhase && InGameManager.Instance.GetOpponentPhase() == Define.InGamePhase.EndPhase);
            waitMainPhase = new WaitUntil(() => Phase == Define.InGamePhase.StartPhase && InGameManager.Instance.GetOpponentPhase() == Define.InGamePhase.StartPhase);
            waitBattlePhase = new WaitUntil(() => Phase == Define.InGamePhase.MainPhaseDone && InGameManager.Instance.GetOpponentPhase() == Define.InGamePhase.MainPhaseDone);
            waitEndPhase = new WaitUntil(() => Phase == Define.InGamePhase.EndPhase);

            InGameManager.Instance.MainPhaseStart.AddPersistentListener(MoveOnMainPhase);
            InGameManager.Instance.YourSubmitDone.AddPersistentListener(MoveOnMainPhaseEnd);
            InGameManager.Instance.YourSubmitDone.AddPersistentListener(RPC_SetSubmitedCard);
            InGameManager.Instance.BattlePhaseStart.AddPersistentListener(MoveOnBattlePhase);
            InGameManager.Instance.EndPhaseStart.AddPersistentListener(MoveOnEndPhase);
            InGameManager.Instance.GameOver.AddPersistentListener(ShutDown);

            gameLoop = StartCoroutine(GameLoop());
        }

        public override IEnumerator GameLoop()
        {
            BattleResult battleResult = new BattleResult(WinScore.None, BattleWinner.None);
            yield return new WaitForEndOfFrame();
            MoveOnEndPhase();
            InGameManager.Instance.NewRoundStart?.Invoke();
            while (true)
            {
                //Start Phase
                yield return waitStartPhase;
                MoveOnStartPhase();

                //Check New Round Start
                if (battleResult.Winner != BattleWinner.None)
                {
                    RPC_CheckSwapKingPlayer(InGameManager.Instance.Round);
                    InGameManager.Instance.Opponent.RPC_CheckSwapKingPlayer(InGameManager.Instance.Round);
                    AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.StartNewRound));
                    InGameManager.Instance.NewRoundStart?.Invoke();
                }

                //Main Phase
                yield return waitMainPhase;
                yield return CoroutineUtilities.WaitForEndOfFrame;
                InGameManager.Instance.MainPhaseStart?.Invoke();

                //Battle Phase
                yield return waitBattlePhase;
                yield return CoroutineUtilities.WaitForEndOfFrame;

                InGameManager.Instance.AIChangeSubmitedCard?.Invoke(SubmitedCard, SingleGameManager.Instance.SingleWinProbability);
                InGameManager.Instance.BattlePhaseStart?.Invoke();
                Battle(out battleResult, SubmitedCard, InGameManager.Instance.GetOpponentSubmitCardType());

                //End Phase
                yield return waitEndPhase;
                yield return CoroutineUtilities.WaitForEndOfFrame;
                if (Score >= InGameManager.Instance.WinConditionScore)
                {
                    if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory)
                    {
                        //   스테이지 갱신 API
                        //   스테이지 체크 -> 젬 획득 API
                        //   게임 종료 이벤트 콜백

                        UserDataManager.Instance.ClearStage(GameManager.Instance.ChallengingStageIndex, () =>
                        {
                            InGameManager.Instance.GameOver?.Invoke(Define.GameResult.Victory);
                        });
                    }
                    else
                    {
                        InGameManager.Instance.GameOver?.Invoke(Define.GameResult.Victory);
                    }
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