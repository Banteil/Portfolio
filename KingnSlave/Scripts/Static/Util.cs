using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace starinc.io.kingnslave
{
    public class Util
    {
        private static object fileLock = new object();
        private const string hostURL = "https://kingnslave.com";
        /// <summary>
        /// 오브젝트의 자식들 중 컴포넌트를 가지고, name 변수와 이름이 일치하는 오브젝트를 찾아 반환하는 함수.
        /// recursive가 true면 모든 자식들을 싹싹 찾아서 검사.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
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
                foreach (T component in obj.GetComponentsInChildren<T>(true))
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

        /// <summary>
        /// 매개변수 오브젝트를 Parent 오류를 해결하며 DontDestryOnLoad 객체로 변환하는 함수
        /// </summary>
        /// <param name="obj"></param>
        public static void DontDestroyObject(GameObject obj)
        {
            if (obj.transform.parent != null)
                obj.transform.parent = null;
            UnityEngine.Object.DontDestroyOnLoad(obj);
        }

        /// <summary>
        /// table 매개변수의 로컬라이징 테이블에 key 매개변수로 반환값을 받아오는 함수
        /// </summary>
        /// <param name="table"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetLocalizationTableString(string table, string key)
        {
            var localizeString = new LocalizedString() { TableReference = table, TableEntryReference = key };
            var result = localizeString.GetLocalizedString();
            return result;
        }

        /// <summary>
        /// SHA512Hash 암호화
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string SHA512Hash(string plainText)
        {
            SHA512 sha = new SHA512Managed();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(plainText));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// tier, division 매개변수 값으로 테이블을 검색하여 티어의 로컬라이징 이름을 반환하는 함수
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="division"></param>
        /// <returns></returns>
        public static string GetTierName(int tier, int division)
        {
            string[] names = Enum.GetNames(typeof(Define.TierKey));
            var keyName = names[tier];
            var result = GetLocalizationTableString(Define.TierNameLocalizationTable, keyName);
            result = division > 0 ? $"{result} {division}" : result;
            return result;
        }

        /// <summary>
        /// userData내의 promo_result 값을 확인하여 승급전 텍스트로 반환하는 함수
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        public static string GetPromotionText(UserData userData)
        {
            var result = "";
            for (int i = 0; i < 5; i++)
            {
                var name = $"promo_result{i + 1}";
                var field = userData.GetType().GetField(name);
                if (field != null)
                {
                    var value = (string)field.GetValue(userData);
                    if (value == "E")
                        result += "―";
                    else
                        result += value;
                }
                if (i < 4)
                    result += " ";
            }
            return result;
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

        #region Exception Util

        private static bool IsAlphabet(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        async public static void UidExceptionProcessing(string uid, Action<Define.UidExceptionHandling> callback)
        {
            if (uid.Length < 6 || uid.Length > 20)
            {
                callback?.Invoke(Define.UidExceptionHandling.uidLengthIssue);
                return;
            }

            if (!uid.All(c => Util.IsAlphabet(c) || char.IsNumber(c)))
            {
                callback?.Invoke(Define.UidExceptionHandling.uidFormatIssue);
                return;
            }

            await CallAPI.APIValidateUid(UserDataManager.Instance.MySid, uid, (resultCd) =>
            {
                if (resultCd == (int)Define.APIReturnCd.Id_Duplicated)
                {
                    callback?.Invoke(Define.UidExceptionHandling.uidDuplicatedIssue);
                }
                else if (resultCd == (int)Define.APIReturnCd.Wrong)
                {
                    callback?.Invoke(Define.UidExceptionHandling.uidInvalidIssue);
                }
                else
                {
                    callback?.Invoke(Define.UidExceptionHandling.uidNoissues);
                }
            });
        }

        public static void EmailExceptionProcessing(string email, Action<Define.EmailExceptionHandling> callback)
        {
            if (string.IsNullOrEmpty(email))
            {
                callback?.Invoke(Define.EmailExceptionHandling.emailLengthIssue);
                return;
            }

            var isValidFormat = Regex.IsMatch(email,
                          @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                          @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                          RegexOptions.IgnoreCase);
            if(!isValidFormat)
            {
                callback?.Invoke(Define.EmailExceptionHandling.emailFormatIssue);
                return;
            }

            callback(Define.EmailExceptionHandling.emailNoissues);
        }

        public static void VerificationCodeExceptionProcessing(string code, Action<Define.VerificationCodeExceptionHandling> callback)
        {
            if (string.IsNullOrEmpty(code))
            {
                callback?.Invoke(Define.VerificationCodeExceptionHandling.verificationEmptyIssue);
                return;
            }

            callback(Define.VerificationCodeExceptionHandling.verificationCodeNoissues);
        }

        public static void PasswordExceptionProcessing(string password, Action<Define.PasswordExceptionHandling> callback)
        {
            if (password.Length < 8 || password.Length > 20)
            {
                callback?.Invoke(Define.PasswordExceptionHandling.passwordLengthIssue);
                return;
            }

            if(!password.Any(c => char.IsLetter(c)) || !password.Any(c => char.IsNumber(c)))
            {
                callback?.Invoke(Define.PasswordExceptionHandling.passwordFormatIssue);
                return;
            }

            callback?.Invoke(Define.PasswordExceptionHandling.passwordNoissues);
        }

        #endregion

        public static void CopyStringClipboard(string value)
        {
            GUIUtility.systemCopyBuffer = value;
            Debug.Log($"{value} : 클립보드 복사 완료");
        }

        /// <summary>
        /// 리스트의 처음 인덱스를 빼서 마지막으로 넣거나, 마지막 인덱스를 빼서 처음으로 넣는 정렬 함수
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="isFirstToLast"></param>
        /// <returns></returns>
        public static List<T> ReorderList<T>(List<T> list, bool isFirstToLast)
        {
            var reorderList = list;
            if (isFirstToLast)
            {
                var firstElement = reorderList[0];
                reorderList.RemoveAt(0);
                reorderList.Add(firstElement);
            }
            else
            {
                var lastElement = reorderList[reorderList.Count - 1];
                reorderList.RemoveAt(reorderList.Count - 1);
                reorderList.Insert(0, lastElement);
            }
            return reorderList;
        }

        /// <summary>
        /// 설정 데이터를 기기에 저장하는 함수
        /// </summary>
        /// <param name="data"></param>
        public static void SaveSettingData(SettingData data)
        {
            var json = ObjectToJson(data);
            PlayerPrefs.SetString(Define.SettingDataKey, json);
        }

        /// <summary>
        /// 설정 데이터를 불러오는 함수
        /// </summary>
        /// <returns></returns>
        public static SettingData LoadSettingData()
        {
            var json = PlayerPrefs.GetString(Define.SettingDataKey);
            var settingData = JsonToObject<SettingData>(json);
            return settingData;
        }

        /// <summary>
        /// 시스템 언어 기준으로 로케일 테이블 인덱스를 반환하는 함수
        /// </summary>
        /// <param name="data"></param>
        public static int GetLocaleIndexBySystemLanguage(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.English:
                    return 0;
                case SystemLanguage.Turkish:
                    return 1;
                case SystemLanguage.French:
                    return 2;
                case SystemLanguage.German:
                    return 3;
                case SystemLanguage.Italian:
                    return 4;
                case SystemLanguage.Spanish:
                    return 5;
                case SystemLanguage.Portuguese:
                    return 6;
                case SystemLanguage.Russian:
                    return 7;
                case SystemLanguage.Japanese:
                    return 8;
                case SystemLanguage.Korean:
                    return 9;
                case SystemLanguage.Arabic:
                    return 10;
                case SystemLanguage.Dutch:
                    return 11;                    
                case SystemLanguage.Chinese:
                    return 12;
                case SystemLanguage.ChineseSimplified:
                    return 12;
                case SystemLanguage.ChineseTraditional:
                    return 13;
                case SystemLanguage.Polish:
                    return 14;
                case SystemLanguage.Indonesian:
                    return 15;
                case SystemLanguage.Ukrainian:
                    return 16;
                case SystemLanguage.Romanian:
                    return 17;
                case SystemLanguage.Vietnamese:
                    return 18;
                case SystemLanguage.Thai:
                    return 19;
                case SystemLanguage.Danish:
                    return 20;
                case SystemLanguage.Swedish:
                    return 21;
                case SystemLanguage.Greek:
                    return 22;
                case SystemLanguage.Hungarian:
                    return 23;
                case SystemLanguage.Hebrew:
                    return 24;
                case SystemLanguage.Czech:
                    return 25;
                case SystemLanguage.Norwegian:
                    return 29;
                default:
                    return 0;
            }
        }

        public static int GetCountryByLocaleIndex(int localeIndex)
        {
            switch(localeIndex)
            {
                case 0:
                    return 252;
                case 1:
                    return 244;
                case 2:
                    return 83;
                case 3:
                    return 89;
                case 4:
                    return 115;
                case 5:
                    return 223;
                case 6:
                    return 189;
                case 7:
                    return 194;
                case 8:
                    return 119;
                case 9:
                    return 126;
                case 10:
                    return 1;
                case 11:
                    return 166;
                case 12:
                    return 50;
                case 13:
                    return 50;
                case 14:
                    return 188;
                case 15:
                    return 109;
                case 16:
                    return 249;
                case 17:
                    return 193;
                case 18:
                    return 259;
                case 19:
                    return 237;
                case 20:
                    return 64;
                case 21:
                    return 231;
                case 22:
                    return 92;
                case 23:
                    return 106;
                case 24:
                    return 114;
                case 25:
                    return 63;
                case 26:
                    return 143;
                case 27:
                    return 16;
                case 28:
                    return 110;
                case 29:
                    return 177;
                default:
                    return 0;
            }
        }

        public static int GetCountryByTwoLetterISORegionName(string regionName)
        {
            switch (regionName)
            {
                case "US":
                    return 0;
                case "TR":
                    return 244;
                case "FR":
                    return 83;
                case "DE":
                    return 89;
                case "IT":
                    return 115;
                case "ES":
                    return 223;
                case "PT":
                    return 189;
                case "RU":
                    return 194;
                case "JP":
                    return 119;
                case "KR":
                    return 126;
                case "SA":
                    return 206;
                case "NL":
                    return 166;
                case "CN":
                    return 50;
                case "PL":
                    return 188;
                case "ID":
                    return 109;
                case "UA":
                    return 249;
                case "RO":
                    return 193;
                case "VN":
                    return 259;
                case "TH":
                    return 237;
                case "DK":
                    return 64;
                case "SE":
                    return 231;
                case "GR":
                    return 92;
                case "HU":
                    return 106;
                case "IL":
                    return 114;
                case "CZ":
                    return 63;
                case "IR":
                    return 143;
                case "AZ":
                    return 16;
                case "MY":
                    return 110;
                case "NO":
                    return 177;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 시스템 언어 기준으로 로케일 테이블 인덱스를 반환하는 함수
        /// </summary>
        /// <param name="data"></param>
        public static string GetLocalei18n(Locale locale)
        {
            string[] split = locale.LocaleName.Split('_');
            return split[1];
        }

        /// <summary>
        /// 게임 종료 프로세스 함수
        /// </summary>
        /// <param name="sceneName"></param>
        public static void ExitProcess(string sceneName)
        {
            if (sceneName == Define.LobbySceneName)
            {
                var yonUI = UIManager.Instance.ShowYesOrNoUI(GetLocalizationTableString(Define.InfomationLocalizationTable, "gameQuit"), () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
            }
            else
            {
                var yonUI = UIManager.Instance.ShowYesOrNoUI(GetLocalizationTableString(Define.InfomationLocalizationTable, "gameExit"), () =>
                {
                    if (GameManager.Instance.CurrentGameMode != Define.GamePlayMode.Practice)
                    {
                        if (NetworkManager.Instance.HasRunner && !NetworkManager.Instance.MyRunner.IsShutdown)
                        {
                            NetworkManager.Instance.CallGameOverWhenPlayerLeft();
                            GameManager.Instance.ClearGame();
                        }
                    }
                    SceneManager.LoadScene(Define.LobbySceneName);
                });
            }
        }

        public static Texture2D SpriteToTexture(Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                             (int)sprite.textureRect.y,
                                                             (int)sprite.textureRect.width,
                                                             (int)sprite.textureRect.height);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else
                return sprite.texture;
        }

        public static string URLToFilePath(string url)
        {
            var replace = Regex.Replace(url, hostURL, "");
            var result = $"{Application.persistentDataPath}{replace}";
            return result;
        }

        public static void SaveTextureFile(Texture2D texture, string filePath)
        {
            if (filePath.Contains("https://")) return;
            lock (fileLock)
            {
                try
                {
                    if (File.Exists(filePath)) return;
                    byte[] textureBytes = texture.EncodeToPNG();
                    string folderPath = System.IO.Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    File.WriteAllBytes(filePath, textureBytes);
                    Debug.Log($"Save Path : {filePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        async public static UniTask<Texture2D> LoadTextureFile(string filePath)
        {
            try
            {
                using (FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
                {
                    var buffer = new byte[sourceStream.Length];
                    await sourceStream.ReadAsync(buffer, 0, (int)sourceStream.Length);

                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(buffer);
                    texture.Apply();
                    return texture;
                }
            }
            catch (Exception ex)
            {                
                Debug.LogException(ex);
                return null;
            }
        }
    }
}
