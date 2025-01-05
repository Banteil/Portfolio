using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Networking;

namespace starinc.io.gallaryx
{
    public static class ChatAIAPI
    {
        private const string _typecastKey = "__pltCChnAQApFXyudNy37jddpuFBSZZxDVb9NyMSDMa8";
        private const string _authorizationHeader = "Bearer";
        public delegate void StringEvent(string _string);
        public static StringEvent CompletedRepostEvent;

        private async static UniTask<string> ClieantResponse<SendData>(SendData request)
        {
            var apiUrl = ((URL)request).Get_API_Url();
            string jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            try
            {
                using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
                {
                    byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonContent);
                    webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();

                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    webRequest.SetRequestHeader("Authorization", $"{_authorizationHeader} {AIManager.Instance.OpenAIKey}");
                    webRequest.SetRequestHeader("User-Agent", "okgodoit/dotnet_openai_api");
                    await webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        return webRequest.downloadHandler.text;
                    }
                    else
                    {
                        Debug.LogError("Error calling OpenAi API to get completion. HTTP status code: " + webRequest.responseCode.ToString() + ". Request body: " + jsonContent);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        #region TTS Request

        public async static UniTask<AudioClip> ClieantResponseTTS(GoogleTTSRequest textToSpeechRequest)
        {
            AudioClip clip = null;
            try
            {
                var apiUrl = $"{((URL)textToSpeechRequest).Get_API_Url()}{AIManager.Instance.GoogleTTSKey}";
                var jsonContent = JsonConvert.SerializeObject(textToSpeechRequest, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                var bodyRaw = Encoding.UTF8.GetBytes(jsonContent);
                var jsonRespons = await GoogleResponseProgress(bodyRaw, apiUrl);

                GoogleContent info = JsonConvert.DeserializeObject<GoogleContent>(jsonRespons);
                var audioBytes = Convert.FromBase64String(info.AudioContent);
                clip = AudioClip.Create("audioContent", audioBytes.Length, 1, 24000, false);

                var floatBytes = Util.ConvertByteToFloat(audioBytes);
                clip.SetData(floatBytes, 0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return clip;
        }

        private async static UniTask<string> GoogleResponseProgress(byte[] bodyRaw, string url)
        {
            try
            {
                using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
                {
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");

                    await webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        return webRequest.downloadHandler.text;
                    }
                    else
                    {
                        Debug.LogError($"Error in GoogleResponseProgress: {webRequest.error}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public async static UniTask<AudioClip> ClieantResponseTTS(TypecastTTSRequest textToSpeechRequest)
        {
            AudioClip clip = null;
            try
            {
                var apiUrl = $"{((URL)textToSpeechRequest).Get_API_Url()}";
                var jsonData = Util.ObjectToJson(textToSpeechRequest.Data);
                using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
                {
                    byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonData);
                    webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                    webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    webRequest.SetRequestHeader("Authorization", "Bearer " + _typecastKey);

                    // 요청 보내기
                    await webRequest.SendWebRequest();
                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        var count = 0;
                        var urlResponce = Util.JsonToObject<TypecastTTSResponce>(webRequest.downloadHandler.text);
                        var pollURL = urlResponce.Result.speak_v2_url;
                        string downloadURL = null;
                        do
                        {
                            await UniTask.WaitForSeconds(1);
                            using (UnityWebRequest polling = new UnityWebRequest(pollURL, "GET"))
                            {
                                polling.SetRequestHeader("Authorization", "Bearer " + _typecastKey);
                                polling.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                                await polling.SendWebRequest();

                                if (polling.result == UnityWebRequest.Result.Success)
                                {
                                    var pollingResponce = Util.JsonToObject<TypecastTTSResponce>(polling.downloadHandler.text);
                                    downloadURL = pollingResponce.Result.audio_download_url;
                                    count++;
                                }
                                else
                                {
                                    Debug.LogError("Error calling OpenAi API to get completion. HTTP status code: " + polling.responseCode.ToString());
                                    break;
                                }
                            }

                            if (count >= 10) break;
                        } while (downloadURL == null);

                        if (downloadURL != null)
                        {
                            using (UnityWebRequest downloadRequest = new UnityWebRequest(downloadURL, "GET"))
                            {
                                downloadRequest.SetRequestHeader("Content-Type", "application/json");
                                downloadRequest.SetRequestHeader("Authorization", "Bearer " + _typecastKey);
                                downloadRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                                await downloadRequest.SendWebRequest();

                                if (downloadRequest.result == UnityWebRequest.Result.Success)
                                {
                                    clip = AudioClip.Create("audioContent", downloadRequest.downloadHandler.data.Length, 1, 44100, false);
                                    var floatBytes = Util.ConvertByteToFloat(downloadRequest.downloadHandler.data);
                                    clip.SetData(floatBytes, 0);
                                }
                                else
                                {
                                    Debug.LogError("Error calling OpenAi API to get completion. HTTP status code: " + downloadRequest.responseCode.ToString());
                                }

                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Error calling OpenAi API to get completion. HTTP status code: " + webRequest.responseCode.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return clip;
        }
        #endregion

        #region STT Request

        public async static UniTask<string> CreateAudioTranslation(OpenAISTTRequest request)
        {
            try
            {
                var apiUrl = ((URL)request).Get_API_Url();
                WWWForm form = new WWWForm();
                form.AddBinaryData("file", request.FileData.Data, request.FileData.Name, "audio/wav");
                form.AddField("model", request.Model);
                form.AddField("language", request.Language);
                form.AddField("response_format", request.ResponseFormat);

                using (UnityWebRequest webRequest = UnityWebRequest.Post(apiUrl, form))
                {
                    webRequest.SetRequestHeader("Authorization", $"{_authorizationHeader} {AIManager.Instance.OpenAIKey}");
                    await webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        return webRequest.downloadHandler.text;
                    }
                    else
                    {
                        Debug.LogError("Error calling OpenAi API to get completion. HTTP status code: " + webRequest.responseCode.ToString());
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public async static UniTask<string> CreateAudioTranslation(GoogleSTTRequest request)
        {
            try
            {
                var apiUrl = $"{((URL)request).Get_API_Url()}{AIManager.Instance.GoogleSTTKey}";
                var requestJson = Util.ObjectToJson(request);

                using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestJson);
                    webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                    webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                    await webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        var responseJson = webRequest.downloadHandler.text;
                        Debug.Log(responseJson);
                        var responseData = Util.JsonToObject<GoogleSTTResponse>(responseJson);
                        if (responseData.Results == null) return null;
                        var result = responseData.Results[0].Alternatives[0].Transcript;
                        return result;
                    }
                    else
                    {
                        Debug.LogError("Error calling OpenAi API to get completion. HTTP status code: " + webRequest.responseCode.ToString());
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        [Serializable]
        public class GoogleSTTResponse
        {
            [JsonProperty("results")]
            public GoogleSTTResult[] Results;

            [JsonProperty("totalBilledTime")]
            public string TotalBilledTime;

            [JsonProperty("speechAdaptationInfo")]
            public SpeechAdaptationInfo SpeechAdaptationInfo;

            [JsonProperty("requestId")]
            public string RequestId;
        }

        [Serializable]
        public class GoogleSTTResult
        {
            [JsonProperty("alternatives")]
            public Alternative[] Alternatives;

            [JsonProperty("channelTag")]
            public int ChannelTag;

            [JsonProperty("resultEndTime")]
            public string ResultEndTime;

            [JsonProperty("languageCode")]
            public string LanguageCode;
        }

        [Serializable]
        public class Alternative
        {
            [JsonProperty("transcript")]
            public string Transcript;

            [JsonProperty("confidence")]
            public float Confidence;

            [JsonProperty("words")]
            public WordInfo[] Words;
        }

        [Serializable]
        public class WordInfo
        {
            [JsonProperty("startTime")]
            public string StartTime;

            [JsonProperty("endTime")]
            public string EndTime;

            [JsonProperty("word")]
            public string Word;

            [JsonProperty("confidence")]
            public float Confidence;

            [JsonProperty("speakerTag")]
            public int SpeakerTag;
        }

        [Serializable]
        public class SpeechAdaptationInfo
        {
            [JsonProperty("adaptationTimeout")]
            public bool AdaptationTimeout;

            [JsonProperty("timeoutMessage")]
            public string TimeoutMessage;
        }
        #endregion

        public async static UniTask<ChatResponse> ClieantResponseChat(ChatRequest r)
        {
            var response = await ClieantResponse(r);
            return response != null ? JsonConvert.DeserializeObject<ChatResponse>(response) : null;
        }

        public async static UniTask<ChatResponse> ClieantResponseChatAnalyze(ChatMessageAnalyze r)
        {
            return JsonConvert.DeserializeObject<ChatResponse>(await ClieantResponse(r));
        }

        interface URL
        {
            public string Get_API_Url();
        }

        #region Chat
        [Serializable]
        public class ChatRequest : URL
        {
            public const string API_Url = "https://api.openai.com/v1/chat/completions";

            [JsonProperty("model")]
            public string Model { get; set; } = "gpt-4o-mini";

            [JsonProperty("messages")]
            public List<ChatMessage> Messages { get; set; }

            public string Get_API_Url()
            {
                return API_Url;
            }
        }

        public enum role
        {
            [EnumMember(Value = "system")]
            system,
            [EnumMember(Value = "user")]
            user,
            [EnumMember(Value = "assistant")]
            assistant
        }

        [Serializable]
        public class ChatMessage
        {
            [JsonProperty("role"), JsonConverter(typeof(StringEnumConverter)), XmlAttribute("role")]
            public role Role;
            [JsonProperty("content"), XmlAttribute("content")]
            public string Message = "";
        }

        [Serializable]
        public class ChatResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("object")]
            public string Object { get; set; }

            [JsonProperty("created")]
            public int Created { get; set; }

            [JsonProperty("model")]
            public string Model { get; set; }

            [JsonProperty("system_fingerprint")]
            public string SystemFingerprint { get; set; }

            [JsonProperty("choices")]
            public List<ChatChoice> Choices { get; set; }

            [JsonProperty("usage")]
            public ChatUsage Usage { get; set; }
        }

        [Serializable]
        public class ChatChoice
        {
            [JsonProperty("index")]
            public int Index { get; set; }

            [JsonProperty("message")]
            public ChatMessage Message { get; set; }

            [JsonProperty("finish_reason")]
            public string FinishReason { get; set; }
        }

        [Serializable]
        public class ChatUsage
        {
            [JsonProperty("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonProperty("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonProperty("total_tokens")]
            public int TotalTokens { get; set; }
        }

        [Serializable]
        public class ImageAnalyzeRequest : URL
        {

            private const string API_Url = "https://api.openai.com/v1/chat/completions";
            [JsonProperty("model")]

            public const string model = "gpt-4-vision-preview";
            [JsonProperty("messages")]
            public List<ChatMessageAnalyze> Messages { get; set; }

            public string Get_API_Url()
            {
                return API_Url;
            }
        }

        [Serializable]
        public class ChatMessageAnalyze
        {
            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("content")]
            public ContentAnalyze Content { get; set; }
        }

        [Serializable]
        public class ContentAnalyze
        {

            [JsonProperty("text")]
            public string text { get; set; }

            [JsonProperty("image_url")]
            public string image_url { get; set; }
        }

        #endregion

        #region STT
        public struct FileData
        {
            public byte[] Data;
            public string Name;
        }

        [Serializable]
        public class OpenAISTTRequest : URL
        {
            public const string API_Url = "https://api.openai.com/v1/audio/transcriptions";

            [JsonProperty("file")]
            public FileData FileData { get; set; }

            [JsonProperty("model")]
            public string Model { get; set; } = "whisper-1";

            [JsonProperty("language")]
            public string Language { get; set; } = "ko";

            [JsonProperty("prompt")]
            public string Prompt { get; set; }

            [JsonProperty("response_format")]
            public string ResponseFormat { get; set; } = AudioResponseFormat.Text;

            [JsonProperty("temperature")]
            public float? Temperature { get; set; } = 0;

            public string Get_API_Url()
            {
                return API_Url;
            }
        }

        [Serializable]
        public class AudioResponseFormat
        {
            public const string Json = "json";
            public const string Text = "text";
            public const string Srt = "srt";
            public const string VerboseJson = "verbose_json";
            public const string Vtt = "vtt";
        }

        [Serializable]
        public class GoogleSTTRequest : URL
        {
            public const string API_Url = "https://speech.googleapis.com/v1/speech:recognize?key=";

            [JsonProperty("config")]
            public GoogleSTTConfig Config { get; set; } = new GoogleSTTConfig();

            [JsonProperty("audio")]
            public GoogleSTTAudio Audio { get; set; } = new GoogleSTTAudio();

            public string Get_API_Url()
            {
                return API_Url;
            }
        }

        [Serializable]
        public class GoogleSTTConfig
        {
            [JsonProperty("encoding")]
            public string Encoding { get; set; } = "LINEAR16";

            [JsonProperty("sampleRateHertz")]
            public int SampleRateHertz { get; set; } = 44100;

            [JsonProperty("languageCode")]
            public string LanguageCode { get; set; } = "ko-KR";

            [JsonProperty("alternativeLanguageCodes")]
            public string[] AlternativeLanguageCodes { get; set; } = new string[] { "en-US" };
#if UNITY_WEBGL && !UNITY_EDITOR
            [JsonProperty("audioChannelCount")]
            public int AudioChannelCount { get; set; } = 2;
#endif
        }

        public class GoogleSTTAudio
        {
            [JsonProperty("content")]
            public string Content { get; set; }
        }

        #endregion

        #region TTS
        public class GoogleTTSRequest : URL
        {
            public const string API_Url = "https://texttospeech.googleapis.com/v1beta1/text:synthesize?key=";

            [JsonProperty("input")]
            public GoogleInput Input { get; set; }

            [JsonProperty("voice")]
            public GoogleVoice Voice { get; set; } = new GoogleVoice();

            [JsonProperty("audioConfig")]
            public GoogleAudioConfig AudioConfig { get; set; } = new GoogleAudioConfig();

            public string Get_API_Url()
            {
                return API_Url;
            }
        }

        [Serializable]
        public class GoogleInput
        {
            [JsonProperty("text")]
            public string Text;
        }

        [Serializable]
        public class GoogleVoice
        {
            [JsonProperty("languageCode")]
            public string LanguageCode = "ko-KR";
            [JsonProperty("name")]
            public string Name = "ko-KR-Wavenet-A";
            [JsonProperty("ssmlGender")]
            public string SsmlGender = "FEMALE";
        }

        [Serializable]
        public class GoogleAudioConfig
        {
            [JsonProperty("audioEncoding")]
            public string AudioEncoding = "LINEAR16";
            [JsonProperty("speakingRate")]
            public float SpeakingRate = 1.2f;
            [JsonProperty("pitch")]
            public int Pitch = 0;
            [JsonProperty("volumeGainDb")]
            public int VolumeGainDb = 0;
        }

        [Serializable]
        public class GoogleContent
        {
            [JsonProperty("audioContent")]
            public string AudioContent;
        }

        [Serializable]
        public class TypecastTTSRequest : URL
        {
            public const string API_Url = "https://typecast.ai/api/speak";

            public TypecastRequest Data { get; set; } = new TypecastRequest();

            public string Get_API_Url()
            {
                return API_Url;
            }
        }

        [Serializable]
        public class TypecastRequest
        {
            [JsonProperty("text")]
            public string Text { get; set; }
            [JsonProperty("lang")]
            public string Language { get; set; } = "auto";
            [JsonProperty("actor_id")]
            public string ActorID { get; set; } = "5c789c317ad86500073a02cc";
            [JsonProperty("xapi_hd")]
            public bool XapiHD { get; set; } = true;
            [JsonProperty("model_version")]
            public string ModelVersion { get; set; } = "latest";
            [JsonProperty("emotion_tone_preset")]
            public string EmotionTonePreset { get; set; } = "toneup-4";
            [JsonProperty("max_seconds")]
            public int MaxSeconds { get; set; } = 60;
            [JsonProperty("last_pitch")]
            public int LastPitch { get; set; } = 1;
            [JsonProperty("speed_x")]
            public float SpeedX { get; set; } = 1f;
        }

        [Serializable]
        public class TypecastTTSResponce
        {
            [JsonProperty("result")]
            public TypecastResponce Result { get; set; }
        }

        [Serializable]
        public class TypecastResponce
        {
            [JsonProperty("speak_v2_url")]
            public string speak_v2_url { get; set; }
            [JsonProperty("status")]
            public string status { get; set; }
            [JsonProperty("audio_download_url")]
            public string audio_download_url { get; set; }
            [JsonProperty("xapi_hd")]
            public bool XapiHD { get; set; } = true;
            [JsonProperty("model_version")]
            public string ModelVersion { get; set; } = "latest";
            [JsonProperty("emotion_tone_preset")]
            public string EmotionTonePreset { get; set; } = "toneup-1";
        }
        #endregion
    }
}