using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Zeus
{
    public class SceneLoadManager : UnitySingleton<SceneLoadManager>
    {
        static string _nextScene = null;

        public static SplashDirectionData SplashData;
        public static float FadeInDuration;
        public static string NextScene { get { return _nextScene; } }
        public static bool IsLoading { get; private set; }
        internal static bool AutoFade { get; private set; }


        private void Start()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        public bool LoadScene(string sceneName, SplashDirectionData splashData = null, float fadeOutDuration = 1f, float fadeInDuration = 1f, bool autoFade = true)
        {
            if (IsLoading || sceneName.Equals("LoadingScene"))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Now Loading sceneName name : {sceneName}");
#endif
                return false;
            }

            //InputReader.Instance.EnableMapUI(false, true);
            InputReader.Instance.EnableActionMap(TypeInputActionMap.UI);
            InputReader.Instance.Enable = false;

            IsLoading = true;
            CheckEventSystem();
            _nextScene = sceneName;
            SplashData = splashData;
            FadeInDuration = fadeInDuration;
            AutoFade = autoFade;

            FadeManager.Instance.DoFade(true, fadeOutDuration, 0f, () =>
            {
                SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive);
            });

            return true;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive)
            {
                if (scene.name.Contains("LoadingScene"))
                {
                    FadeManager.Instance.DoFade(false, FadeInDuration);
                }
            }
        }

        void CheckEventSystem()
        {
            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem != null)
                Destroy(eventSystem.gameObject);
        }

        internal static int GetSceneNameID(string name)
        {
            var stringID = 0;
            switch (name)
            {
                case "3.0_Scene":
                    {
                        stringID = 9001;
                    }
                    break;
                default:
                    break;
            }

            return stringID;
        }

        internal void LoadingSceneLoadComplete()
        {
            FadeManager.Instance.DoFade(true, 1f, 0f, () =>
            {
                SceneManager.UnloadSceneAsync("LoadingScene");
                if (AutoFade)
                {
                    FadeManager.Instance.DoFade(false, FadeInDuration, 0f, () =>
                    {
                        IsLoading = false;
                        Debug.Log("LoadingSceneLoadComplete 1");
                    });
                }
                else
                {
                    IsLoading = false;
                    Debug.Log("LoadingSceneLoadComplete 2");
                }
            });
        }
    }
}
