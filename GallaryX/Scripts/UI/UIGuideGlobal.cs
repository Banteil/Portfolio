using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class UIGuideGlobal : UIGlobal
    {
        enum GuideImage
        {
            MicrophoneState,
        }

        enum GuideCanvasGroup
        {
            PCGroup,
            MobileLandscapeGroup,
            MobilePortraitGroup,
        }

        enum GuideText
        {
            InfoText,
        }

        private CanvasGroup _mainGroup;
        private UIChatPopup _uiChatPopup;
        private UISettingPopup _uiSettingPopup;
        private Tween _blinkTween;

        protected override void OnAwake()
        {
            base.OnAwake();
            Bind<Image>(typeof(GuideImage));
            Bind<CanvasGroup>(typeof(GuideCanvasGroup));
            Bind<TextMeshProUGUI>(typeof(GuideText));

            var popups = transform.Find("Popups");
            Transform mainPopup;

            if (Util.IsMobileWebPlatform)
            {
                _mainGroup = Get<CanvasGroup>((int)(Util.IsLandscape ? GuideCanvasGroup.MobileLandscapeGroup : GuideCanvasGroup.MobilePortraitGroup));
                mainPopup = popups.Find("MobilePopups");
                popups.Find("PCPopups").gameObject.SetActive(false);

                var micStateRT = (RectTransform)GetImage((int)GuideImage.MicrophoneState).transform;
                micStateRT.anchorMax = new Vector2(0.5f, Util.IsLandscape ? 0 : 1);
                micStateRT.anchorMin = new Vector2(0.5f, Util.IsLandscape ? 0 : 1);
                micStateRT.pivot = new Vector2(0.5f, Util.IsLandscape ? 0 : 1);
                micStateRT.anchoredPosition = new Vector2(0, Util.IsLandscape ? 10f : -100f);
                micStateRT.localScale = Util.IsLandscape ? Vector3.one : new Vector3(1.5f, 1.5f);
            }
            else
            {
                _mainGroup = Get<CanvasGroup>((int)GuideCanvasGroup.PCGroup);
                mainPopup = popups.Find("PCPopups");
                popups.Find("MobilePopups").gameObject.SetActive(false);
            }

            _mainGroup.alpha = 1f;
            _mainGroup.interactable = true;
            _mainGroup.blocksRaycasts = true;

            _uiChatPopup = mainPopup.GetComponentInChildren<UIChatPopup>();
            _uiSettingPopup = mainPopup.GetComponentInChildren<UISettingPopup>();

            var groups = transform.Find("Groups");
            for (int i = 0; i < groups.childCount; i++)
            {
                groups.GetChild(i).gameObject.SetActive(groups.GetChild(i) == _mainGroup.transform);
            }
        }

        protected override void OnStart()
        {
            if (_uiChatPopup.IsEnable) _uiChatPopup.IsEnable = false;
            if (_uiSettingPopup.IsEnable) _uiSettingPopup.IsEnable = false;

            InputManager.Instance.KeyInputEvent.InputUpCallback += MicrophoneShortcut;
            AIManager.Instance.StopRecordingCallback += InactiveMicrophone;
        }

        protected override void ScreenOrientationChanged(bool isLandscape)
        {
            base.ScreenOrientationChanged(isLandscape);
            var prevGroup = _mainGroup;            
            _mainGroup = Get<CanvasGroup>((int)(isLandscape ? GuideCanvasGroup.MobileLandscapeGroup : GuideCanvasGroup.MobilePortraitGroup));
            _mainGroup.alpha = prevGroup.alpha;
            _mainGroup.blocksRaycasts = prevGroup.blocksRaycasts;
            _mainGroup.interactable = prevGroup.interactable;
            _mainGroup.gameObject.SetActive(prevGroup.gameObject.activeSelf);
            prevGroup.gameObject.SetActive(false);

            var micStateRT = (RectTransform)GetImage((int)GuideImage.MicrophoneState).transform;
            micStateRT.anchorMax = new Vector2(0.5f, isLandscape ? 0 : 1);
            micStateRT.anchorMin = new Vector2(0.5f, isLandscape ? 0 : 1);
            micStateRT.pivot = new Vector2(0.5f, isLandscape ? 0 : 1);
            micStateRT.anchoredPosition = new Vector2(0, isLandscape ? 10f : -100f);
            micStateRT.localScale = isLandscape ? Vector3.one : new Vector3(1.5f, 1.5f);
        }

        public void ActiveChatUI()
        {
            if (!_uiChatPopup.IsEnable)
            {
                _uiChatPopup.EnableCallback += MobileChatUICallback;
                UIManager.Instance.AddPopupUI(_uiChatPopup);
                _uiChatPopup.IsEnable = true;
                _uiChatPopup.transform.SetAsLastSibling();
            }
            else
            {
                _uiChatPopup.IsEnable = false;
                _uiChatPopup.EnableCallback -= MobileChatUICallback;
            }
        }

        /// <summary>
        /// 모바일 채팅 UI 오픈 시 다른 UI 모두 임시 비활성화 
        /// </summary>
        /// <param name="isActive"></param>
        private void MobileChatUICallback(bool isActive)
        {
            if (Util.IsMobileWebPlatform)
            {
                _mainGroup.gameObject.SetActive(!isActive);
                _uiSettingPopup.gameObject.SetActive(!isActive);

                UIManager.Instance.SceneUI.UICanvas.enabled = !isActive;
                var popupList = UIManager.Instance.GetPopupList();
                for (int i = 0; i < popupList.Count; i++)
                {
                    if (popupList[i] == _uiChatPopup) continue;
                    popupList[i].UICanvas.enabled = !isActive;
                }

                InputManager.Instance.MobileVirtualController.InactiveRunButton();
                InputManager.Instance.MobileVirtualController.UICanvas.enabled = !isActive;
            }
        }

        public void ActiveSettingUI()
        {
            if (!_uiSettingPopup.IsEnable)
            {
                UIManager.Instance.AddPopupUI(_uiSettingPopup);
                _uiSettingPopup.IsEnable = true;
                _uiSettingPopup.transform.SetAsLastSibling();
            }
            else
            {
                _uiSettingPopup.IsEnable = false;
            }
        }

        private void ActiveTTSVoice(bool isOff)
        {
            AIManager.Instance.ActiveTTS = !isOff;
            if (isOff)
                AIManager.Instance.StopTTS();
        }

        private void MicrophoneShortcut()
        {
            if (Util.IsUIFocusing) return;
            if (InputManager.Instance.KeyInputEvent.InputRecordKey() == InputState.Up)
            {
                if (!AIManager.Instance.IsRecording)
                {
                    ActiveMicrophone();
                }
                else
                {
                    InactiveMicrophone();
                }
            }
        }

        public void ActiveMicrophone()
        {
            var micState = GetImage((int)GuideImage.MicrophoneState);
            var color = micState.color;
            color.a = 0;
            micState.color = color;
            micState.enabled = true;
            if (_blinkTween != null)
            {
                _blinkTween.Kill();
                _blinkTween = null;
            }
            _blinkTween = micState.DOFade(1, 1).SetLoops(-1, LoopType.Yoyo);
            AIManager.Instance.StartRecord();
        }

        public void InactiveMicrophone()
        {
            if (_blinkTween != null)
            {
                _blinkTween.Kill();
                _blinkTween = null;
                GetImage((int)GuideImage.MicrophoneState).enabled = false;
                AIManager.Instance.StopRecord();
            }
        }
    }
}
