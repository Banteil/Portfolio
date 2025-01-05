using System;
using System.Collections;
using System.Text;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public class MultiPlayer : BasePlayer
    {
        public int SpecialCardNum { get; protected set; }

        private static byte startStatePlayerCount = 0;
        private static byte mainDoneStatePlayerCount = 0;
        private Coroutine gameLoop;
        private string cardArray;
        private bool isSpecialCardSended = false;

        protected override void Start()
        {
            //Spawned()에서 Start 대신 처리
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            InGameManager.Instance.OpponentLastScore = Score;
            InGameManager.Instance.OpponentLastMultiSpecialCardNum = SpecialCardNum;
        }

        public override void Spawned()
        {
            base.Start();
            InGameManager.Instance.GameOver.AddPersistentListener(ShutDown);
            Phase = Define.InGamePhase.EndPhase;
            if (Object.HasStateAuthority)
            {
                Debug.Log("YouSpawn");
                gameObject.name = "Player";
                InGameManager.Instance.SetPlayerYou(GetComponent<MultiPlayer>());
                InGameManager.Instance.MainPhaseStart.AddPersistentListener(MoveOnMainPhase);
                InGameManager.Instance.ClickCard.AddPersistentListener(RPC_InvokeOpponentClickCard);
                InGameManager.Instance.ClickExpression.AddPersistentListener(RPC_InvokeOpponentShowExpression);
                InGameManager.Instance.MultiOpponentSubmitStart.AddPersistentListener(RPC_InvokeMultiOpponentSubmitStart);
                InGameManager.Instance.YourSubmitDone.AddPersistentListener(MoveOnMainPhaseEnd);
                InGameManager.Instance.YourSubmitDone.AddPersistentListener(RPC_SetSubmitedCard);
                InGameManager.Instance.YourSubmitDone.AddPersistentListener(RPC_InvokeOpponentSubmitDone);
                InGameManager.Instance.BattlePhaseStart.AddPersistentListener(MoveOnBattlePhase);
                InGameManager.Instance.EndPhaseStart.AddPersistentListener(MoveOnEndPhase);
                InGameManager.Instance.MultiSetYourSpecialCardNumEvent.AddPersistentListener(SetSpecialCardNum);
                gameLoop = StartCoroutine(GameLoop());
            }
            else
            {
                Debug.Log("OppoSpawn");
                gameObject.name = "OpponentPlayer";
                InGameManager.Instance.SetPlayerOpponent(GetComponent<MultiPlayer>());
            }
        }

        public override IEnumerator GameLoop()
        {
            BattleResult battleResult = new BattleResult(WinScore.None, BattleWinner.None);
            yield return new WaitUntil(() => InGameManager.Instance.Opponent != null);
            yield return new WaitForEndOfFrame();
            while (true)
            {
                //Start Phase
                mainDoneStatePlayerCount = 0;
                MoveOnStartPhase();
                yield return new WaitUntil(() => startStatePlayerCount >= Define.MAX_GAME_PLAYER);

                //Check New Round Start
                if (battleResult.Winner != BattleWinner.None || InGameManager.Instance.Round == 0)
                {
                    isSpecialCardSended = false;
                    RPC_CheckSwapKingPlayer(InGameManager.Instance.Round);
                    AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.StartNewRound));
                    InGameManager.Instance.NewRoundStart?.Invoke();
                    battleResult = new BattleResult();
                }

                //Main Phase
                yield return new WaitForEndOfFrame();
                InGameManager.Instance.MainPhaseStart?.Invoke();

                //Battle Phase
                yield return new WaitUntil(() => (mainDoneStatePlayerCount >= Define.MAX_GAME_PLAYER) && isSpecialCardSended);
                startStatePlayerCount = 0;
                Debug.Log("OppoSCard:" + InGameManager.Instance.MultiGetOpponentSpecialCardNum() + "/ YourSCard:" + InGameManager.Instance.MultiGetYourSpecialCardNum());
                yield return new WaitForEndOfFrame();
                InGameManager.Instance.BattlePhaseStart?.Invoke();
                Battle(out battleResult, SubmitedCard, InGameManager.Instance.GetOpponentSubmitCardType());
                RPC_SetScoreToProxies(Score);
                // 승부가 났을 때
                if (battleResult.Winner != BattleWinner.None)
                {
                    SetCardArray();
                    try
                    {
                        string winnerSid = battleResult.Winner == BattleWinner.You ? UserDataManager.Instance.MySid : UserDataManager.Instance.OpponentDataList[0].sid;
                        string loserSid = battleResult.Winner == BattleWinner.You ? UserDataManager.Instance.OpponentDataList[0].sid : UserDataManager.Instance.MySid;
                        CallAPI.APIGameRoomFinishRound(UserDataManager.Instance.MyData, UserDataManager.Instance.OpponentDataList[0], cardArray,
                            InGameManager.Instance.MultiGetBlueTeamSubmitOrder(), InGameManager.Instance.MultiGetRedTeamSubmitOrder(), winnerSid, loserSid);

                        // 게임 최종 결과가 결정됐을 때 API 호출
                        if (Score >= Define.DEFAULT_WIN_CONDITION_SCORE || InGameManager.Instance.GetOpponentScore() >= Define.DEFAULT_WIN_CONDITION_SCORE)
                        {
                            NetworkManager.Instance.CallGameOverAPI(battleResult.Winner);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                //End Phase
                yield return new WaitUntil(() => Phase == Define.InGamePhase.EndPhase);
                yield return new WaitForEndOfFrame();
                if (Score >= Define.DEFAULT_WIN_CONDITION_SCORE)
                {
                    InGameManager.Instance.GameOver?.Invoke(Define.GameResult.Victory);
                    break;
                }
                else if (InGameManager.Instance.GetOpponentScore() >= Define.DEFAULT_WIN_CONDITION_SCORE)
                {
                    InGameManager.Instance.GameOver?.Invoke(Define.GameResult.Defeat);
                    break;
                }

                yield return null;
            }
        }

        private void SetSpecialCardNum(int specialCardNum_)
        {
            SpecialCardNum = specialCardNum_;
            RPC_SetSpecialCardNumToProxies(SpecialCardNum);
            isSpecialCardSended = true;
        }

        private void SetCardArray()
        {
            cardArray = "CCCCCCCCCC";
            StringBuilder cardArrayBuilder = new StringBuilder(cardArray);
            if (GameManager.Instance.BlueTeamPlayerSid == UserDataManager.Instance.MySid)
            {
                cardArrayBuilder[InGameManager.Instance.MultiGetYourSpecialCardNum()] = IsKingPlayer ? (char)Define.APICardCd.King : (char)Define.APICardCd.Slave;
                cardArrayBuilder[InGameManager.Instance.MultiGetOpponentSpecialCardNum() + Define.MAX_HANDS] = IsKingPlayer ? (char)Define.APICardCd.Slave : (char)Define.APICardCd.King;
            }
            else
            {
                cardArrayBuilder[InGameManager.Instance.MultiGetYourSpecialCardNum() + Define.MAX_HANDS] = IsKingPlayer ? (char)Define.APICardCd.King : (char)Define.APICardCd.Slave;
                cardArrayBuilder[InGameManager.Instance.MultiGetOpponentSpecialCardNum()] = IsKingPlayer ? (char)Define.APICardCd.Slave : (char)Define.APICardCd.King;
            }
            cardArray = cardArrayBuilder.ToString();
            Debug.Log("<color=red>CardArray:</color>" + cardArray);
        }

        public async void ShutDown(Define.GameResult gameResult)
        {
            if (gameObject.Equals(null) || gameLoop == null) return;
            StopCoroutine(gameLoop);
            await NetworkManager.Instance.Shutdown();
        }

        /// <summary>
        /// 멀티플레이용 CheckSwapKingPlayer RPC 메서드
        /// </summary>
        /// <param name="roundBeforeStart"></param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.StateAuthority | RpcTargets.Proxies)]
        public override void RPC_CheckSwapKingPlayer(int roundBeforeStart)
        {
            base.RPC_CheckSwapKingPlayer(roundBeforeStart);
        }

        /// <summary>
        /// 멀티플레이용 StartPhase 전환 메서드
        /// </summary>
        public override void MoveOnStartPhase()
        {
            Phase = Define.InGamePhase.StartPhase;
            MultiPlayer.RPC_AddStartStatePlayer(NetworkManager.Instance.MyRunner);
            Debug.Log($"start: {startStatePlayerCount} {Object.HasStateAuthority}");
        }

        /// <summary>
        /// 멀티플레이용 EndPhase 전환 메서드
        /// </summary>
        public override void MoveOnMainPhaseEnd(int cardEnum)
        {
            Phase = Define.InGamePhase.MainPhaseDone;
            MultiPlayer.RPC_AddMainDoneStatePlayer(NetworkManager.Instance.MyRunner);
            Debug.Log($"mainDone: {mainDoneStatePlayerCount}");
        }

        [Rpc]
        public static void RPC_AddStartStatePlayer(NetworkRunner myRunner, RpcInfo rpcInfo = default)
        {
            ++startStatePlayerCount;
        }

        [Rpc]
        public static void RPC_AddMainDoneStatePlayer(NetworkRunner myRunner, RpcInfo rpcInfo = default)
        {
            ++mainDoneStatePlayerCount;
        }

        /// <summary>
        /// 멀티플레이용 제출 카드 번호 결정 및 전달 RPC 메서드
        /// </summary>
        /// <param name="cardEnum"></param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.StateAuthority | RpcTargets.Proxies)]
        public override void RPC_SetSubmitedCard(int cardEnum) => base.RPC_SetSubmitedCard(cardEnum);

        /// <summary>
        /// 멀티플레이용 상대방에게 제출 상태 필드 업데이트 RPC 메서드
        /// </summary>
        /// <param name="cardTypeEnum"></param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.Proxies)]
        public void RPC_InvokeOpponentSubmitDone(int cardTypeEnum)
        {
            if (Object.HasStateAuthority)
            {
                return;
            }
            InGameManager.Instance.OpponentSubmitDone?.Invoke(cardTypeEnum);
        }

        /// <summary>
        /// 멀티플레이용 상대방에게 제출 상태 패 업데이트 RPC 메서드
        /// </summary>
        /// <param name="cardNum"></param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.Proxies)]
        public void RPC_InvokeMultiOpponentSubmitStart(int cardNum)
        {
            if (Object.HasStateAuthority)
            {
                return;
            }
            InGameManager.Instance.MultiOpponentSubmitStartEvent?.Invoke(cardNum);
        }

        /// <summary>
        /// 멀티플레이용 상대방에게 선택 상태 업데이트 RPC 메서드
        /// </summary>
        /// <param name="cardNum"></param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.Proxies)]
        public void RPC_InvokeOpponentClickCard(int cardNum)
        {
            if (Object.HasStateAuthority)
            {
                return;
            }
            InGameManager.Instance.MultiOpponentClickCard?.Invoke(cardNum);
        }

        /// <summary>
        /// 멀티플레이용 상대방에게 감정 표현 업데이트 RPC 메서드
        /// </summary>
        /// <param name="cardNum"></param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.Proxies)]
        public void RPC_InvokeOpponentShowExpression(int expression)
        {
            if (Object.HasStateAuthority)
            {
                return;
            }
            InGameManager.Instance.MultiOpponentShowExpression?.Invoke(expression);
        }

        /// <summary>
        /// 멀티플레이용 상대방에게 패 배열을 전달하는 RPC 메서드
        /// </summary>
        /// <param name="score_"></param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.Proxies)]
        public void RPC_SetSpecialCardNumToProxies(int specialCardNum_)
        {
            if (Object.HasStateAuthority) { return; }
            SpecialCardNum = specialCardNum_;
        }

        /// <summary>
        /// 멀티플레이용 상대방에게 점수를 전달하는 RPC 메서드
        /// </summary>
        /// <param name="score_"></param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.Proxies)]
        public void RPC_SetScoreToProxies(int score_)
        {
            if (Object.HasStateAuthority) { return; }
            Score = score_;
        }

        // 인게임 중 내 연결 끊김 후 복귀 처리
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                if (!GameManager.Instance.IsNetworkGameMode() || !gameObject.scene.isLoaded)
                    return;

                CheckSession();
            }
        }

        private async void CheckSession()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            await UniTask.Yield(PlayerLoopTiming.Update);

            if (!NetworkManager.Instance.HasRunner)
            {
                NetworkManager.Instance.CallGameOverWhenPlayerLeft(true);
                return;
            }

            if (NetworkManager.Instance.MyRunner.SessionInfo.PlayerCount < Define.MAX_GAME_PLAYER
                && InGameManager.Instance.GetOpponentScore() < Define.DEFAULT_WIN_CONDITION_SCORE && InGameManager.Instance.GetYourScore() < Define.DEFAULT_WIN_CONDITION_SCORE)
            {
                Debug.LogWarning($"The game is already finished {NetworkManager.Instance.MyRunner.SessionInfo.PlayerCount} < {Define.MAX_GAME_PLAYER} ?");
                NetworkManager.Instance.CallGameOverWhenPlayerLeft(true);
                return;
            }
        }
    }
}