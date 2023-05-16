using DG.Tweening;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Zeus
{
    [System.Serializable]
    public class SoundSettingData
    {
        public bool PlayBGM { get; set; }
        public bool PlayFX { get; set; }
    }

    public class SoundManager : UnitySingleton<SoundManager>
    {
        struct EffectSoundData
        {
            public AudioSource AudioSource;
            public float Duration;
        }

        public AudioSource BGMPlayer;
        public AudioSource EffectPlayer;
        public AudioListener AudioListener;

        private List<EffectSoundData> _effectSounds = new List<EffectSoundData>();
        private List<EffectSoundData> _effectExceptionSounds = new List<EffectSoundData>();

        private float _oldBGMVolume;
        private SoundSettingData _soundSettingData;
        private Transform _followTarget;

        protected override void _OnAwake()
        {
            RemoveAnotherListener();

            SceneManager.sceneLoaded += SceneLoad;
            _oldBGMVolume = BGMPlayer.volume;
        }

        private void RemoveAnotherListener()
        {
            var listeners = FindObjectsOfType<AudioListener>();
            for (int i = 0; i < listeners.Length; i++)
            {
                var item = listeners[i];
                if (item.gameObject.Equals(AudioListener.gameObject))
                    continue;

                Destroy(item);
            }

            if (Camera.main != null && Camera.main.transform != null)
                _followTarget = Camera.main.transform;
        }

        internal void SetAudioListenerFollowTarget(Transform target)
        {
            if (target == null)
            {
                _followTarget = null;
                AudioListener.transform.localPosition = Vector3.zero;
            }
            _followTarget = target;
        }

        private void SceneLoad(Scene scene, LoadSceneMode loadMode)
        {
            RemoveAnotherListener();
        }

        public void SoundSettingDatatLoad()
        {
            if (_soundSettingData != null)
                return;

            var jsonString = PlayerPrefs.GetString("SoundSetting");
            if (string.IsNullOrEmpty(jsonString))
            {
                _soundSettingData = new SoundSettingData();
                _soundSettingData.PlayBGM = true;
                _soundSettingData.PlayFX = true;
            }
            else
                _soundSettingData = JsonConvert.DeserializeObject<SoundSettingData>(jsonString);
        }

        public void SoundSettingSave()
        {
            var jsonString = JsonConvert.SerializeObject(_soundSettingData);
            PlayerPrefs.SetString("SoundSetting", jsonString);
        }

        internal int Play(string assetName, Vector3 position, bool _bgm = false)
        {
            var audioClip = TableManager.GetAudioClip(assetName);
            if (audioClip == null)
                return 0;

            if (_bgm)
                BGMChange(audioClip, 1f);
            else
               return PlayEffect(audioClip, position);

            return 0;
        }

        public int Play(int _tableID, Vector3 playPosition, bool _bgm = false, bool random = false, bool _exception = false)
        {
            if (_tableID == 0)
                return 0;

            var clip = TableManager.GetAudioClip(_tableID, random);
            if (_bgm)
            {
                BGMChange(clip);
            }
            else
            {
                return PlayEffect(clip, playPosition, _exception);                
            }
            return 0;
        }

        public int Play(int _tableID, bool _bgm = false, bool _exception = false)
        {
            if (_tableID == 0)
                return 0;

            var clip = TableManager.GetAudioClip(_tableID);
            if (clip == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"Not Found Audio Clip Table ID : {_tableID}");
#endif
                return 0;
            }
            if (_bgm)
            {
                BGMChange(clip);
            }
            else
            {
                return PlayEffect(clip, _exception);                
            }

            return 0;
        }

        public void PlayAsync(int _tableID, Vector3 position, bool _bgm = false, bool _exception = false)
        {
            if (_tableID == 0)
                return;

            TableManager.Instance.GetAudioClipAsync(_tableID, result =>
            {
                if (result == null)
                    return;

                if (_bgm)
                {
                    BGMChange(result);
                }
                else
                {
                    PlayEffect(result, position, _exception);
                }
            });
        }

        public void BGMChange(AudioClip _clip, float fadeTime = 0)
        {
            BGMPlayer.volume = _oldBGMVolume;

            if (BGMPlayer.clip != null && BGMPlayer.clip.name.Equals(_clip.name))
            {
                BGMPlayer.Play();
                return;
            }

            if (_soundSettingData != null && !_soundSettingData.PlayBGM)
            {
                BGMPlayer.Stop();
                BGMPlayer.clip = _clip;
                return;
            }

            if (fadeTime == 0)
            {
                BGMPlayer.clip = _clip;
                BGMPlayer.Play();
            }
            else
            {
                BGMPlayer.DOFade(0, fadeTime).onComplete = (() =>
                {
                    BGMPlayer.clip = _clip;
                    BGMPlayer.DOFade(_oldBGMVolume, fadeTime).onComplete = (() =>
                    {
                        BGMPlayer.Play();
                    });
                });
            }
        }

        internal void BGMFade(bool fadeIn, float fadeTime, UnityAction callback = null)
        {
            var destVolume = fadeIn ? 0 : _oldBGMVolume;
            BGMPlayer.DOFade(destVolume, fadeTime).onComplete = (() =>
            {
                callback?.Invoke();
            });
        }

        public int PlayEffect(AudioClip _audioClip, Transform parent, bool _exception = false)
        {
            if (_audioClip == null)
                return 0;

            if (EffectPlayer == null)
                return 0;

            if (_soundSettingData != null && !_soundSettingData.PlayFX)
                return 0;

            if (parent == null)
                return 0;

            var player = AddSoundData(_audioClip, _exception);

            player.AudioSource.transform.SetParent(parent);
            player.AudioSource.transform.localPosition = Vector3.zero;
            player.AudioSource.PlayOneShot(_audioClip);
            return player.AudioSource.GetInstanceID();
        }

        public int PlayEffect(AudioClip _audioClip, Vector3 playPosition, bool _exception = false)
        {
            if (_audioClip == null)
                return 0;

            if (EffectPlayer == null)
                return 0;

            if (_soundSettingData != null && !_soundSettingData.PlayFX)
                return 0;

            var player = AddSoundData(_audioClip, _exception);
            player.AudioSource.transform.SetParent(transform, false);
            player.AudioSource.transform.position = playPosition;
            player.AudioSource.PlayOneShot(_audioClip);
            return player.AudioSource.GetInstanceID();
        }

        public int PlayEffect(AudioClip _audioClip, bool _exception = false)
        {
            if (_audioClip == null)
                return 0;

            if (EffectPlayer == null)
                return 0;

            if (_soundSettingData != null && !_soundSettingData.PlayFX)
                return 0;

            var player = AddSoundData(_audioClip, _exception);
            player.AudioSource.transform.SetParent(transform);
            player.AudioSource.PlayOneShot(_audioClip);
            return player.AudioSource.GetInstanceID();
        }

        public void StopEffect(int id)
        {
            var sound = _effectSounds.Find((x) => x.AudioSource.GetInstanceID() == id);
            if(sound.AudioSource != null)
            {
                sound.AudioSource.Stop();
            }
        }

        private EffectSoundData AddSoundData(AudioClip clip, bool exception)
        {
            var player = new EffectSoundData()
            {
                AudioSource = Instantiate(EffectPlayer),
                Duration = clip.length,
            };

            if (exception)
            {
                _effectExceptionSounds.Add(player);
            }
            else
            {
                if (_effectSounds.Count >= 19)
                {
                    var item = _effectSounds[0];
                    item.AudioSource.Stop();
                    Destroy(item.AudioSource.gameObject);
                    _effectSounds.Remove(item);
                }

                _effectSounds.Add(player);
            }

            return player;
        }

        private void Update()
        {
            if (_followTarget != null && AudioListener != null)
                AudioListener.transform.SetPositionAndRotation(_followTarget.position, Quaternion.identity);

            for (int i = _effectSounds.Count - 1; i >= 0; --i)
            {
                if (_effectSounds[i].AudioSource == null || !_effectSounds[i].AudioSource.isPlaying)
                {
                    Destroy(_effectSounds[i].AudioSource.gameObject);
                    _effectSounds.RemoveAt(i);
                }
            };

            for (int i = _effectExceptionSounds.Count - 1; i >= 0; --i)
            {
                var item = _effectExceptionSounds[i];
                if (item.AudioSource == null || !item.AudioSource.isPlaying)
                {
                    Destroy(item.AudioSource.gameObject);
                    _effectExceptionSounds.RemoveAt(i);
                }
            }
        }

        internal int ConvertHitMaterialToSoundTableID(TypeHitMaterial typeHitMaterial)
        {
            var soundTableID = 0;
            switch (typeHitMaterial)
            {
                case TypeHitMaterial.NONE:
                    break;
                case TypeHitMaterial.METAL:

                    break;
                case TypeHitMaterial.WOOD:
                    break;
                case TypeHitMaterial.STONE:
                    {
                        soundTableID = (int)TypeFootStepSound.GRAVEL;
                    }
                    break;
                case TypeHitMaterial.WATER:
                    break;
                default:
                    {
                        Debug.LogError("Not Found Type");
                    }
                    break;
            }

            return soundTableID;
        }
    }
}
