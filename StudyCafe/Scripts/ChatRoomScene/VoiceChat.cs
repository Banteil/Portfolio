using FrostweepGames.Plugins.Native;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FrostweepGames.WebGLPUNVoice.Examples
{
    public class VoiceChat : MonoBehaviour
    {
        private List<RemoteSpeakerItem> _remoteSpeakerItems;

        public Dropdown microphonesDropdown;

        public Button refreshMicrophonesButton;

        public Toggle debugEchoToggle;

        public Toggle reliableTransmissionToggle;

        public Toggle muteRemoteClientsToggle;

        public Image mutoRemoteImage;

        public Transform parentOfRemoteClients;

        public Toggle muteMyClientToggle;

        public GameObject remoteClientPrefab;

        public Recorder recorder;

        public Listener listener;

        private void Start()
        {
            refreshMicrophonesButton.onClick.AddListener(RefreshMicrophonesButtonOnClickHandler);
            muteMyClientToggle.onValueChanged.AddListener(MuteMyClientToggleValueChanged);
            muteRemoteClientsToggle.onValueChanged.AddListener(MuteRemoteClientsToggleValueChanged);
            debugEchoToggle.onValueChanged.AddListener(DebugEchoToggleValueChanged);
            reliableTransmissionToggle.onValueChanged.AddListener(ReliableTransmissionToggleValueChanged);

            _remoteSpeakerItems = new List<RemoteSpeakerItem>();

            RefreshMicrophonesButtonOnClickHandler();

            listener.SpeakersUpdatedEvent += SpeakersUpdatedEventHandler;
        }

        /// <summary>
        /// 스피커 업데이트 핸들러 함수
        /// </summary>
        private void SpeakersUpdatedEventHandler()
        {
            if(_remoteSpeakerItems.Count > 0)
            {
                foreach(var item in _remoteSpeakerItems)
                {
                    item.Dispose();
                }
                _remoteSpeakerItems.Clear();
            }

            foreach(var speaker in listener.Speakers)
            {
                _remoteSpeakerItems.Add(new RemoteSpeakerItem(parentOfRemoteClients, remoteClientPrefab, speaker.Value));
            }
        }

        /// <summary>
        /// 연결 마이크 정보 새로고침
        /// </summary>
        public void RefreshMicrophonesButtonOnClickHandler()
        {
            Debug.Log("마이크 정보 새로고침");
            CustomMicrophone.RequestMicrophonePermission();

            microphonesDropdown.ClearOptions();
            microphonesDropdown.AddOptions(CustomMicrophone.devices.ToList());            
        }

        /// <summary>
        /// 클라이언트(자신) 음소거 여부 전환 함수
        /// </summary>
        public void MuteMyClientToggleValueChanged(bool status)
        {
            if (RoomManager.Instance.chatRoom.banMicrophone) return;
            Debug.Log("Mute : " + status);
            if(status)
            {
                recorder.StartRecord();
                mutoRemoteImage.color = new Color(255, 255, 255, 0);
            }
            else
            {
                recorder.StopRecord();
                mutoRemoteImage.color = new Color(255, 255, 255, 255);
            }
        }

        /// <summary>
        /// 원격 클라이언트 음소거 상태 변경(모든 활성 스피커의 음소거 상태를 설정)
        /// </summary>
        private void MuteRemoteClientsToggleValueChanged(bool status)
        {
            listener.SetMuteStatus(status);
        }

        /// <summary>
        /// 디버그 에코 상태 변경(자신한테 자신의 음성 들리는 것 테스트)
        /// </summary>
        private void DebugEchoToggleValueChanged(bool status)
        {
            recorder.debugEcho = status;
        }

        /// <summary>
        /// 안정적인 전송 상태 변경(네트워크를 통한 전송이 신뢰할 수 있는지 여부)
        /// </summary>
        private void ReliableTransmissionToggleValueChanged(bool status)
        {
            recorder.reliableTransmission = status;
        }


        private class RemoteSpeakerItem
        {
            private GameObject _selfObject;

            private Text _speakerNameText;

            public Speaker Speaker { get; private set; }

            public RemoteSpeakerItem(Transform parent, GameObject prefab, Speaker speaker)
            {
                Speaker = speaker;
                _selfObject = Instantiate(prefab, parent, false);
                _speakerNameText = _selfObject.transform.Find("Text").GetComponent<Text>();
                _speakerNameText.text = Speaker.Name;
            }

            public void Dispose()
            {
                Destroy(_selfObject);
            }
        }
    }
}