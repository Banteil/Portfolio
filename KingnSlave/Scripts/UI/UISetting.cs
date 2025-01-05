using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class SettingData
    {
        public float MasterVolume = 1f;
        public float BGMVolume = 1f;
        public float SFXVolume = 1f;
        public int LocaleIndex = 0;
        public bool IsAdActive = false;
        public bool AdAccept = true;
    }


    public class UISetting : UIPopup
    {
        private enum SettingSliders
        {
            MasterVolumeSlider,
            BGMVolumeSlider,
            SFXVolumeSlider,
        }

        private enum SettingText
        {
            VersionText,
        }

        private enum SettingSideButtonSelectUI
        {
            SelectLocalizationUI,
        }

        private enum SettingLabelToggles
        {
            PushToggle,
        }

        private enum SettingButtons
        {
            HelpButton = 1,
            ExitButton,
            ShareGameLinkButton,
            PrivacyPolicyButton,
            TermsOfServiceButton,
            WithdrawalButton,
        }

        private SettingData settingData;

        private void Start() => Initialized();

        protected override void InitializedProcess()
        {
            base.InitializedProcess();
            BindUI();
            LoadSettingData();
            LocalizationSettings.SelectedLocaleChanged += ChangeLocaleCallback;
        }

        private void ChangeLocaleCallback(Locale locale)
        {
            var index = LocalizationSettings.AvailableLocales.Locales.IndexOf(locale);
            var localizationUI = Get<SideButtonSelectUI>((int)SettingSideButtonSelectUI.SelectLocalizationUI);
            localizationUI.SetValue(index);
            settingData.LocaleIndex = index;
            Util.SaveSettingData(settingData);
        }

        private void BindUI()
        {
            Bind<SideButtonSelectUI>(typeof(SettingSideButtonSelectUI));
            var localizationUI = Get<SideButtonSelectUI>((int)SettingSideButtonSelectUI.SelectLocalizationUI);
            localizationUI.gameObject.BindEvent(ClickLocalizationUI);
            localizationUI.OnValueChanged.AddListener(SetLocalizationValue);
            localizationUI.MinValue = 0;
            localizationUI.MaxValue = LocalizationSettings.AvailableLocales.Locales.Count - 1;

            Bind<Slider>(typeof(SettingSliders));
            var masterSlider = Get<Slider>((int)SettingSliders.MasterVolumeSlider);
            masterSlider.gameObject.BindEvent((data) => LogManager.Instance.InsertActionLog(30));
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            var bgmSlider = Get<Slider>((int)SettingSliders.BGMVolumeSlider);
            bgmSlider.gameObject.BindEvent((data) => LogManager.Instance.InsertActionLog(31));
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            var sfxSlider = Get<Slider>((int)SettingSliders.SFXVolumeSlider);
            sfxSlider.gameObject.BindEvent((data) => LogManager.Instance.InsertActionLog(32));
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);

            Bind<LabelToggle>(typeof(SettingLabelToggles));

            Bind<Button>(typeof(SettingButtons));
            var exitButton = GetButton((int)SettingButtons.ExitButton);
            exitButton.gameObject.BindEvent(ClickExitButton);
            var shareGameLinkButton = GetButton((int)SettingButtons.ShareGameLinkButton);
            shareGameLinkButton.gameObject.BindEvent(ShareGameLinkButton);
            var helpButton = GetButton((int)SettingButtons.HelpButton);
            helpButton.gameObject.BindEvent(ClickHelpButton);
            var ppButton = GetButton((int)SettingButtons.PrivacyPolicyButton);
            ppButton.gameObject.BindEvent(ClickPrivacyPolicyButton);
            var tosButton = GetButton((int)SettingButtons.TermsOfServiceButton);
            tosButton.gameObject.BindEvent(ClickTermsOfServiceButton);
            var witdrawalButton = GetButton((int)SettingButtons.WithdrawalButton);
            witdrawalButton.gameObject.BindEvent(ClickWithdrawalButton);

            Bind<TextMeshProUGUI>(typeof(SettingText));
            GetText((int)SettingText.VersionText).text = $"Version {Application.version}";
        }

        private void LoadSettingData()
        {
            settingData = Util.LoadSettingData();

            var masterSlider = Get<Slider>((int)SettingSliders.MasterVolumeSlider);
            masterSlider.value = settingData.MasterVolume;

            var bgmSlider = Get<Slider>((int)SettingSliders.BGMVolumeSlider);
            bgmSlider.value = settingData.BGMVolume;

            var sfxSlider = Get<Slider>((int)SettingSliders.SFXVolumeSlider);
            sfxSlider.value = settingData.SFXVolume;

            var localizationUI = Get<SideButtonSelectUI>((int)SettingSideButtonSelectUI.SelectLocalizationUI);
            localizationUI.Value = settingData.LocaleIndex;

            //_tempToggle.isOn = settingData.IsAdActive;
        }

        private void ClickLocalizationUI(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(33);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(Define.SFXTableIndex.ClickNormalButton));
            UIManager.Instance.ShowUI<UISelectLocalization>("SelectLocalizationUI");
        }

        /// <summary>
        /// 언어 설정 변경 함수
        /// </summary>
        private void SetLocalizationValue()
        {
            var localizationUI = Get<SideButtonSelectUI>((int)SettingSideButtonSelectUI.SelectLocalizationUI);
            var index = localizationUI.Value;

            if (index >= LocalizationSettings.AvailableLocales.Locales.Count || index < 0)
            {
                Debug.Log("잘못된 인덱스, 혹은 지원되는 언어가 아닙니다.");
                return;
            }

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
            settingData.LocaleIndex = index;
            Util.SaveSettingData(settingData);
        }

        private void SetMasterVolume(float value)
        {
            AudioManager.Instance.SetMasterVolume(value);
            settingData.MasterVolume = value;
            Util.SaveSettingData(settingData);
        }

        private void SetBGMVolume(float value)
        {
            AudioManager.Instance.SetMusicVolume(value);
            settingData.BGMVolume = value;
            Util.SaveSettingData(settingData);
        }

        private void SetSFXVolume(float value)
        {
            AudioManager.Instance.SetSoundEffectVolume(value);
            settingData.SFXVolume = value;
            Util.SaveSettingData(settingData);
        }

        private void ClickHelpButton(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(35);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
#if UNITY_EDITOR
            Debug.Log("도움말 버튼");
#else
            CallWebView.ShowUrlFullScreen(Define.HelpURL, "helpWebViewTitle");
#endif
        }

        private void ClickPrivacyPolicyButton(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(36);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
#if UNITY_EDITOR
            Debug.Log("개인정보 처리방침 버튼");
#else
            CallWebView.ShowUrlFullScreen(Define.PrivacyPolicyURL, "privacyPolicyWebViewTitle");
#endif
        }

        private void ClickTermsOfServiceButton(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(37);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
#if UNITY_EDITOR
            Debug.Log("서비스 이용약관 버튼");
#else
            CallWebView.ShowUrlFullScreen(Define.TermsOfUseURL, "termsOfUseWebViewTitle");
#endif
        }

        private void ClickWithdrawalButton(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(38);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
#if UNITY_EDITOR
            Debug.Log("회원탈퇴 버튼");
#else
            CallWebView.ShowUrlFullScreen(Define.WithdrawalURL, "withdrawalText");
#endif
        }

        private void ClickExitButton(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(39);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
            var sceneName = SceneManager.GetActiveScene().name;
            Util.ExitProcess(sceneName);
        }

        async private void ShareGameLinkButton(PointerEventData data)
        {
            LogManager.Instance.InsertActionLog(34);
            AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(1));
#if UNITY_ANDROID && !UNITY_EDITOR
            var keyID = Define.CDKey.android_market_url.ToString();
#elif UNITY_IOS && !UNITY_EDITOR
            var keyID = Define.CDKey.ios_market_url.ToString();
#else
            var keyID = Define.CDKey.android_market_url.ToString();
#endif
            await CallAPI.APISelectKeyValue(UserDataManager.Instance.MySid, keyID, (obj) =>
            {
                if (obj != null)
                {
                    var url = (string)obj;
                    Util.CopyStringClipboard(url);
                    UIManager.Instance.ShowWarningUI("shareGameLinkComplete");
                }
                else
                    UIManager.Instance.ShowWarningUI("Fail ShareGameLink", false);
            });            
        }

        public void ActiveAD(bool isOn)
        {
            if (InGameManager.HasInstance)
                InGameManager.Instance.ADCallback?.Invoke(isOn);
            settingData.IsAdActive = isOn;
            Util.SaveSettingData(settingData);
        }

        public void AcceptAdOption()
        {
            var isAccept = Get<LabelToggle>((int)SettingLabelToggles.PushToggle).isOn;
            if(isAccept)
            {
                LocalPushManager.Instance.SendNotification(LocalPushManager.Instance.GetNotificationData("request_access_7days"));
            }
            else
            {
                LocalPushManager.Instance.CancelAllScheduledNotifications();
            }
            settingData.AdAccept = isAccept;
            Util.SaveSettingData(settingData);
        }

        private void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= ChangeLocaleCallback;
        }
    }
}