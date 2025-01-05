using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using Microphone = FrostweepGames.MicrophonePro.Microphone;

namespace starinc.io.gallaryx
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField]
        private int _Seq = -1;
        public int Seq
        {
            get { return _Seq; }
            set
            {
                _Seq = value;

            }
        }

        [SerializeField]
        private string _localeCode = "ko";
        public string LocaleCode
        {
            get { return _localeCode; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                _localeCode = value;
                Locale targetLocale = null;

                foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
                {
                    if (locale.Identifier.Code == _localeCode)
                    {
                        targetLocale = locale;
                        break;
                    }
                }

                if (targetLocale != null)
                {
                    // 로케일을 변경
                    LocalizationSettings.SelectedLocale = targetLocale;
                    Debug.Log($"Locale changed to: {_localeCode}");
                }
                else
                {
                    Debug.LogError($"Locale '{_localeCode}' not found.");
                }
            }
        }

        [SerializeField]
        private string _uid = "guest";
        public string UID
        {
            get { return _uid; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                _uid = value;
            }
        }

        [SerializeField]
        private string _exhibitionUID = "guest";
        public string ExhibitionUID
        {
            get { return _exhibitionUID; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                _exhibitionUID = value;
            }
        }

        [SerializeField]
        private int _exhibitionSeq = 1;
        public int ExhibitionSeq
        {
            get { return _exhibitionSeq; }
            set
            {
                if (value <= 0) return;
                _exhibitionSeq = value;
            }
        }

        public bool IsServiceMod
        {
            get
            {
                if (_Seq < 0) return false;
                else return true;
            }
        }
        public bool IsLoading { get; set; }
        public string NextScene { get; set; }

        public Action RequireObjectsSpawnCallback;
        public Action<float> ChangeVolumeCallback;
        public Action<string> ChangeLocaleCallback;
        public Action<bool> FullscreenCallback;
        public Action LowMemoryCallback;

        [DllImport("__Internal")]
        public static extern int CheckMobilePlatform();

        protected override void OnAwake()
        {            
            Application.lowMemory += OnLowMemory;
#if UNITY_WEBGL && !UNITY_EDITOR
            //Util.IsMobileWebPlatform = true;
            Util.IsMobileWebPlatform = CheckMobilePlatform() == 1;
            Debug.Log($"IsMobileWebPlatform : {Util.IsMobileWebPlatform}");
#else
            Microphone.RequestPermission();
            Util.IsMobileWebPlatform = false;
#endif
            Application.targetFrameRate = Util.IsMobileWebPlatform ? 30 : 60;
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
            var eventSystem = FindAnyObjectByType<EventSystem>();
            if (eventSystem != null)
                Destroy(eventSystem.gameObject);
            SceneManager.LoadSceneAsync(Define.LoadingSceneName, LoadSceneMode.Additive);
            return true;
        }

        public void ChangeVolume(float volume) => ChangeVolumeCallback?.Invoke(volume);

        private void OnLowMemory()
        {
            Debug.LogWarning($"!!!!Low Memory Warning!!!! Total Allocated Memory : {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()}");
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            LowMemoryCallback?.Invoke();            
        }
    }
}
