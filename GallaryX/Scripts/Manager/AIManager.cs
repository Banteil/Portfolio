using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using static starinc.io.gallaryx.ChatAIAPI;
using Microphone = FrostweepGames.MicrophonePro.Microphone;

namespace starinc.io.gallaryx
{
    public class AIManager : Singleton<AIManager>
    {
        public string OpenAIKey { get; set; }
        public string GoogleTTSKey { get; set; }
        public string GoogleSTTKey { get; set; }

        public bool IsRecording { get; private set; }

        private AudioSource _ttsSource;
        private AudioClip _recordClip;

        private string _systemMessage;

        private int _frequency = 44100;
        private int _recordingTime = 10;

        public bool ActiveTTS { get; set; } = true;
        [FrostweepGames.Plugins.ReadOnly]
        public string SelectedDevice;
        [FrostweepGames.Plugins.ReadOnly]
        public bool PermissionGranted;

        public bool PreparingTTS { get { return _ttsSource.isPlaying || _ttsSource.clip != null; } }

        public event Action<string> SendMessageCallback;
        public event Action StopRecordingCallback;

        private Coroutine _recordRoutine;

        protected override async void OnAwake()
        {
            _ttsSource = Util.GetOrAddComponent<AudioSource>(gameObject);
            GameManager.Instance.ChangeVolumeCallback += (float vol) => { _ttsSource.volume = vol; };
            GameManager.Instance.ChangeLocaleCallback += SettingAISystem;

            SetKeyInfo();
            var handle = LocalizationSettings.InitializationOperation;
            await handle;
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Localization initialized successfully.");
                SettingAISystem(LocalizationSettings.SelectedLocale.Identifier.Code);
            }
        }

        private async void SetKeyInfo()
        {
            OpenAIKey = await CallAPI.GetAPIKey(Define.APIKey.open_ai_key.ToString());
            GoogleTTSKey = await CallAPI.GetAPIKey(Define.APIKey.google_tts_key.ToString());
            GoogleSTTKey = await CallAPI.GetAPIKey(Define.APIKey.google_stt_key.ToString());
        }

        public async void SettingAISystem(string locale)
        {
            _systemMessage = "인스타버스 메타버스 미술 전시관에서 도슨트 역할을 하는 친절하고 지식이 풍부한 챗봇입니다. 존댓말과 활기찬 어조로 사용자들과 소통합니다. 인스타버스와 미술, 전시관과 관련된 주제에 대해 답변을 제공할 수 있습니다. 미술 혹은 인스타버스, 전시관의 정보와 관련되지 않은 주제나 질문에 대해서는 답변을 제공하지 않습니다. ";
            _systemMessage += await GetSystemMessageForLocale(locale);
            //try
            //{
            //    _systemMessage = "인스타버스 메타버스 미술 전시관에서 도슨트 역할을 하는 친절하고 지식이 풍부한 챗봇입니다. 존댓말과 활기찬 어조로 사용자들과 소통합니다. 인스타버스와 미술, 전시관과 관련된 주제에 대해 답변을 제공할 수 있습니다. 미술 혹은 인스타버스, 전시관의 정보와 관련되지 않은 주제나 질문에 대해서는 답변을 제공하지 않습니다. ";
            //    _systemMessage += await GetSystemMessageForLocale(locale);
            //}
            //catch
            //{
            //    Debug.LogError("DB AI 설정 정보 획득 실패");
            //    _systemMessage = "인스타버스 메타버스 미술 전시관에서 도슨트 역할을 하는 친절하고 지식이 풍부한 챗봇입니다. 존댓말과 활기찬 어조로 사용자들과 소통합니다. 인스타버스와 미술, 전시관과 관련된 주제에 대해 답변을 제공할 수 있습니다. 미술 혹은 인스타버스, 전시관의 정보와 관련되지 않은 주제나 질문에 대해서는 답변을 제공하지 않습니다.";
            //    _systemMessage += await GetSystemMessageForLocale(locale);
            //}
        }

        private async UniTask<string> GetSystemMessageForLocale(string locale)
        {
            try
            {
                var exhibitionData = await CallAPI.GetExhibitionData(GameManager.Instance.Seq);
                var language = "";
                var exhibitionTitle = exhibitionData?.title;
                var exhibitionInfo = Util.FilterUnsupportedTags(exhibitionData?.description);
                var artistDetail = Util.FilterUnsupportedTags(exhibitionData?.artist_details);

                switch (locale)
                {
                    case "en":
                        language = "You must respond only in English, regardless of the language used in the user's question.";
                        break;
                    case "ko":
                        language = "사용자가 어떤 언어로 질문하든지, 오직 한국어로만 응답해야 합니다.";
                        break;
                    default:
                        language = "You must respond only in English, regardless of the language used in the user's question.";
                        break;
                }
                var result = $"전시관 이름 : {exhibitionTitle}, 전시관 정보 : {exhibitionInfo}, 작가 정보 : {artistDetail}, {language}";
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                var language = "";
                switch (locale)
                {
                    case "en":
                        language = "You must respond only in English, regardless of the language used in the user's question.";
                        break;
                    case "ko":
                        language = "사용자가 어떤 언어로 질문하든지, 오직 한국어로만 응답해야 합니다.";
                        break;
                    default:
                        language = "You must respond only in English, regardless of the language used in the user's question.";
                        break;
                }
                return language;
            }
        }

        public async UniTask<string> GetOpenAIChat(string userMessage)
        {
            ChatRequest chatRequest = new ChatRequest();
            chatRequest.Messages = new List<ChatMessage>
            {
                new ChatMessage() { Role = role.system, Message = _systemMessage },
                new ChatMessage() { Role = role.user, Message = userMessage },
            };

            string chatText = "";
            var chatResponse = await ClieantResponseChat(chatRequest);

            if (chatResponse != null)
            {
                var data = chatResponse.Choices;
                foreach (var choice in data)
                {
                    chatText += choice.Message.Message;
                }
                return chatText;
            }
            else
            {
                return null;
            }
        }

        public async UniTask<List<AudioClip>> GetTTSClips(string messageText, string splitStr)
        {
            List<AudioClip> clipList = new List<AudioClip>();
            var splitText = messageText.Split(splitStr);
            for (int i = 0; i < splitText.Length; i++)
            {
                //var textToSpeech = new TypecastTTSRequest() { Data = new TypecastRequest() { Text = messageText } };
                var textToSpeech = new GoogleTTSRequest() { Input = new GoogleInput() { Text = messageText } };
                var clip = await ClieantResponseTTS(textToSpeech);
                clipList.Add(clip);
            }
            return clipList;
        }

        public async UniTask<bool> SettingTTS(string receiveText)
        {
            if (!ActiveTTS) return false;
            //var textToSpeech = new TypecastTTSRequest() { Data = new TypecastRequest() { Text = receiveText } };
            var textToSpeech = new GoogleTTSRequest() { Input = new GoogleInput() { Text = receiveText } };
            var clip = await ClieantResponseTTS(textToSpeech);
            if (clip != null)
            {
                _ttsSource.clip = clip;
                return true;
            }
            return false;
        }

        public void SetTTSClip(AudioClip clip) => _ttsSource.clip = clip;

        public void PlayTTS()
        {
            _ttsSource.Play();
            CheckTTSState();
        }

        public void StopTTS() => _ttsSource.Stop();

        private async void CheckTTSState()
        {
            while (true)
            {
                await UniTask.Yield();
                if (!_ttsSource.isPlaying && _ttsSource.clip != null)
                {
                    Destroy(_ttsSource.clip);
                    _ttsSource.clip = null;
                    return;
                }
            }
        }

        public void StartRecord()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.Log("마이크 디바이스가 없음.");
                return;
            }

            Debug.Log("녹음 시작" + SelectedDevice);
            _recordClip = Microphone.Start(SelectedDevice, false, _recordingTime, _frequency);
            if (_recordRoutine != null)
            {
                StopCoroutine(_recordRoutine);
                _recordRoutine = null;
            }
            _recordRoutine = StartCoroutine(CheckRecordingTime());
            IsRecording = true;
        }

        private IEnumerator CheckRecordingTime()
        {
            var currentTime = 0f;
            while (currentTime < _recordingTime)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }
            _recordRoutine = null;
            StopRecord();            
        }

        public async void StopRecord()
        {
            if (_recordRoutine != null)
            {
                StopCoroutine(_recordRoutine);
                _recordRoutine = null;
            }
            
            if (Microphone.devices.Length == 0)
            {
                Debug.Log("마이크 디바이스가 없음.");
                return;
            }

            Microphone.End(SelectedDevice);

            float[] samples = new float[_recordClip.samples * _recordClip.channels];
            if (!_recordClip.GetData(samples, 0) || IsSilent(samples))
            {
                Debug.LogError("무음이거나, 녹음에 문제가 있음");
                Destroy(_recordClip);
                IsRecording = false;
                StopRecordingCallback?.Invoke();
                return;
            }

            var data = Util.ToWav(_recordClip);
            Debug.Log("녹음 종료 : " + data.Length);
            var req = new OpenAISTTRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
            };
            //var content = Convert.ToBase64String(data);
            //var req = new GoogleSTTRequest
            //{
            //    Config = new GoogleSTTConfig(),
            //    Audio = new GoogleSTTAudio() { Content = content },
            //};
            var message = await CreateAudioTranslation(req);
            Debug.Log(message);
            if (!string.IsNullOrEmpty(message) && !string.IsNullOrWhiteSpace(message))
                SendMessageCallback?.Invoke(message);
            Destroy(_recordClip);
            IsRecording = false;
            StopRecordingCallback?.Invoke();
        }

        private bool IsSilent(float[] samples, float threshold = 0.01f, int steps = 20)
        {
            if (samples.Length < steps)
                steps = samples.Length;
            int stepSize = samples.Length / steps;  // 샘플링 간격
            if(stepSize == 0) stepSize = 1;

            for (int i = stepSize; i < samples.Length; i += stepSize)
            {
                if (Mathf.Abs(samples[i]) > threshold)
                {
                    return false;
                }                
            }
            return true;
        }
    }
}
