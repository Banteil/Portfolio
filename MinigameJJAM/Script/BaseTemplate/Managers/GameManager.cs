using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace starinc.io
{
    public class GameManager : BaseManager
    {
        #region Cache
        public MinigameData Minigames { get; private set; }
        public HighScoreData Scores { get; private set; }

        public int CurrentGameAddress = -1;
        public bool CompletePreparedData { get; private set; }
        #endregion

        #region Callback
        public event Action<SettingData> OnLoadSettingData;
        public event Action OnChangedLocale;
        #endregion

        protected override void OnAwake()
        {
            Application.targetFrameRate = 30;
            base.OnAwake(); 
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        protected override void OnStart()
        {
            PrepareRequiredData();
        }

        public bool HasData<T>() where T : class
        {
            var key = typeof(T).FullName;
            return PlayerPrefs.HasKey(key);
        }

        public void SaveData<T>(T data) where T : class
        {
            try
            {
                var json = Util.ObjectToJson(data);
                var key = typeof(T).FullName;
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save data: {ex.Message}");
            }
        }

        public T LoadData<T>() where T : new()
        {
            try
            {
                var key = typeof(T).FullName;
                if (PlayerPrefs.HasKey(key))
                {
                    string json = PlayerPrefs.GetString(key);
                    return JsonUtility.FromJson<T>(json);
                }
                else
                {
                    return new T();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load data: {ex.Message}");
                return new T();
            }
        }

        private async void PrepareRequiredData()
        {
            await UniTask.WaitUntil(() => Manager.User.CompletePreparedUserInfo);
            await SettingInitialization();
            await SettingMinigameData();
            await SettingScoreData();
            CompletePreparedData = true;
        }

        #region SettingInfo    
        private async UniTask SettingInitialization()
        {
            await LocalizationSettings.InitializationOperation.Task;
            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                Debug.Log("Localization Tables loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to load Localization Tables.");
            }

            var settingData = LoadData<SettingData>();
            OnLoadSettingData?.Invoke(settingData);
            
            var locales = LocalizationSettings.AvailableLocales.Locales;
            LocalizationSettings.SelectedLocale = locales[settingData.LanguageIndex];
        }
        #endregion

        #region GameInfo
        private async UniTask SettingMinigameData()
        {
            var sid = Manager.User.SID;
            var locale = Util.GetSelectedLocalizedCode();
            //Minigames = await CallAPI.GetMinigameData(locale, sid);
            Minigames = Resources.Load<LocalTestData>("LocalTestData").MinigameData;
        }

        private async UniTask SettingScoreData()
        {
            var scoreData = await CallAPI.GetHighScoreData(Manager.User.SID);
            Scores = scoreData;
        }

        public MinigameEntry GetCurrentMinigameEntry() => Minigames.GetEntryByAddress(CurrentGameAddress);

        public async void UpdateHighScore(int address, int score)
        {
            Scores.SetScoreByAddress(address, score);
            var updateResult = await CallAPI.UpdateAndInsertUserScore(Manager.User.SID, address, score);
            Debug.Log($"HighScore Update Result : {updateResult}");
        }
        #endregion

        public MinigameBase GetCurrentMinigameData()
        {
            var minigameScene = Manager.UI.SceneUI as MinigameSceneUI;
            return minigameScene?.Data;
        }

        private async void OnLocaleChanged(Locale newLocale)
        {
            await SettingMinigameData();
            OnChangedLocale?.Invoke();
        }        
    }

    #region SerializableData
    [Serializable]
    public class SettingData
    {
        public float BGMVolume = 1f;
        public float SFXVolume = 1f;
        public int LanguageIndex = 0;
        /// <summary>
        /// 0 : 로그인 필요, 1 : 이메일 로그인, 2 : 구글 로그인, 3 : 애플 로그인
        /// </summary>
        public int AccountState = 0;
    }    
    #endregion
}
