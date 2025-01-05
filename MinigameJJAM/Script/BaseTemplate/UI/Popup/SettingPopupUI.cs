using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace starinc.io
{
    public class SettingPopupUI : PopupUI
    {
        #region Cache
        private enum SettingSlider
        {
            BGMVolumeSlider,
            SFXVolumeSlider,
        }

        private enum SettingButton
        {
            BackButton,
            LanguageButton,
            SignInButton,
        }

        private enum SettingText
        {
            LanguageText,
            VersionText,
        }

        private SettingData _settingData;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            LoadSettingData();
        }

        protected override void BindInitialization()
        {
            Bind<Slider>(typeof(SettingSlider));
            Bind<Button>(typeof(SettingButton));
            Bind<TextMeshProUGUI>(typeof(SettingText));

            var bgmSlider = GetSlider((int)SettingSlider.BGMVolumeSlider);
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            var sfxSlider = GetSlider((int)SettingSlider.SFXVolumeSlider);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);

            var backButton = GetButton((int)SettingButton.BackButton);
            backButton.gameObject.BindEvent(OnCloseButtonClicked);
            var languageButton = GetButton((int)SettingButton.LanguageButton);
            languageButton.gameObject.BindEvent(OnLanguageButton);
            var accountButton = GetButton((int)SettingButton.SignInButton);
            accountButton.gameObject.BindEvent(OnSignInButton);

            var versionText = GetText((int)SettingText.VersionText);
            versionText.text = Application.version;

            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        private void LoadSettingData()
        {
            _settingData = Manager.Game.LoadData<SettingData>();
            var bgmSlider = GetSlider((int)SettingSlider.BGMVolumeSlider);
            bgmSlider.value = _settingData.BGMVolume;
            var sfxSlider = GetSlider((int)SettingSlider.SFXVolumeSlider);
            sfxSlider.value = _settingData.SFXVolume;

            var languageText = GetText((int)SettingText.LanguageText);
            languageText.text = LocalizationSettings.SelectedLocale.LocaleName; 
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnDestroy()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }

        #region BindEvnet
        private void SetBGMVolume(float volume)
        {
            Manager.Sound.SetBGMVolume(volume);
            _settingData.BGMVolume = volume;
            Manager.Game.SaveData(_settingData);
        }

        private void SetSFXVolume(float volume)
        {
            Manager.Sound.SetSFXVolume(volume);
            _settingData.SFXVolume = volume;
            Manager.Game.SaveData(_settingData);
        }

        private void OnLanguageButton(PointerEventData data)
        {
            var languageUI = Manager.UI.ShowPopupUI<SelectLanguagePopupUI>();
            languageUI.SettingData = _settingData;
        }

        private void OnSignInButton(PointerEventData data)
        {
            var message = Util.GetLocalizedString(Define.LOCALIZATION_TABLE_MESSAGE, "inPreparation");
            Manager.UI.ShowMessage(message);
            //Manager.UI.ShowPopupUI<SelectSignInPopupUI>();
        }
        #endregion

        private void OnLocaleChanged(Locale newLocale)
        {
            var languageText = GetText((int)SettingText.LanguageText);
            languageText.text = LocalizationSettings.SelectedLocale.LocaleName;
        }
    }
}
