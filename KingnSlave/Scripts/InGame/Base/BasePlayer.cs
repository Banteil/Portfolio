using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    public struct BattleResult
    {
        public BattleResult(WinScore scoreAddition_, BattleWinner winner_)
        {
            ScoreAddition = scoreAddition_;
            Winner = winner_;
        }

        public WinScore ScoreAddition { get; private set; }
        public BattleWinner Winner { get; private set; }
    }

    public enum BattleWinner
    {
        None = 0,
        You,
        Opponent
    }

    public enum WinScore
    {
        None = 0,
        CitizenWin = 1,
        KingWin = 2,
        SlaveWin = 4
    }

    public class BasePlayer : NetworkBehaviour
    {
        [field: SerializeField]
        public bool IsKingPlayer { get; protected set; }
        [field: SerializeField]
        public int Score { get; protected set; }
        public Define.CardType SubmitedCard { get; protected set; }
        public Define.InGamePhase Phase { get; protected set; }

        protected Dictionary<(Define.CardType yourCard, Define.CardType opponentCard), BattleResult> battleCalculator;

        protected virtual void Start()
        {
            InitializeBattleCalculator();
        }

        public virtual void SetFirstTurnCard(bool isKingPlayer_)
        {
            if (InGameManager.Instance.Round != 0)
            {
                return;
            }

            IsKingPlayer = isKingPlayer_;
        }

        public virtual IEnumerator GameLoop()
        {
            while (true)
            {
                yield return null;
            }
        }

        public virtual void RPC_CheckSwapKingPlayer(int roundBeforeStart)
        {
            int prevRound = roundBeforeStart;
            if (prevRound % 2 == 1 || prevRound == 0)
            {
                return;
            }
            IsKingPlayer = !IsKingPlayer;
        }

        /// <summary>
        /// 배틀 결과 반환 메서드
        /// </summary>
        /// <returns></returns>
        public void Battle(out BattleResult battleResult, Define.CardType yourCard, Define.CardType opponentCard)
        {
            battleResult = new BattleResult(WinScore.None, BattleWinner.None);
            battleCalculator.TryGetValue((yourCard, opponentCard), out battleResult);
            Score += (int)battleResult.ScoreAddition;
        }

        public virtual void RPC_SetSubmitedCard(int cardEnum)
        {
            SubmitedCard = (Define.CardType)cardEnum;
        }

        public virtual void MoveOnStartPhase() => Phase = Define.InGamePhase.StartPhase;
        public virtual void MoveOnMainPhase() => Phase = Define.InGamePhase.MainPhase;
        public virtual void MoveOnBattlePhase() => Phase = Define.InGamePhase.BattlePhase;
        public virtual void MoveOnEndPhase() => Phase = Define.InGamePhase.EndPhase;
        public virtual void MoveOnMainPhaseEnd(int cardEnum) => Phase = Define.InGamePhase.MainPhaseDone;

        protected virtual void InitializeBattleCalculator()
        {
            battleCalculator = new Dictionary<(Define.CardType yourCard, Define.CardType opponentCard), BattleResult>();
            //Draw
            battleCalculator.Add((Define.CardType.Citizen, Define.CardType.Citizen), new BattleResult(WinScore.None, BattleWinner.None));

            //Lose
            battleCalculator.Add((Define.CardType.Slave, Define.CardType.Citizen), new BattleResult(WinScore.None, BattleWinner.Opponent));
            battleCalculator.Add((Define.CardType.Citizen, Define.CardType.King), new BattleResult(WinScore.None, BattleWinner.Opponent));
            battleCalculator.Add((Define.CardType.King, Define.CardType.Slave), new BattleResult(WinScore.None, BattleWinner.Opponent));

            //Win
            battleCalculator.Add((Define.CardType.Slave, Define.CardType.King), new BattleResult(WinScore.SlaveWin, BattleWinner.You));
            battleCalculator.Add((Define.CardType.Citizen, Define.CardType.Slave), new BattleResult(WinScore.CitizenWin, BattleWinner.You));
            battleCalculator.Add((Define.CardType.King, Define.CardType.Citizen), new BattleResult(WinScore.KingWin, BattleWinner.You));
        }
    }
}