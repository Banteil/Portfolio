using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace starinc.io
{
    public static class CallAPI
    {
        #region Cache
        private static string _mainAPIURL = "https://jjamgame.com/api";
        private const float _timeout = 10f;
        #endregion

        /// <summary>
        /// 키 밸류 조회 함수
        /// </summary>
        /// <param name="cdKey"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public static async UniTask<string> GetAPIKey(string cdKey, string sid = "system")
        {
            try
            {
                var getURL = "/common/selectKeyValue";
                var webRequest = await GetRequest(getURL, $"sid={sid}", $"cdKey={cdKey}");
                var response = Util.JsonToObject<Response>(webRequest.downloadHandler.text);
                var keyValue = (string)response.returnValue;
                return keyValue;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// 미니게임 정보 반환 함수
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async UniTask<MinigameData> GetMinigameData(string locale, string sid = "system")
        {
            try
            {
                var getURL = "/game/selectGameList";
                var webRequest = await GetRequest(getURL, $"sid={sid}", $"locale={locale}", $"isTest={Manager.IsTestBuild}");
                var response = Util.JsonToObject<Response>(webRequest.downloadHandler.text);
                var json = Convert.ToString(response.returnValue);
                var data = Util.JsonToObject<MinigameData>(json);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public static async UniTask<HighScoreData> GetHighScoreData(string sid)
        {
            try
            {
                var getURL = "/userscore/selectHighscoreList";
                var webRequest = await GetRequest(getURL, $"sid={sid}");
                var response = Util.JsonToObject<Response>(webRequest.downloadHandler.text);
                var json = Convert.ToString(response.returnValue);
                var data = Util.JsonToObject<HighScoreData>(json);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public static async UniTask<int> GetHighScore(string sid, int address)
        {
            try
            {
                var getURL = "/userscore/selectHighscore";
                var webRequest = await GetRequest(getURL, $"sid={sid}", $"gameSeq={address}");
                var response = Util.JsonToObject<Response>(webRequest.downloadHandler.text);
                var json = Convert.ToString(response.returnValue);
                var data = Util.JsonToObject<HighScoreEntry>(json);
                return data.highScore;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return 0;
            }
        }

        public static async UniTask<bool> UpdateAndInsertUserScore(string sid, int address, int score)
        {
            try
            {
                var postUrl = "/userscore/updateAndInsertUser";
                var requestData = new UpdateHighScoreData(sid, address, score);
                var rawData = Util.GetRawData(requestData);
                var webRequest = await PostRequest(postUrl, rawData);
                var response = Util.JsonToObject<Response>(webRequest.downloadHandler.text);
                return response.returnCd == 0;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        public static async UniTask<string> InsertUser()
        {
            try
            {
                var postUrl = "/user/insertUser";
                var webRequest = await PostRequest(postUrl);
                var response = Util.JsonToObject<Response>(webRequest.downloadHandler.text);
                var sid = (string)response.returnValue;
                return sid;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public static async void InsertPaymentInfo(string sid, string productId, string receipt, Define.Platform platform)
        {
            try
            {
                var postUrl = "/payment/insertPayment";
                var encReceipt = await Manager.Enc.Encrypt(receipt);
                var requestData = new InsertPaymentData(sid, productId, encReceipt, platform);
                var rawData = Util.GetRawData(requestData);
                var webRequest = await PostRequest(postUrl, rawData);
                var response = Util.JsonToObject<Response>(webRequest.downloadHandler.text);
                var result = response.returnCd == 0;
                Debug.Log($"InsertPaymentInfo Is Success : {result}");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        #region Request
        public static async UniTask<UnityWebRequest> GetRequest(string getURL, params string[] param)
        {
            if (param != null || param.Length > 0)
            {
                for (int i = 0; i < param.Length; i++)
                {
                    getURL += ((i == 0 ? "?" : "&") + $"{param[i]}");
                }
            }

            UnityWebRequest webRequest = UnityWebRequest.Get($"{_mainAPIURL}{getURL}");
            try
            {                
                var operation = webRequest.SendWebRequest();
                var startTime = Time.time;
                while (!operation.isDone)
                {
                    if (Time.time - startTime > _timeout)
                    {
                        Debug.LogError($"Request Timeout : {getURL}");
                        webRequest.Abort();
                        return null;
                    }
                    await UniTask.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{webRequest.result}, {webRequest.error}");
                    return null;
                }
                else
                {
                    return webRequest;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                webRequest.Abort();
                return null;
            }
        }

        public static async UniTask<UnityWebRequest> PostRequest(string postUrl, WWWForm form = null)
        {
            UnityWebRequest webRequest = UnityWebRequest.Post($"{_mainAPIURL}{postUrl}", form);
            try
            {                
                var operation = webRequest.SendWebRequest();
                var startTime = Time.time;
                while (!operation.isDone)
                {
                    if (Time.time - startTime > _timeout)
                    {
                        Debug.LogError($"Request Timeout : {postUrl}");
                        webRequest.Abort();
                        return null;
                    }
                    await UniTask.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{webRequest.result}, {webRequest.error}");
                    return null;
                }
                else
                {
                    return webRequest;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                webRequest.Abort();
                return null;
            }
        }

        public static async UniTask<UnityWebRequest> PostRequest(string postUrl, byte[] rawData)
        {            
            UnityWebRequest webRequest = new UnityWebRequest($"{_mainAPIURL}{postUrl}", "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(rawData)
            {
                contentType = "application/json" // 서버에 전송하는 데이터 형식을 설정합니다. 필요에 따라 변경하십시오.
            };
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            try
            {
                var operation = webRequest.SendWebRequest();
                var startTime = Time.time;
                while (!operation.isDone)
                {
                    if (Time.time - startTime > _timeout)
                    {
                        Debug.LogError($"Request Timeout : {postUrl}");
                        webRequest.Abort();
                        return null;
                    }
                    await UniTask.Yield();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{webRequest.result}, {webRequest.error}");
                    return null;
                }
                else
                {
                    return webRequest;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                webRequest.Abort();
                return null;
            }
        }

        public static async UniTask<Sprite> GetSprite(string url)
        {
            // 이미지 파일명 추출
            string imageName = System.IO.Path.GetFileName(url);
            string localPath = System.IO.Path.Combine(Application.persistentDataPath, imageName);

            // 로컬에 이미지가 있는지 체크
            if (System.IO.File.Exists(localPath))
            {
                // 로컬 이미지가 존재하면, 해당 이미지를 로드
                byte[] imageBytes = System.IO.File.ReadAllBytes(localPath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                texture.name = imageName;

                // 서버의 마지막 수정 시간을 확인하기 위해 HEAD 요청을 보냄
                UnityWebRequest headRequest = UnityWebRequest.Head(url);
                await headRequest.SendWebRequest();
                if (headRequest.result == UnityWebRequest.Result.Success)
                {
                    string lastModified = headRequest.GetResponseHeader("Last-Modified");
                    string localLastModified = System.IO.File.GetLastWriteTime(localPath).ToString("R");

                    if (lastModified != null && lastModified != localLastModified)
                    {
                        Debug.Log("Image updated on server, downloading new image...");
                        return await DownloadAndSaveImage(url, localPath);
                    }
                    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    Debug.LogError($"Failed to check image update: {headRequest.error}");
                }
            }

            // 로컬 이미지가 없거나 업데이트가 필요할 경우, 웹에서 이미지 다운로드
            return await DownloadAndSaveImage(url, localPath);
        }

        private static async UniTask<Sprite> DownloadAndSaveImage(string url, string localPath)
        {
            try
            {
                // 웹에서 이미지 다운로드
                UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to download image: {webRequest.error}");
                    return null; // 오류 처리
                }

                // 웹에서 다운로드한 텍스처
                Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(webRequest);
                downloadedTexture.name = System.IO.Path.GetFileName(url);

                // 텍스처를 로컬에 저장
                byte[] bytes = downloadedTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(localPath, bytes);
                return Sprite.Create(downloadedTexture, new Rect(0, 0, downloadedTexture.width, downloadedTexture.height), new Vector2(0.5f, 0.5f));
            }
            catch (Exception ex)
            {
                Debug.LogError($"UnityWebRequest Exception: {ex.Message}");
                return null;
            }
        }
    }
    #endregion

    #region RequestData
    [Serializable]
    public class UpdateHighScoreData
    {
        public string sid;
        public int game_seq;
        public int high_score;

        public UpdateHighScoreData(string checkSid, int address, int score)
        {
            sid = checkSid;
            game_seq = address;
            high_score = score;
        }
    }

    [Serializable]
    public class InsertPaymentData
    {
        public string sid;
        public string product_id;
        public string receipt;
        public int platform;

        public InsertPaymentData(string sid, string productId, string receipt, Define.Platform platform)
        {
            this.sid = sid;
            this.product_id = productId;
            this.receipt = receipt;
            this.platform = (int)platform;
        }
    }
    #endregion

    #region ResponseData

    [Serializable]
    public class Response
    {
        public string resultCd;
        public string resultMsg;
        public int returnCd;
        public object returnValue;
    }

    [Serializable]
    public class MinigameData
    {
        public List<MinigameEntry> minigameEntries = new List<MinigameEntry>();

        public int GetEntriesCount() => minigameEntries.Count;

        public MinigameEntry GetEntryByIndex(int index)
        {
            var entry = minigameEntries[index];
            return entry;
        }

        public MinigameEntry GetEntryByAddress(int gameAddress)
        {
            var entry = minigameEntries.FirstOrDefault(e => e.address == gameAddress);
            return entry;
        }
    }


    [Serializable]
    public class MinigameEntry
    {
        public string name;
        public int address;
        public string iconUrl;
        public string thumbnailUrl;
        public string controlInfo;
    }

    [Serializable]
    public class HighScoreData
    {
        public List<HighScoreEntry> scoreEntries = new List<HighScoreEntry>();
        public int GetScoreByAddress(int gameAddress)
        {
            var entry = scoreEntries.FirstOrDefault(e => e.address == gameAddress);
            return entry != null ? entry.highScore : 0;
        }

        public void SetScoreByAddress(int gameAddress, int score)
        {
            var entry = scoreEntries.FirstOrDefault(e => e.address == gameAddress);
            if (entry != null)
                entry.highScore = score;
            else
                scoreEntries.Add(new HighScoreEntry { address = gameAddress, highScore = score });
        }
    }

    [Serializable]
    public class HighScoreEntry
    {
        public int address;
        public int highScore;
    }
    #endregion
}

