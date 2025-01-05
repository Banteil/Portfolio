using System;
using UnityEngine;
using UnityEngine.Events;

namespace starinc.io.kingnslave
{
    public class InGameManager : Singleton<InGameManager>
    {
        [SerializeField]
        private Txt_Round roundText;
        [SerializeField]
        private GameObject inGameBackground;

        [field: SerializeField]
        public EffectController EffectController { get; private set; }
        [field: SerializeField]
        public CardAnimController CardAnimController { get; private set; }
        public BasePlayer You { get; private set; }
        public BasePlayer Opponent { get; private set; }

        [field: SerializeField]
        public int Round { get; private set; } = 0;

        [field: SerializeField]
        public int WinConditionScore { get; protected set; } = Define.DEFAULT_WIN_CONDITION_SCORE;

        public UnityEvent NewRoundStart = new UnityEvent();
        public UnityEvent MainPhaseStart = new UnityEvent();
        public UnityEvent BattlePhaseStart = new UnityEvent();
        public UnityEvent EndPhaseStart = new UnityEvent();

        public UnityEvent<int> ClickCard = new UnityEvent<int>();
        public UnityEvent<int> ClickExpression = new UnityEvent<int>();
        public UnityEvent TimeOver = new UnityEvent();
        public UnityEvent YourSubmitStart = new UnityEvent();
        public UnityEvent OpponentSubmitStart = new UnityEvent();
        public UnityEvent<int> YourSubmitDone = new UnityEvent<int>();
        public UnityEvent<int> OpponentSubmitDone = new UnityEvent<int>();
        public UnityEvent<Define.GameResult> GameOver = new UnityEvent<Define.GameResult>();

        #region Single
        public float SingleWinProbability { get; protected set; } // [0 ~ 1] (0% ~ 100%)
        public UnityEvent AISelectAndSubmitCard = new UnityEvent();
        public UnityEvent<Define.CardType, float> AIChangeSubmitedCard = new UnityEvent<Define.CardType, float>();
        #endregion

        #region Multi
        public UnityEvent<int> MultiOpponentClickCard = new UnityEvent<int>();
        public UnityEvent<int> MultiOpponentSubmitStart = new UnityEvent<int>();
        public UnityEvent<int> MultiOpponentSubmitStartEvent = new UnityEvent<int>();
        public UnityEvent<int> MultiOpponentShowExpression = new UnityEvent<int>();
        public UnityEvent<int> MultiSetYourSpecialCardNumEvent = new UnityEvent<int>();
        public int OpponentLastMultiSpecialCardNum = -1;
        public int OpponentLastScore = 0;
        public string YourCardSubmitOrder = string.Empty;
        public string OpponentCardSubmitOrder = string.Empty;
        #endregion

        #region Ad
        public Texture AdTexture { get; private set; }
        public Action<bool> ADCallback;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            Debug.Log("Game mode: " + GameManager.Instance.CurrentGameMode);
            ResourceManager.Instance.Instantiate($"{GameManager.Instance.CurrentGameMode.ToString()}GameBackground", inGameBackground.transform, false);
            
            NewRoundStart.AddPersistentListener(MoveOnNextRound);
            NewRoundStart.AddPersistentListener(MultiResetBothSubmitOrder);
            MultiOpponentSubmitStartEvent.AddPersistentListener(MultiAddOpponentSubmitOrder);
            GameOver.AddPersistentListener(ShutDown);

            if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.PVPNormal)
            {
                AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.NormalGame));
            }
            else if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.PVPRank)
            {
                AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.RankGame));
            }
            else if (GameManager.Instance.CurrentGameMode == Define.GamePlayMode.SingleStory)
            {
                switch (GameManager.Instance.ChallengingStageInCycle)
                {
                    case < Define.MIDDLE_BOSS_STAGE:
                        AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.SingleGame_1to4));
                        break;
                    case Define.MIDDLE_BOSS_STAGE:
                        AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.SingleGame_MiddleBoss));
                        break;
                    case < Define.FINAL_BOSS_STAGE:
                        AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.SingleGame_6to9));
                        break;
                    default:
                        AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.SingleGame_FinalBoss));
                        break;
                }
                //if (GameManager.Instance.ChallengingStageInCycle < Define.MIDDLE_BOSS_STAGE)
                //{
                //    // 1 ~ 4 스테이지
                //}
                //else if (GameManager.Instance.ChallengingStageInCycle == Define.MIDDLE_BOSS_STAGE)
                //{
                //    // 중간 보스
                //    AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.SingleGame_MiddleBoss));
                //}
                //else if (GameManager.Instance.ChallengingStageInCycle < Define.FINAL_BOSS_STAGE)
                //{
                //    // 6 ~ 9 스테이지
                //    AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.SingleGame_FinalBoss));
                //}
                //else
                //{
                //    // 최종 보스
                //    AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.SingleGame_FinalBoss));
                //}
            }
            else
            {
                AudioManager.Instance.PlayBGM(ResourceManager.Instance.GetBGMClip(Define.BGMTableIndex.PracticeGame));
            }

            GetAds();
        }

        public virtual void SetPlayerYou(BasePlayer you_)
        {
            if (You != null) return;
            You = you_;
        }

        public virtual void SetPlayerOpponent(BasePlayer opponent_)
        {
            if (Opponent != null) return;
            Opponent = opponent_;
        }

        public virtual void SetKingPlayer(bool isKing) { }

        public Define.InGamePhase GetYourPhase() => You.Phase;
        public Define.InGamePhase GetOpponentPhase() => Opponent.Phase;

        public Define.CardType GetYourSubmitCardType() => You.SubmitedCard;
        public Define.CardType GetOpponentSubmitCardType() => Opponent.SubmitedCard;

        public int MultiGetYourSpecialCardNum()
        {
            if (!GameManager.Instance.IsNetworkGameMode()) { return -1; }
            return ((MultiPlayer)You).SpecialCardNum;
        }
        public int MultiGetOpponentSpecialCardNum()
        {
            if (!GameManager.Instance.IsNetworkGameMode()) { return -1; }
            return ((MultiPlayer)Opponent)?.SpecialCardNum ?? OpponentLastMultiSpecialCardNum;
        }

        public void MultiAddYourSubmitOrder(int cardNum)
        {
            YourCardSubmitOrder += GameManager.Instance.BlueTeamPlayerSid == UserDataManager.Instance.MySid ? cardNum.ToString() : (cardNum + Define.MAX_HANDS).ToString();
        }

        public void MultiAddOpponentSubmitOrder(int cardNum) =>
            OpponentCardSubmitOrder += GameManager.Instance.BlueTeamPlayerSid == UserDataManager.Instance.MySid ? (cardNum + Define.MAX_HANDS).ToString() : cardNum.ToString();

        public string MultiGetBlueTeamSubmitOrder()
        {
            string submitOrder = GameManager.Instance.BlueTeamPlayerSid == UserDataManager.Instance.MySid ? YourCardSubmitOrder : OpponentCardSubmitOrder;
            return submitOrder;
        }
        public string MultiGetRedTeamSubmitOrder()
        {
            string submitOrder = GameManager.Instance.BlueTeamPlayerSid == UserDataManager.Instance.MySid ? OpponentCardSubmitOrder : YourCardSubmitOrder;
            return submitOrder;
        }

        public int GetYourScore() => You.Score;
        public int GetOpponentScore() => Opponent?.Score ?? OpponentLastScore;

        public void MoveOnNextRound()
        {
            if (Round < int.MaxValue)
            {
                ++Round;
                roundText?.UpdateRoundText(Round);
            }
        }

        private void MultiResetBothSubmitOrder()
        {
            YourCardSubmitOrder = string.Empty;
            OpponentCardSubmitOrder = string.Empty;
        }

        private void ShutDown(Define.GameResult gameResult)
        {
            NewRoundStart.RemoveAllListeners();
            MainPhaseStart.RemoveAllListeners();
            BattlePhaseStart.RemoveAllListeners();
            EndPhaseStart.RemoveAllListeners();

            ClickCard.RemoveAllListeners();
            TimeOver.RemoveAllListeners();
            YourSubmitStart.RemoveAllListeners();
            OpponentSubmitStart.RemoveAllListeners();
            YourSubmitDone.RemoveAllListeners();
            OpponentSubmitDone.RemoveAllListeners();

            AISelectAndSubmitCard.RemoveAllListeners();
        }

        async private void GetAds()
        {
            if (UserDataManager.Instance.MyData.membership_cd != 0) return;

            var adUrl = "";
            await CallAPI.APISelectAdMedium(UserDataManager.Instance.MySid, (data) =>
            {
                adUrl = data.medium_url;
            });

            if (string.IsNullOrEmpty(adUrl)) return;
            await NetworkManager.Instance.GetTextureTask((texture) =>
            {
                AdTexture = texture;
                var settingData = Util.LoadSettingData();
                ADCallback?.Invoke(settingData.IsAdActive);
            }, adUrl);
        }
    }
}