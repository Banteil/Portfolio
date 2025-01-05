using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class UISettingPopup : UIPopup
    {
        enum SettingButton
        {
            LanguageSetting,
            SoundSetting,
            MicrophoneSetting
        }        

        private UILanguageSubmenu _languageSubmenu;
        private UISoundSubmenu _soundSubmenu;
        private UIMicrophoneSubmenu _microphoneSubmenu;

        public bool IsEnable
        {
            get { return UICanvas.enabled; }
            set
            {
                UICanvas.enabled = value;
                if(value)
                {
                    if(Util.IsMobileWebPlatform)
                    {
                        var rectTr = (RectTransform)transform;
                        rectTr.anchoredPosition = Util.IsLandscape ? new Vector2(-15f, -120f) : new Vector2(-15f, -260f);
                    }
                }
                else
                {
                    var submenus = GetComponentsInChildren<UISubmenu>();
                    foreach (var submenu in submenus)
                    {
                        submenu.IsEnable = false;
                    }
                    UIManager.Instance.RemovePopupUI<UISettingPopup>();
                }
            }
        }

        protected override void ScreenOrientationChanged(bool isLandscape)
        {
            base.ScreenOrientationChanged(isLandscape);
            var rectTr = (RectTransform)transform;
            rectTr.anchoredPosition = isLandscape ? new Vector2(-15f, -120f) : new Vector2(-15f, -260f);
        }

        protected override void OnAwake()
        {
            Bind<Button>(typeof(SettingButton));
            var language = GetButton((int)SettingButton.LanguageSetting);
            language.gameObject.BindEvent(LanguageMenu);
            var sound = GetButton((int)SettingButton.SoundSetting);
            sound.gameObject.BindEvent(SoundMenu);
            var microphone = GetButton((int)SettingButton.MicrophoneSetting);
            microphone.gameObject.BindEvent(MicrophoneMenu);

            _languageSubmenu = GetComponentInChildren<UILanguageSubmenu>();
            _soundSubmenu = GetComponentInChildren<UISoundSubmenu>();
            _microphoneSubmenu = GetComponentInChildren<UIMicrophoneSubmenu>();
        }

        private void LanguageMenu(PointerEventData data)
        {
            _languageSubmenu.IsEnable = !_languageSubmenu.IsEnable;
            _soundSubmenu.IsEnable = false;
            _microphoneSubmenu.IsEnable = false;
        }

        private void SoundMenu(PointerEventData data)
        {
            _soundSubmenu.IsEnable = !_soundSubmenu.IsEnable;
            _languageSubmenu.IsEnable = false;
            _microphoneSubmenu.IsEnable = false;
        }

        private void MicrophoneMenu(PointerEventData data)
        {
            _microphoneSubmenu.IsEnable = !_microphoneSubmenu.IsEnable;
            _languageSubmenu.IsEnable = false;
            _soundSubmenu.IsEnable = false;
        }

        protected override void OnCloseButtonClicked(PointerEventData data)
        {
            UIManager.Instance.RemovePopupUI<UISettingPopup>();
            IsEnable = false;
        }

    }
}