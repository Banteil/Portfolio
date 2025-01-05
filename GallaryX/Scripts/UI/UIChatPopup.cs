using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class UIChatPopup : UIPopup
    {
        enum ChatInputField
        {
            ChatInputField,
        }

        enum ChatScrollView
        {
            ChatScrollView,
        }

        enum ChatButton
        {
            RecordButton,
            SendButton,
        }

        private const int LIMITCOUNT = 50;
        private List<UIChatList> _chatLists = new List<UIChatList>();
        private bool _isReceiving = false;

        public event Action SendTextCallback, ReceiveTextCallback;
        public event Action<bool> EnableCallback;

        public bool IsEnable
        {
            get { return UICanvas.enabled; }
            set
            {
                UICanvas.enabled = value;
                if (value)
                {
                    InputManager.Instance.KeyInputEvent.InputDownCallback += InputEnterKey;
#if UNITY_WEBGL
                    InputManager.Instance.InputEnterCallback = () => SendChatText(null);
#endif
                }
                else
                {
                    InputManager.Instance.InputEnterCallback -= InputEnterKey;
#if UNITY_WEBGL
                    InputManager.Instance.InputEnterCallback = null;
#endif
                    UIManager.Instance.RemovePopupUI<UIChatPopup>();
                }
                EnableCallback?.Invoke(value);
            }
        }

        protected override void ScreenOrientationChanged(bool isLandscape)
        {
            base.ScreenOrientationChanged(isLandscape);
            for (int i = 0; i < _chatLists.Count; i++)
            {
                _chatLists[i].SetPreferredWidth(isLandscape ? 1100 : 800);
            }
        }

        protected override void OnAwake()
        {
            Bind<Button>(typeof(ChatButton));
            var sendButton = GetButton((int)ChatButton.SendButton);
            sendButton.gameObject.BindEvent(SendChatText);
            var recordButton = GetButton((int)ChatButton.RecordButton);
            recordButton.gameObject.BindEvent(RecordInfo);

            Bind<ScrollRect>(typeof(ChatScrollView));
            Bind<TMP_InputField>(typeof(ChatInputField));

            SendTextCallback += CheckListLimit;
            SendTextCallback += SetScroll;
            SendTextCallback += () => AIManager.Instance.StopTTS();
            ReceiveTextCallback += CheckListLimit;
            ReceiveTextCallback += SetScroll;
            AIManager.Instance.SendMessageCallback += SendMessageToAI;
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            UIManager.Instance.RemovePopupUI<UIChatPopup>();
            IsEnable = false;
        }

        private void RecordInfo(PointerEventData data)
        {
            var global = UIManager.Instance.GlobalUI.GetComponent<UIGuideGlobal>();
            if (!AIManager.Instance.IsRecording)
                global.ActiveMicrophone();
            else
                global.InactiveMicrophone();
        }

        public void SendChatText(PointerEventData data)
        {
            if (_isReceiving) return;
            //인풋 필드 텍스트 저장 및 초기화
            var inputField = Get<TMP_InputField>((int)ChatInputField.ChatInputField);
            var text = inputField.text;
            if (string.IsNullOrEmpty(text)) return;
            inputField.text = "";
            //입력한 텍스트 표시 및 수신 처리
            var scrollView = Get<ScrollRect>((int)ChatScrollView.ChatScrollView);
            var chatList = CreateChatList();
            chatList.SetText(text);
            inputField.ActivateInputField();
            SendTextCallback?.Invoke();

            ReceiveChatText(text);
        }

        public void SendMessageToAI(string message)
        {
            if (_isReceiving) return;
            if (string.IsNullOrEmpty(message)) return;
            var scrollView = Get<ScrollRect>((int)ChatScrollView.ChatScrollView);
            var chatList = CreateChatList();
            chatList.SetText(message);
            SendTextCallback?.Invoke();

            ReceiveChatText(message);
        }

        public async void ReceiveChatText(string text)
        {
            _isReceiving = true;
            try
            {
                var aiChatList = CreateChatList(ChatBoxType.OPPONENT);
                var aiText = await AIManager.Instance.GetOpenAIChat(text);
                var splitText = Util.ExtractingPatternString(aiText);
                aiText = Util.ReplacePatternString(aiText);
                if (AIManager.Instance.ActiveTTS)
                {
                    var ttsReady = await AIManager.Instance.SettingTTS(aiText);
                    if (ttsReady)
                        AIManager.Instance.PlayTTS();
                }
                aiChatList.SetText(aiText);
                if (splitText != null)
                    aiChatList.ActiveLinkButton(splitText);

                ReceiveTextCallback?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                var aiChatList = CreateChatList(ChatBoxType.OPPONENT);
                aiChatList.SetText("A network error has been confirmed. Please try again in a few minutes.");
            }
            _isReceiving = false;
        }

        public async void DocentSpeaking(string text)
        {
            try
            {
                var aiChatList = CreateChatList(ChatBoxType.OPPONENT);
                if (AIManager.Instance.ActiveTTS)
                {
                    var ttsReady = await AIManager.Instance.SettingTTS(text);
                    if (ttsReady)
                        AIManager.Instance.PlayTTS();
                }
                aiChatList.SetText(text);
                ReceiveTextCallback?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void InputEnterKey()
        {
            if (InputManager.Instance.KeyInputEvent.InputReturnKey() == InputState.Down || InputManager.Instance.KeyInputEvent.InputKeypadEnterKey() == InputState.Down)
                SendChatText(null);
        }

        private void CheckListLimit()
        {
            if (_chatLists.Count > LIMITCOUNT)
            {
                Destroy(_chatLists[0].gameObject);
                _chatLists.RemoveAt(0);
            }
        }

        public UIChatList CreateChatList(ChatBoxType type = ChatBoxType.MINE)
        {
            var scrollView = Get<ScrollRect>((int)ChatScrollView.ChatScrollView);
            var chatList = UIManager.Instance.AddListUI<UIChatList>(_chatLists.Count, scrollView.content, "ChatListUI");
            chatList.SetType(type);
            if (Util.IsMobileWebPlatform)
            {
                chatList.SetPreferredWidth(Util.IsLandscape ? 1100 : 800);
                chatList.SetFontSize(30);
            }
            _chatLists.Add(chatList);
            return chatList;
        }

        private async void SetScroll()
        {
            await UniTask.WaitForSeconds(0.1f);
            var scrollView = Get<ScrollRect>((int)ChatScrollView.ChatScrollView);
            scrollView.verticalNormalizedPosition = 0;
        }

        public void MobileEndEdit(string message)
        {
            if (!Util.IsMobileWebPlatform) return;
            var inputField = Get<TMP_InputField>((int)ChatInputField.ChatInputField);
            var text = inputField.text;
            inputField.text = "";
            SendMessageToAI(text);
        }

        public void MobileCloseButton() => OnCloseButtonClicked(null);
    }
}