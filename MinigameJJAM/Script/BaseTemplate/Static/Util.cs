using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace starinc.io
{
    public static class Util
    {
        public static T FindChild<T>(GameObject obj, string name = null, bool recursive = false) where T : UnityEngine.Object
        {
            if (obj == null) return null;

            if (!recursive)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    Transform transform = obj.transform.GetChild(i);
                    if (string.IsNullOrEmpty(name) || transform.name == name)
                    {
                        T component = transform.GetComponent<T>();
                        if (component != null)
                            return component;
                    }
                }
            }
            else
            {
                foreach (T component in obj.GetComponentsInChildren<T>())
                {
                    if (string.IsNullOrEmpty(name) || component.name == name)
                        return component;
                }
            }

            return null;
        }

        /// <summary>
        /// 컴포넌트 없는 GameObject를 찾아 반환하는 함수.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public static GameObject FindChild(GameObject obj, string name = null, bool recursive = false)
        {
            Transform transform = FindChild<Transform>(obj, name, recursive);
            if (transform == null)
                return null;

            return transform.gameObject;
        }

        /// <summary>
        /// 오브젝트의 컴포넌트를 가져오는데, 없으면 Add해서 가져오는 함수.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(GameObject obj) where T : UnityEngine.Component
        {
            T component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        public static Transform FindOrAddChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.parent = parent;
            }

            return child;
        }

        /// <summary>
        /// 부모 객체들을 탐색하여 원하는 타입의 Component 반환하는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentTransform"></param>
        /// <returns></returns>
        public static T FindComponentInParents<T>(Transform currentTransform) where T : Component
        {
            while (currentTransform != null)
            {
                T component = currentTransform.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
                currentTransform = currentTransform.parent;
            }
            return null;
        }

        /// <summary>
        /// object에 담긴 json 정보를 특정 타입으로 캐스팅하여 반환하는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T CastingJsonObject<T>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 특정 오브젝트를 Json 데이터로 변환(직렬화)하는 함수
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Json 데이터를 클래스로 변환(역직렬화)하는 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(string jsonData)
        {
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        public static void DontDestroyObject(GameObject obj)
        {
            if (obj.transform.parent != null)
                obj.transform.SetParent(null, false);
            UnityEngine.Object.DontDestroyOnLoad(obj);
        }

        public static int GetSceneIndexByName(string name)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (scenePath.Contains(name)) return i;
            }
            return -1;
        }

        public static void ExcludingCloneName(GameObject obj)
        {
            int index = obj.name.IndexOf("(Clone)");
            if (index > 0)
                obj.name = obj.name.Substring(0, index);
        }

        public static string RemoveCloneSuffix(string input)
        {
            const string cloneSuffix = "(Clone)";
            if (input.EndsWith(cloneSuffix))
            {
                return input.Substring(0, input.Length - cloneSuffix.Length).Trim();
            }
            return input;
        }

        public static bool IsPointerOverUI(Vector2 pos, bool onlyUILayer = false)
        {
            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = pos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (onlyUILayer)
            {
                foreach (var result in results)
                {
                    if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
                        return true;
                }
                return false;
            }
            else
                return results.Count > 0;
        }

        public static bool IsUIFocusing
        {
            get
            {
                if (EventSystem.current == null) return false;
                return EventSystem.current.currentSelectedGameObject != null;
            }
        }

        public static void UnfocusUI()
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        public static bool IsUIObject(GameObject obj)
        {
            return obj.GetComponent<RectTransform>() != null;
        }

        public static int GetSelectedLocaleIndex()
        {
            var availableLocales = LocalizationSettings.AvailableLocales.Locales.ToList();
            int index = availableLocales.IndexOf(LocalizationSettings.SelectedLocale);
            return index;
        }

        public static string GetSelectedLocalizedCode()
        {
            var locale = LocalizationSettings.SelectedLocale;
            return locale != null ? locale.Identifier.Code : "en";
        }

        public static string GetLocalizedString(string tableName, string key)
        {
            var stringTable = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (stringTable != null)
            {
                var entry = stringTable.GetEntry(key);
                if (entry != null)
                {
                    return entry.GetLocalizedString();
                }
            }
            return null;
        }

        public static void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public static void UnPause()
        {
            var soundManager = Manager.Sound;
            soundManager.PlayBGM();
            soundManager.ReplayAllSFX();
            Time.timeScale = 1.0f;
        }

        public static bool CheckNetworkReachability()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                var message = GetLocalizedString(Define.LOCALIZATION_TABLE_MESSAGE, "networkNotReachable");
                Manager.UI.ShowMessage(message);
                return false;
            }
            return true;
        }

        public static bool IsPointerWithinScreenBounds(Vector2 position)
        {
            return position.x >= 0 && position.x <= Screen.width && position.y >= 0 && position.y <= Screen.height;
        }

        public static byte[] GetRawData<T>(T data) where T : class
        {
            var jsonData = ObjectToJson(data);
            Debug.Log($"GetRawData json : {jsonData}");
            return Encoding.UTF8.GetBytes(jsonData);
        }

        public static Define.Platform GetCurrentPlatform()
        {
            Define.Platform platform;
            if (Application.platform == RuntimePlatform.Android)
            {
                platform = Define.Platform.Google;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                platform = Define.Platform.Apple;
            }
            else
            {
                platform = Define.Platform.Unknown; // 필요시, 기본값 설정
            }
            return platform;
        }

        public static bool IsInsideViewportPosition(Vector3 position)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null) return false; 
            var viewportPosition = mainCamera.WorldToViewportPoint(position);

            var isInside = viewportPosition.x > 0f && viewportPosition.x < 1f &&
                viewportPosition.y > 0f && viewportPosition.y < 1f;

            return isInside;
        }

        public static bool IsOutsideViewportPosition(Vector3 position)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null) return false;
            var viewportPosition = mainCamera.WorldToViewportPoint(position);

            var isOutside = viewportPosition.x < 0f || viewportPosition.x > 1f ||
            viewportPosition.y < 0f || viewportPosition.y > 1f;

            return isOutside;
        }
    }
}
