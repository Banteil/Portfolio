using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace starinc.io
{
    public class SoundManager : BaseManager
    {
        #region Cache
        private AudioMixer _audioMixer;
        private AudioSource _bgmSource;
        private AudioSource _oneShotSFXSource;
        private List<AudioSource> _sfxSources;
        private Transform _sfxSourcesTr;

        private Dictionary<string, AudioClip> _commonBGMClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> _commonSFXClips = new Dictionary<string, AudioClip>();

        private bool _isBGMFading;
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            LoadAudioMixer();
            InitializeAudioSources();
            LoadCommonAudioClips();
            Manager.Load.OnSceneLoadProcessStarted += ClearSoundSource;
            Manager.Game.OnLoadSettingData += VolumeInit;
        }

        private void VolumeInit(SettingData data)
        {
            SetMasterVolume(1f);
            SetBGMVolume(data != null ? data.BGMVolume : 1f);
            SetSFXVolume(data != null ? data.SFXVolume : 1f);
        }

        /// <summary>
        /// AudioMixer를 생성하고 그룹을 설정합니다.
        /// </summary>
        private void LoadAudioMixer()
        {
            if (_audioMixer == null)
            {
                _audioMixer = Resources.Load<AudioMixer>("MainAudioMixer");
                if (_audioMixer == null)
                {
                    Debug.LogError("MainAudioMixer를 찾을 수 없습니다. Resources 폴더에 MainAudioMixer가 있는지 확인하세요.");
                }
            }
        }

        /// <summary>
        /// 오디오 소스를 초기화합니다.
        /// </summary>
        private void InitializeAudioSources()
        {
            GameObject bgmObject = new GameObject("BGMSource");
            bgmObject.transform.SetParent(transform);
            _bgmSource = bgmObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("BGM")[0];

            GameObject oneShotSFXObject = new GameObject("OneShotSFXSource");
            oneShotSFXObject.transform.SetParent(transform);
            _oneShotSFXSource = oneShotSFXObject.AddComponent<AudioSource>();
            _oneShotSFXSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("SFX")[0];

            _sfxSources = new List<AudioSource>();
            GameObject sfxObjects = new GameObject("SFXSources");
            sfxObjects.transform.SetParent(transform);
            _sfxSourcesTr = sfxObjects.transform;

            gameObject.AddComponent<AudioListener>();
        }

        /// <summary>
        /// 필요한 Common 오디오 클립을 캐싱합니다.
        /// </summary>
        private void LoadCommonAudioClips()
        {
            var commonTable = Resources.Load<SoundResourceTable>("CommonSoundTable");
            if (commonTable == null)
            {
                Debug.LogError("CommonSoundTable is Null");
                return;
            }
            _commonBGMClips = commonTable.GetBGMDictionary();
            _commonSFXClips = commonTable.GetSFXDictionary();
        }

        /// <summary>
        /// 로딩 시작 할 때 현재 캐싱된 Sound소스 Clear 처리
        /// </summary>
        private void ClearSoundSource()
        {
            StopBGM();
            _bgmSource.clip = null;

            var sourcesToRemove = _sfxSources.Where(source => !source.isPlaying).ToList();
            foreach (var source in sourcesToRemove)
            {
                source.Stop();
                Destroy(source.gameObject);
                _sfxSources.Remove(source);
            }

            foreach (var source in _sfxSources)
            {
                source.DOFade(0, 0.1f).OnComplete(() =>
                {
                    source.Stop();
                    source.volume = 1;
                    source.clip = null;
                });
            }
        }

        /// <summary>
        /// 배경음을 재생합니다.
        /// </summary>
        /// <param name="bgmName">재생할 배경음의 이름.</param>
        public async void PlayBGM(string bgmName)
        {
            var sceneBGMClips = Manager.UI.SceneUI.SoundTable?.GetBGMDictionary();
            AudioClip clip;
            if (_commonBGMClips.TryGetValue(bgmName, out clip) || (sceneBGMClips != null && sceneBGMClips.TryGetValue(bgmName, out clip)))
            {
                await UniTask.WaitUntil(() => !_isBGMFading);
                _bgmSource.clip = clip;
                _bgmSource.Play();
            }
            else
            {
                Debug.LogError($"BGM '{bgmName}'을(를) 찾을 수 없습니다.");
            }
        }

        public async void PlayBGM(AudioClip clip = null)
        {
            if (clip != null)
                _bgmSource.clip = clip;

            if (_bgmSource.clip != null)
            {
                await UniTask.WaitUntil(() => !_isBGMFading);
                _bgmSource.Play();
            }
        }

        /// <summary>
        /// 배경음을 멈춥니다.
        /// </summary>
        public void StopBGM(float fadeDuration = 0)
        {
            if (_bgmSource.isPlaying)
            {
                if (fadeDuration <= 0)
                    _bgmSource.Stop();
                else
                {
                    _isBGMFading = true;
                    _bgmSource.DOFade(0, fadeDuration).OnComplete(() =>
                    {
                        _isBGMFading = false;
                        _bgmSource.Stop();
                        _bgmSource.volume = 1;
                    });
                }
            }
        }

        public void PauseBGM()
        {
            if (_bgmSource.isPlaying)
                _bgmSource.Pause();
        }

        /// <summary>
        /// 효과음을 재생합니다.
        /// </summary>
        /// <param name="sfxName">재생할 효과음의 이름.</param>
        public void PlaySFX(string sfxName, bool loop = false)
        {
            var sceneSFXClips = Manager.UI.SceneUI.SoundTable?.GetSFXDictionary();
            AudioClip clip;
            if (_commonSFXClips.TryGetValue(sfxName, out clip) || (sceneSFXClips != null && sceneSFXClips.TryGetValue(sfxName, out clip)))
            {
                AudioSource sfxSource = GetAvailableSFXSource();
                sfxSource.clip = clip;
                sfxSource.loop = loop;
                sfxSource.Play();
            }
            else
            {
                Debug.LogError($"SFX '{sfxName}'을(를) 찾을 수 없습니다.");
            }
        }

        public void PlaySFX(AudioClip clip, bool loop = false)
        {
            if (clip == null) return;
            AudioSource sfxSource = GetAvailableSFXSource();
            sfxSource.clip = clip;
            sfxSource.loop = loop;
            sfxSource.Play();
        }

        public void PlayOneShotSFX(string sfxName)
        {
            var sceneSFXClips = Manager.UI.SceneUI.SoundTable?.GetSFXDictionary();
            AudioClip clip;
            if (_commonSFXClips.TryGetValue(sfxName, out clip) || (sceneSFXClips != null && sceneSFXClips.TryGetValue(sfxName, out clip)))
            {
                _oneShotSFXSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogError($"SFX '{sfxName}'을(를) 찾을 수 없습니다.");
            }
        }

        public void PlayOneShotSFX(AudioClip clip)
        {
            if (clip == null) return;
            _oneShotSFXSource.PlayOneShot(clip);
        }

        public void StopSFX(string clipName, float fadeDuration = 0)
        {
            var source = _sfxSources.FirstOrDefault(s => s.clip != null && s.clip.name == clipName);
            if (source == null)
            {
                Debug.LogWarning($"Clip named {clipName} does not exist.");
                return;
            }

            if (source.isPlaying)
            {
                if (fadeDuration <= 0)
                    source.Stop();
                else
                {
                    source.DOFade(0, fadeDuration).OnComplete(() =>
                    {
                        source.Stop();
                        source.volume = 1;
                    });
                }
            }
        }

        public void StopAllSFX(float fadeDuration = 0)
        {
            foreach (var source in _sfxSources)
            {
                if (source.isPlaying)
                {
                    if (fadeDuration <= 0)
                        source.Stop();
                    else
                    {
                        source.DOFade(0, fadeDuration).OnComplete(() =>
                        {
                            source.Stop();
                            source.volume = 1;
                        });
                    }
                }
            }
        }

        public void PauseAllSFX()
        {
            foreach (var source in _sfxSources)
            {
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
        }

        public void ReplayAllSFX()
        {
            foreach (var source in _sfxSources)
            {
                if (!source.IsPause())
                {
                    source.Play();
                }
            }
        }

        public bool IsPlayingSFX(string name)
        {
            foreach (var source in _sfxSources)
            {
                if (source.isPlaying && source.clip != null && source.clip.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 사용할 수 있는 효과음 오디오 소스를 가져옵니다.
        /// </summary>
        /// <returns>사용할 수 있는 AudioSource.</returns>
        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in _sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // 새로운 오디오 소스를 추가합니다.
            GameObject sfxObject = new GameObject("SFXSource");
            sfxObject.transform.SetParent(_sfxSourcesTr);
            AudioSource newSource = sfxObject.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("SFX")[0];
            _sfxSources.Add(newSource);
            return newSource;
        }

        /// <summary>
        /// Master 볼륨을 설정합니다.
        /// </summary>
        /// <param name="volume">설정할 볼륨 (0~1 사이의 값).</param>
        public void SetMasterVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0.001f, 1f);
            _audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }

        /// <summary>
        /// 배경음 볼륨을 설정합니다.
        /// </summary>
        /// <param name="volume">설정할 볼륨 (0~1 사이의 값).</param>
        public void SetBGMVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0.001f, 1f);
            _audioMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
        }

        /// <summary>
        /// 효과음 볼륨을 설정합니다.
        /// </summary>
        /// <param name="volume">설정할 볼륨 (0~1 사이의 값).</param>
        public void SetSFXVolume(float volume)
        {
            volume = Mathf.Clamp(volume, 0.001f, 1f);
            _audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
    }
}