using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace starinc.io.kingnslave
{
    public class GameManager : Singleton<GameManager>
    {
        public bool CheckCurrentVersion { get; set; } = false;

        private bool runtimeComplete;
        public bool RuntimeComplete { get { return runtimeComplete; } }

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                //if (NetworkManager.HasInstance)
                if (IsNetworkGameMode())
                {
                    if (isLoading)
                        NetworkManager.Instance.MyRunner.InvokeSceneLoadStart();
                    else
                        NetworkManager.Instance.MyRunner.InvokeSceneLoadDone();
                }
            }
        }
        public event Action StartLoadingCallback;

        public Define.SingleFirstTurnMode SingleMode { get; set; } = Define.SingleFirstTurnMode.Random;
        public int PlayerID { get; set; }
        private Define.GamePlayMode currentGameMode = Define.GamePlayMode.None;
        public Define.GamePlayMode CurrentGameMode
        {
            get { return currentGameMode; }
            set { currentGameMode = value; }
        }

        public string NextScene { get; set; }

        public GameRoomData RoomData { get; private set; }
        public string BlueTeamPlayerSid { get; private set; } = string.Empty;
        public string RedTeamPlayerSid { get; private set; } = string.Empty;

        public event Action RuntimeCompleteEvent;

        #region Single
        public int ChallengingStageIndex { get; private set; }
        public int ChallengingCycle { get; private set; } // User Stage가 0이면 1(1-1), User Stage가 49이면 5(5-10)
        public int ChallengingStageInCycle { get; private set; } // User Stage의 다음 스테이지
        // [0 ~ 100]범위 승리 확률 퍼센트
        public float SingleGameWinProbPercent
        {
            get
            {
                return Mathf.Clamp(initProbabilityPercent - decreasePercent * (ChallengingStageIndex - 1), Define.MIN_WIN_PROB_PERCENT, Define.MAX_WIN_PROB_PERCENT);
            }
        }

        private float initProbabilityPercent;
        private float decreasePercent;
        #endregion

        #region Practice
        public float PracticeGameWinProbPercent { get; set; } = Define.DEFAULT_WIN_PROBABILITY_PERCENT;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            UserDataManager.Instance.CompleteSetMyDataCallback += SetSingleProbability;
            RuntimeSetting();
        }

        async private void RuntimeSetting()
        {
            Input.multiTouchEnabled = false;
            Application.targetFrameRate = 30;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            try
            {
                //시스템 언어 감지 및 세팅
                if (!PlayerPrefs.HasKey(Define.SettingDataKey))
                {
                    FirstLaunchProcess();
                }
                await LocalizationSettings.InitializationOperation;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            RuntimeCompleteEvent?.Invoke();
            RuntimeCompleteEvent = null;
            runtimeComplete = true;
            Debug.Log("Runtime Complete");
        }

        private void FirstLaunchProcess()
        {
            //로케일 관련 세팅
            var index = Util.GetLocaleIndexBySystemLanguage(Application.systemLanguage);
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
            UserDataManager.Instance.CompleteSetMyDataCallback += SetBasicCountry;

            //설정 관련 세팅
            var data = new SettingData();
            data.LocaleIndex = index;
            AudioManager.Instance.SetMasterVolume(data.MasterVolume);
            AudioManager.Instance.SetMusicVolume(data.BGMVolume);
            AudioManager.Instance.SetSoundEffectVolume(data.SFXVolume);
            Util.SaveSettingData(data);
        }

        /// <summary>
        /// 최초 접속자의 국가 선별 함수
        /// </summary>
        private async void SetBasicCountry()
        {
            var culture = CultureInfo.CurrentCulture;
            var region = new RegionInfo(culture.Name);            
            var countryIndex = Util.GetCountryByTwoLetterISORegionName(region.TwoLetterISORegionName);
            var mySid = UserDataManager.Instance.MySid;
            await CallAPI.APIUpdateUserCountrySeq(mySid, countryIndex, async (returnCd) =>
            {
                if(returnCd == (int)Define.APIReturnCd.OK)
                {
                    await CallAPI.APISelectUser(mySid, mySid, (data) =>
                    {
                        UserDataManager.Instance.MyData.country_seq = data.country_seq;
                    });
                }
            });
        }

        public bool LoadScene(string sceneName)
        {
            if (IsLoading || sceneName.Equals(Define.LoadingSceneName))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Now Loading sceneName name : {sceneName}");
#endif
                return false;
            }

            IsLoading = true;
            NextScene = sceneName;
            StartLoadingCallback?.Invoke();
            DOTween.KillAll();

            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem != null)
                Destroy(eventSystem.gameObject);

            AudioManager.Instance.StopMusic(0.1f);
            SceneManager.LoadSceneAsync(Define.LoadingSceneName, LoadSceneMode.Additive);
            return true;
        }

        public bool IsNetworkGameMode()
        {
            byte shiftCount = 0;
            byte networkingMask = (byte)Define.GamePlayMode.NetworkModeMask;
            while (networkingMask >= 2)
            {
                networkingMask /= 2;
                ++shiftCount;
            }
            return (((byte)CurrentGameMode & (byte)Define.GamePlayMode.NetworkModeMask) >> shiftCount) == (int)Define.Boolean.TRUE;
        }

        public int GetAPIGameTypeFromGameMode()
        {
            switch (currentGameMode)
            {
                case Define.GamePlayMode.PVPNormal:
                    return (int)Define.APIGameType.Normal;
                case Define.GamePlayMode.PVPRank:
                    return (int)Define.APIGameType.Rank;
                default:
                    return 0;
            }
        }

        public void InitializeGameRoom(GameRoomData data)
        {
            RoomData = data;
            BlueTeamPlayerSid = RoomData?.sid_blue ?? string.Empty;
            RedTeamPlayerSid = RoomData?.sid_red ?? string.Empty;
        }

        public void ClearGame()
        {
            InitializeGameRoom(null);
            currentGameMode = Define.GamePlayMode.None;
            UserDataManager.Instance.MyCardSkinImageUrl = string.Empty;
            UserDataManager.Instance.MyCardSkinImage = null;
            UserDataManager.Instance.OpponentCardSkinImageList.Clear();
            UserDataManager.Instance.OpponentDataList.Clear();
            UserDataManager.Instance.OpponentProfileImageList.Clear();
            NetworkManager.Instance.ResetInitializedPlayers();
            NetworkManager.Instance.ResetLoadedePlayers();
        }

        public void SetSingleStageInfo()
        {
            Debug.Log("SingleStageSetting! " + UserDataManager.Instance.MyData.single_stage);
            ChallengingStageIndex = UserDataManager.Instance.MyData.single_stage + 1;
            ChallengingCycle = (ChallengingStageIndex - 1) / 10 + 1;
            ChallengingStageInCycle = (ChallengingStageIndex - 1) % 10 + 1;
        }

        async public void SetSingleProbability()
        {
            // API로 초기 확률 세팅
            var cdKey = Define.CDKey.single_init_win_probability.ToString();
            await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, cdKey, (obj) =>
            {
                if (obj != null)
                    initProbabilityPercent = Convert.ToSingle(obj);
            });

            // API로 확률 감소율 세팅
            cdKey = Define.CDKey.single_probability_decrease_rate.ToString();
            await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, cdKey, (obj) =>
            {
                if (obj != null)
                    decreasePercent = Convert.ToSingle(obj);
            });

            Debug.Log("value?: " + initProbabilityPercent + ", " + decreasePercent);
        }
    }
}