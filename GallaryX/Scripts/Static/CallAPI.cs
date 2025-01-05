using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace starinc.io.gallaryx
{
    public static class CallAPI
    {
        private static string _mainAPIURL = "https://galleryx.io/api";
        private const float _timeout = 10f;

        public static async UniTask<List<GalleryXMedia>> GetMediaData(int exhibitionSeq, string uid = "system")
        {
            try
            {
                Debug.Log("GetMediaData");
                var getURL = "/media/selectMediaListByProjectSeq";
                var webRequest = await GetRequest(getURL, $"uid={uid}", $"projectSeq={exhibitionSeq}");
                var dataList = Util.JsonToObject<List<GalleryXMedia>>(webRequest.downloadHandler.text);
                var exhibitionSeqDataList = dataList.OrderBy(data => data.order_no).ToList();
                return exhibitionSeqDataList;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public static async void SetMediaData(List<GalleryXMedia> seqDataList)
        {
            try
            {
                var jsonData = Util.ObjectToJson(seqDataList);
                var rawData = Encoding.UTF8.GetBytes(jsonData);
                var postURL = "/media/updateMediaOrderNoList";
                var webRequest = await PostRequest(postURL, rawData);
                var response = Util.JsonToObject<Response>(webRequest.downloadHandler.text);
                Debug.Log(response.resultCd);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static async UniTask<GalleryXExhibitionData> GetExhibitionData(int exhibitionSeq)
        {
            try
            {
                Debug.Log("GetExhibitionData");
                var getURL = "/project/selectProjectAndExhibitionHall";
                var webRequest = await GetRequest(getURL, $"seq={exhibitionSeq}");
                var data = Util.JsonToObject<GalleryXExhibitionData>(webRequest.downloadHandler.text);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public static async UniTask<string> GetAPIKey(string cdKey, string uid = "system")
        {
            try
            {
                var getURL = "/common/selectKeyValue";
                var webRequest = await GetRequest(getURL, $"uid={uid}", $"cdKey={cdKey}");
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

            Debug.Log($"CallAPI : {_mainAPIURL}{getURL}");
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

        public static async UniTask<Texture2D> GetTextureData(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            try
            {
                UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
                await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    var downloadTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                    //텍스쳐 압축은 동기적이라 렉이 걸리니 유니티 내부에서 처리하는건 보류, 근데 메모리 너무 터지니 다시 테스트
                    var compressTexture = Util.CompressTexture(downloadTexture);
                    compressTexture.name = url;

                    Texture2D.DestroyImmediate(downloadTexture);

                    return compressTexture;
                }
                else
                {
                    Debug.Log($"{webRequest.result}, {webRequest.error}");
                    return null;
                }
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }
    }

    public class Response
    {
        public string resultCd;
        public string resultMsg;
        public int returnCd;
        public object returnValue;
    }
    #endregion

    #region Serializable Class
    [Serializable]
    public class GalleryXMedia
    {
        public int seq;
        public int project_seq;
        public string nft_id;
        public int order_no;
        public string sale_yn;
        public string price;
        public string del_yn;
        public string uid;
        public string type;
        public string url;
        public string thumbnail_url;
        public string title;
        public string subtitle;
        public string description;
        public string created_at;
        public string size;
        public string materials;
        public int last_nft_id;
        public string nft_market_url;
    }

    [Serializable]
    public class GalleryXExhibitionData
    {
        public int seq;
        public string title;
        public string subtitle;
        public int exhibition_seq;
        public string exhibition_name;
        public string exhibition_type;
        public int display_maximum;
        public string exhibition_description;
        public string exhibition_url;
        public string nickname;
        public string artist_details;
        public string email;
        public string instargram;
        public string twitter;
        public string discord;
        public string telegram;
        public string postar_url;
        public string description;
        public string contract_address;
        public string collection_url;
        public string media_url_prefix;
        public string uid;
    }

    [Serializable]
    public class ExhibitionAndMediaData
    {
        [JsonProperty("mediaList")]
        public List<GalleryXMedia> _mediaList;
        [JsonProperty("project")]
        public GalleryXExhibitionData _exhibitionData;
    }
    #endregion
}

