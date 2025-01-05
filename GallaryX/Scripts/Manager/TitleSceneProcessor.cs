using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace starinc.io.gallaryx
{
    public class TitleSceneProcessor : MonoBehaviour
    {
        [DllImport("__Internal")]
        public static extern string GetURLParameters();

        [DllImport("__Internal")]
        private static extern void ManageCache(string version);

        [DllImport("__Internal")]
        private static extern void ReloadPage();

        [DllImport("__Internal")]
        private static extern System.IntPtr GetCookie(string cookieName);

        private async void Awake()
        {
            await LocalizationSettings.InitializationOperation;
            string currentVersion = await CallAPI.GetAPIKey("version");
            Debug.Log(currentVersion);

#if UNITY_WEBGL && !UNITY_EDITOR
            ManageCache(currentVersion);
#else
            GameManager.Instance.LoadScene(Define.MainSceneName);
#endif
        }

        public void OnManageCacheComplete(int result)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            GameManager.Instance.LocaleCode = ReadCookie(Define.Locale);
            GameManager.Instance.UID = ReadCookie(Define.UID);

            bool checkNewVersion = result == 1;
            if (checkNewVersion)
            {
                var reloadPage = transform.GetChild(0).gameObject;
                reloadPage.SetActive(true);
                var processUI = transform.GetChild(1).gameObject;
                processUI.SetActive(false);
            }
            else
            {
                var jsonString = GetURLParameters();
                var urlParameters = Util.JsonToObject<Dictionary<string, string>>(jsonString);
                if (urlParameters != null && urlParameters.Count != 0)
                {
                    if (urlParameters.ContainsKey(Define.ExhibitionSeq))
                        GameManager.Instance.Seq = int.Parse(urlParameters[Define.ExhibitionSeq]);

                    Debug.Log($"Seq : {GameManager.Instance.Seq}");                    
                }
                else
                {
                    Debug.Log("No URL parameters found.");
                }

                GameManager.Instance.LoadScene(Define.MainSceneName);
            }
#endif
        }

        public void ReloadNewVersion()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ReloadPage();
#endif
        }

        private string ReadCookie(string cookieName)
        {
            System.IntPtr cookiePtr = GetCookie(cookieName);
            if (cookiePtr == System.IntPtr.Zero)
            {
                Debug.Log("쿠키를 찾을 수 없습니다: " + cookieName);
                return null;
            }

            string cookieValue = Marshal.PtrToStringUTF8(cookiePtr);
            Debug.Log("쿠키 값: " + cookieValue);
            return cookieValue;
        }
    }
}

