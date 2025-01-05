namespace starinc.io
{
    public class Define
    {
        public const string SUCCESS = "SUCCESS";
        public const string FAIL = "FAIL";

        public const int REFERENCE_WIDTH = 1080;
        public const int REFERENCE_HEIGHT = 1920;

        public const int DIRECTION_SORT_ORDER = 9000;
        public const int LOADING_SORT_ORDER = 9100;

        public const int MIDDLE_BOSS_STAGE = 5;
        public const int FINAL_BOSS_STAGE = 10;
        public const int MIDDLE_BOSS_REWARD_GEM = 1;
        public const int FINAL_BOSS_REWARD_GEM = 2;
        public const float MIN_WIN_PROB_PERCENT = 1f;
        public const float MAX_WIN_PROB_PERCENT = 100f;

        public const int MAX_GAME_PLAYER = 2;
        public const int MAX_HANDS = 5;
        public const float DEFAULT_WIN_PROBABILITY_PERCENT = 50f;
        public const int DEFAULT_WIN_CONDITION_SCORE = 10;
        public const float MAX_ROUND_TIME = 10f;
        public const float EXPESSION_REMAINING_TIME = 2f;
        public const float OPEN_WAITING_TIME = 0.2f;
        public const float CARD_OPENNING_TIME = 0.5f;

        public const float BATTLE_ANIMATION_TIME = 2f;
        public const float BATTLE_DRAW_ANIMATION_TIME = 1f;

        public const string AD_REWARD_RECIEVED_KEY = "watchAdCompletedText";

        public const int CheckCcuLimitCount = 120;

        #region Locale Table Name
        public const string CommonLocalizationTable = "CommonUITable";
        public const string InGameLocalizationTable = "InGameUITable";
        public const string TierNameLocalizationTable = "TierNameTable";
        public const string InfomationLocalizationTable = "InfomationTable";
        public const string CharacterLocalizationTable = "CharacterTable";
        public const string PushTable = "PushTable";
        #endregion

        #region Scene Name
        public const string LobbySceneName = "LobbyScene";
        public const string LoadingSceneName = "LoadingScene";
        public const string SingleGameSceneName = "SingleGameScene";
        public const string MultiGameSceneName = "MultiGameScene";
        #endregion

        #region WebView URL
        public const string HelpURL = "https://kingnslave.com/help";
        public const string TermsOfUseURL = "https://kingnslave.com/terms";
        public const string PrivacyPolicyURL = "https://kingnslave.com/privacy";
        public const string NoticeURL = "https://kingnslave.com/notice";
        public const string WithdrawalURL = "https://kingnslave.com/delete";
        #endregion

        #region PlayerPref Key
        public const string KEY_SID = "SID";
        public const string KEY_UserData = "UserData";
        public const string SettingDataKey = "SettingData";
        #endregion

        public enum CDKey
        {
            ad_day_limit,
            ad_reward_gem,
            ccu_limit,
            rank_precondition,
            enable_version_check,
            client_version_android,
            client_version_ios,
            ios_get_gem_reward_id,
            android_get_gem_reward_id,
            test_get_gem_reward_id,
            ios_market_url,
            android_market_url,
            single_middle_boss_reward_gem,
            single_boss_reward_gem,
            single_init_win_probability,
            single_probability_decrease_rate
        }

        public enum Boolean
        {
            FALSE = 0,
            TRUE = 1,
        }

        public enum UIEvent
        {
            Click,
            Drag,
            EndDrag,
        }

        public enum MouseEvent
        {
            Press,
            Click,
        }

        public enum PlayerType : int
        {
            Player,
            Observer
        }

        public enum GamePlayMode
        {
            None = 0b_0000,
            Practice = 0b_0001,
            SingleStory = 0b_0010,
            NetworkModeMask = 0b_1000,
            PVPNormal= 0b_1001,
            PVPRank = 0b_1010,
        }

        public enum APIGameType
        {
            None,
            Normal = 1,
            Rank = 2
        }

        public enum SingleFirstTurnMode
        {
            Random = 0,
            King,
            Slave
        }

        public enum InGamePhase
        {
            StartPhase,
            MainPhase,
            MainPhaseDone,
            BattlePhase,
            EndPhase
        }

        public enum CardType
        {
            None = -1,
            King = 0,
            Slave,
            Citizen
        }

        public enum GameResult
        {
            Defeat,
            Victory
        }

        public enum InGameAction
        {
            Select,
            Submit
        }

        public enum UserDataType
        {
            You,
            Opponent
        }

        public enum PlayerTeamType
        {
            Red,
            Blue
        }

        public enum BGMTableIndex
        {
            LobbyScene = 0,
            PracticeGame,
            NormalGame,
            RankGame,
            SingleGame_1to4,
            SingleGame_MiddleBoss,
            SingleGame_6to9,
            SingleGame_FinalBoss
        }

        public enum SFXTableIndex
        {
            ClickCloseButton = 0,
            ClickNormalButton,
            BuyItem,
            TimerAlarm,
            SubmitCard,
            OpenCard,
            SelectCard,
            StartNewRound,
            Victory,
            Defeat,
            CitizenVsCitizen, // 10
            CitizenVsSlave,
            KingVsCitizen,
            SlaveVsKing
        }

        public enum APICardCd
        {
            King = 'K',
            Slave = 'S',
            Citizen = 'C',
        }

        public enum APIActionCd
        {
            select,
            submit,
            expression
        }

        public enum APIReturnCd
        {
            OK = 0,
            Wrong = 1,
            Id_Duplicated = 2
        }

        public enum TierKey
        {
            unRankedKey,
            ironKey,
            bronzeKey,
            silverKey,
            goldKey,
            platinumKey,
            emeraldKey,
            diamondKey,
            masterkey,
            grandMasterKey,
            challengerKey,
        }

        public enum LoginType
        {
            None,
            Email,
            Google,
            Apple,
            Facebook,
        }

        public enum UidExceptionHandling
        {
            uidNoissues,
            uidLengthIssue,
            uidFormatIssue,
            uidDuplicatedIssue,
            uidInvalidIssue,
        }

        public enum EmailExceptionHandling
        {
            emailNoissues,
            emailLengthIssue,
            emailFormatIssue,
        } 
        
        public enum VerificationCodeExceptionHandling
        {
            verificationCodeNoissues,
            verificationEmptyIssue,
            sendEmailVerificationCodeSuccess,
            sendEmailVerificationCodeFail,
            checkVerificationCodeSuccess,
            checkVerificationCodeFail,
        }

        public enum PasswordExceptionHandling
        {
            passwordNoissues,
            passwordLengthIssue,
            passwordFormatIssue,
            passwordInvalidIssue,
            passwordModifySuccess,
        }

        public enum ProfileImageExceptionHandling
        {
            profileImageModifySuccess,
            profileImageModifyFail,
            profileImageBelowTier,
        }

        public enum ExpressionCd
        {
            king,
            slave,
            citizen,
            hi,
            wow,
            sorry,
            smile,
            anger,
            cry,
            despair,
            beg
        }

        public enum LocaleName
        {
            English = 0,
            Japanese = 8,
            Korean = 9,
            ChineseSimplified = 12,
            ChineseTraditional = 13,
        }

        public enum ItemType
        { 
            All,
            Gem,
            Expression,
            CardSkin,
            ProfileImage,
        }

        public enum ItemCardSkin
        {
            Basic = 16,
        }

        public enum GemLogType
        {
            MiddleBossClear = 3,
            MiddleBossAddition = 4,
            BossClear = 5,
            BossAddition = 6
        }
    }
}