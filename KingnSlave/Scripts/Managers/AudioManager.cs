using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace starinc.io.kingnslave
{
    public enum AudioType
    {
        BGM,
        SFX,
    }


    public class AudioManager : Singleton<AudioManager>
    {
        private const string masterMixer = "MasterVolume";
        private const string bgmMixer = "BGMVolume";
        private const string sfxMixer = "SFXVolume";

        [SerializeField] private AudioMixer knsAudioMixer;
        private AudioSource bgmSource;
        private AudioSource sfxSource;
        private AudioListener audioListener;

        private TweenCallback stopBGMTween, playBGMTween;
        private SettingData settingData;

        protected override void OnAwake()
        {
            base.OnAwake();
            SetSourceAndListner();
        }

        private void SetSourceAndListner()
        {
            bgmSource = transform.GetChild(0).GetOrAddComponent<AudioSource>();
            sfxSource = transform.GetChild(1).GetOrAddComponent<AudioSource>();
            audioListener = transform.GetChild(2).GetOrAddComponent<AudioListener>();
        }

        private void Start()
        {
            RemoveAnotherListener();
            LoadSettingVolume();
            SceneManager.sceneLoaded += SceneLoad;
        }

        private void LoadSettingVolume()
        {
            if (PlayerPrefs.HasKey(Define.SettingDataKey))
            {
                var json = PlayerPrefs.GetString(Define.SettingDataKey);
                var settingData = Util.JsonToObject<SettingData>(json);
                knsAudioMixer.SetFloat(masterMixer, Mathf.Log10(settingData.MasterVolume) * 20);
                knsAudioMixer.SetFloat(bgmMixer, Mathf.Log10(settingData.BGMVolume) * 20);
                knsAudioMixer.SetFloat(sfxMixer, Mathf.Log10(settingData.SFXVolume) * 20);
            }
            else
            {
                knsAudioMixer.SetFloat(masterMixer, Mathf.Log10(1f) * 20);
                knsAudioMixer.SetFloat(bgmMixer, Mathf.Log10(1f) * 20);
                knsAudioMixer.SetFloat(sfxMixer, Mathf.Log10(1f) * 20);
            }
        }

        private void RemoveAnotherListener()
        {
            var listeners = FindObjectsOfType<AudioListener>();
            for (int i = 0; i < listeners.Length; i++)
            {
                var item = listeners[i];
                if (item.gameObject.Equals(audioListener.gameObject))
                    continue;

                Destroy(item);
            }
        }

        /// <summary>
        /// 효과음 재생
        /// </summary>
        /// <param name="sfxClip"></param>
        public void PlaySFX(AudioClip sfxClip)
        {
            sfxSource.PlayOneShot(sfxClip);
        }

        /// <summary>
        /// 배경 음악 재생
        /// </summary>
        /// <param name="music"></param>
        public async void PlayBGM(AudioClip bgmClip, float fadeTime = 0, bool isDelay = false)
        {
            if (stopBGMTween != null)
            {
                SetMusicVolume(settingData.BGMVolume);
                DOTween.Kill(stopBGMTween);
                stopBGMTween = null;
            }

            if (bgmSource.clip != null && bgmSource.clip.name.Equals(bgmClip.name))
            {
                bgmSource.Play();
                return;
            }

            if (fadeTime == 0)
            {
                bgmSource.clip = bgmClip;
                bgmSource.Play();
            }
            else
            {
                bgmSource.clip = bgmClip;
                if (isDelay)
                {
                    var delay = (int)(fadeTime * 1000);
                    await UniTask.Delay(delay);
                    bgmSource.Play();
                }
                else
                {                  
                    bgmSource.Play();
                    SetMusicVolume(0.001f);
                    var json = PlayerPrefs.GetString(Define.SettingDataKey);
                    var settingData = Util.JsonToObject<SettingData>(json);
                    playBGMTween = knsAudioMixer.DOSetFloat(bgmMixer, Mathf.Log10(settingData.BGMVolume) * 20, fadeTime).onComplete = (() =>
                    {
                        playBGMTween = null;
                    });
                }
            }
        }

        /// <summary>
        /// 배경 음악 중지
        /// </summary>
        public void StopMusic(float fadeTime = 0f)
        {
            if (playBGMTween != null)
            {
                DOTween.Kill(playBGMTween);
                playBGMTween = null;
            }

            if (fadeTime == 0)
            {
                bgmSource.Stop();
            }
            else
            {
                var json = PlayerPrefs.GetString(Define.SettingDataKey);
                settingData = Util.JsonToObject<SettingData>(json);
                stopBGMTween = knsAudioMixer.DOSetFloat(bgmMixer, Mathf.Log10(0.001f) * 20, fadeTime).onComplete = (() =>
                {
                    bgmSource.Stop();
                    SetMusicVolume(settingData.BGMVolume);
                    stopBGMTween = null;
                    Debug.Log("FADE BGM STOP END");
                });
            }
        }

        /// <summary>
        /// 마스터 볼륨 조절
        /// </summary>
        /// <param name="volume"></param>
        public void SetMasterVolume(float volume)
        {
            volume = AdjustVolume(volume);
            knsAudioMixer.SetFloat(masterMixer, Mathf.Log10(volume) * 20);
        }

        public float GetMasterVolume()
        {
            var data = Util.LoadSettingData();
            return data.MasterVolume;
        }

        /// <summary>
        /// 배경 음악 볼륨 조절
        /// </summary>
        /// <param name="volume"></param>
        public void SetMusicVolume(float volume)
        {
            volume = AdjustVolume(volume);
            knsAudioMixer.SetFloat(bgmMixer, Mathf.Log10(volume) * 20);
        }

        public float GetMusicVolume()
        {
            var data = Util.LoadSettingData();
            return data.BGMVolume;
        }

        /// <summary>
        /// 효과음 볼륨 조절
        /// </summary>
        /// <param name="volume"></param>
        public void SetSoundEffectVolume(float volume)
        {
            volume = AdjustVolume(volume);
            knsAudioMixer.SetFloat(sfxMixer, Mathf.Log10(volume) * 20);
        }

        public float GetSoundEffectVolume()
        {
            var data = Util.LoadSettingData();
            return data.SFXVolume;
        }

        private void SceneLoad(Scene scene, LoadSceneMode loadMode)
        {
            if (audioListener == null) return;
            RemoveAnotherListener();
        }

        private float AdjustVolume(float volume)
        {
            if (volume < 0.001f)
                volume = 0.001f;
            else if (volume > 1f)
                volume = 1f;
            return volume;
        }
    }
}