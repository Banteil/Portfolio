using System.Collections.Generic;
using Gpm.WebView;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace starinc.io.kingnslave
{
    public class CallWebView
    {
        /// <summary>
        /// 웹뷰 풀 스크린으로 오픈하는 함수
        /// </summary>
        /// <param name="url"></param>
        /// <param name="title"></param>
        public static void ShowUrlFullScreen(string url, string titleKey, bool isLocale = true)
        {
            if (isLocale)
                url += $"?locale={GetCurrentLocaleParameter()}";

            GpmWebView.ShowUrl(
                url,
                new GpmWebViewRequest.Configuration()
                {
                    style = GpmWebViewStyle.FULLSCREEN,
                    orientation = GpmOrientation.UNSPECIFIED,
                    isClearCookie = true,
                    isClearCache = true,
                    backgroundColor = "#FFFFFF",
                    isNavigationBarVisible = true,
                    navigationBarColor = "#4B96E6",
                    title = Util.GetLocalizationTableString(Define.CommonLocalizationTable, titleKey),
                    isBackButtonVisible = true,
                    isForwardButtonVisible = true,
                    isCloseButtonVisible = true,
                    supportMultipleWindows = true,
#if UNITY_IOS
                    contentMode = GpmWebViewContentMode.MOBILE
#endif
                },
            // See the end of the code example
                OnWebViewCallback,
                new List<string>()
                {
            "USER_ CUSTOM_SCHEME"
                });
        }

        public static void ShowHtmlFile(string fileName, string titleKey)
        {
            var htmlFilePath = string.Empty;
#if UNITY_IOS && !UNITY_EDITOR
            htmlFilePath = string.Format("file://{0}/html/{1}.html", Application.streamingAssetsPath, fileName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            htmlFilePath = string.Format("file:///android_asset/{0}.html", fileName);
#endif

            GpmWebView.ShowHtmlFile(
                htmlFilePath,
                new GpmWebViewRequest.Configuration()
                {
                    style = GpmWebViewStyle.FULLSCREEN,
                    orientation = GpmOrientation.UNSPECIFIED,
                    isClearCookie = true,
                    isClearCache = true,
                    backgroundColor = "#FFFFFF",
                    isNavigationBarVisible = true,
                    navigationBarColor = "#4B96E6",
                    title = Util.GetLocalizationTableString(Define.CommonLocalizationTable, titleKey),
                    isBackButtonVisible = true,
                    isForwardButtonVisible = true,
                    isCloseButtonVisible = true,
                    supportMultipleWindows = true,
#if UNITY_IOS
                    contentMode = GpmWebViewContentMode.MOBILE
#endif
                },
                OnWebViewCallback,
                new List<string>()
                {
            "USER_ CUSTOM_SCHEME"
                });
        }

        /// <summary>
        /// 웹뷰 콜백 함수
        /// </summary>
        /// <param name="callbackType"></param>
        /// <param name="data"></param>
        /// <param name="error"></param>
        private static void OnWebViewCallback(
    GpmWebViewCallback.CallbackType callbackType,
    string data,
    GpmWebViewError error)
        {
            Debug.Log("OnCallback: " + callbackType);
            switch (callbackType)
            {
                case GpmWebViewCallback.CallbackType.Open:
                    if (error != null)
                    {
                        Debug.LogFormat("Fail to open WebView. Error:{0}", error);
                    }
                    break;
                case GpmWebViewCallback.CallbackType.Close:
                    if (error != null)
                    {
                        Debug.LogFormat("Fail to close WebView. Error:{0}", error);
                    }
                    UserDataManager.Instance.CheckWithdrawal();
                    break;
                case GpmWebViewCallback.CallbackType.PageStarted:
                    if (string.IsNullOrEmpty(data) == false)
                    {
                        Debug.LogFormat("PageStarted Url : {0}", data);
                    }
                    break;
                case GpmWebViewCallback.CallbackType.PageLoad:
                    if (string.IsNullOrEmpty(data) == false)
                    {
                        Debug.LogFormat("Loaded Page:{0}", data);
                    }
                    break;
                case GpmWebViewCallback.CallbackType.MultiWindowOpen:
                    Debug.Log("MultiWindowOpen");
                    break;
                case GpmWebViewCallback.CallbackType.MultiWindowClose:
                    Debug.Log("MultiWindowClose");
                    break;
                case GpmWebViewCallback.CallbackType.Scheme:
                    if (error == null)
                    {
                        if (data.Equals("USER_ CUSTOM_SCHEME") == true || data.Contains("CUSTOM_SCHEME") == true)
                        {
                            Debug.Log(string.Format("scheme:{0}", data));
                        }
                    }
                    else
                    {
                        Debug.Log(string.Format("Fail to custom scheme. Error:{0}", error));
                    }
                    break;
                case GpmWebViewCallback.CallbackType.GoBack:
                    Debug.Log("GoBack");
                    break;
                case GpmWebViewCallback.CallbackType.GoForward:
                    Debug.Log("GoForward");
                    break;
                case GpmWebViewCallback.CallbackType.ExecuteJavascript:
                    Debug.LogFormat("ExecuteJavascript data : {0}, error : {1}", data, error);
                    break;
#if UNITY_ANDROID
                case GpmWebViewCallback.CallbackType.BackButtonClose:
                    Debug.Log("BackButtonClose");
                    break;
#endif
            }
        }

        private static string GetCurrentLocaleParameter()
        {
            var index = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
            switch ((Define.LocaleName)index)
            {
                case Define.LocaleName.English:
                    return "en-US";
                case Define.LocaleName.Korean:
                    return "ko-KR";
                case Define.LocaleName.ChineseSimplified:
                    return "en-US";
                case Define.LocaleName.ChineseTraditional:
                    return "en-US";
                case Define.LocaleName.Japanese:
                    return "en-US";
                default:
                    return "en-US";
            }
        }
    }
}
