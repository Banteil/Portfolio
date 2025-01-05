using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace starinc.io
{
    public class BaseEffect : MonoBehaviour
    {
        #region Cache
        protected SpriteRenderer _spriteRenderer;

        [SerializeField]
        protected List<Sprite> _sprites = new List<Sprite>();
        [SerializeField]
        protected List<FrameSFXInfo> _frameSFXInfo = new List<FrameSFXInfo>();
        [SerializeField]
        protected string _startSFX, _endSFX;
        [SerializeField]
        protected bool _sfxOneshotPlay = true;

        [SerializeField]
        protected float _frameDuration = 0.1f;
        [SerializeField]
        protected bool _startPlayback = true;

        protected int _currentFrame = 0;
        protected float _timer = 0;
        protected Coroutine _processRoutine;
        #endregion

        #region Callback
        public event Action OnPlayEffect;
        public event Action OnStopEffect;
        protected Action _onFrameProcess, _onProcess;
        #endregion

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            OnAwake();
            if (_sprites.Count <= 0)
            {
                DestroyEffect();
                return;
            }
        }

        private void Start()
        {
            OnStart();
        }

        protected virtual void OnAwake()
        {
            _onFrameProcess += ChangeSpriteProcess;
            _onFrameProcess += PlaySFXProcess;
        }

        protected virtual void OnStart()
        {
            if (_startPlayback)
                Play();
        }

        protected IEnumerator UpdateRoutine()
        {
            while (true)
            {
                _onProcess?.Invoke();
                _timer += Time.deltaTime;
                if (_timer >= _frameDuration)
                {
                    _timer -= _frameDuration;
                    _onFrameProcess?.Invoke();
                }
                yield return null;
            }
        }

        protected virtual void ChangeSpriteProcess()
        {
            _currentFrame = (_currentFrame + 1) % _sprites.Count;
            _spriteRenderer.sprite = _sprites[_currentFrame];
        }

        protected virtual void PlaySFXProcess()
        {
            if (_frameSFXInfo.Count <= 0) return;
            _frameSFXInfo.Where(sfxInfo => sfxInfo.PlayFrame == _currentFrame).ToList().ForEach(sfxInfo => Manager.Sound.PlaySFX(sfxInfo.SFXName));
            if (_sfxOneshotPlay)
            {
                _frameSFXInfo.RemoveAll(sfxInfo => sfxInfo.PlayFrame == _currentFrame);
            }
        }

        public virtual void Play()
        {
            Reset();
            if (_processRoutine != null)
            {
                StopCoroutine(_processRoutine);
                _processRoutine = null;
            }
            _processRoutine = StartCoroutine(UpdateRoutine());
            if (!string.IsNullOrEmpty(_startSFX))
                Manager.Sound.PlaySFX(_startSFX);
            OnPlayEffect?.Invoke();
        }

        public virtual void Stop()
        {
            if (_processRoutine != null)
            {
                StopCoroutine(_processRoutine);
                _processRoutine = null;
            }
            if (!string.IsNullOrEmpty(_endSFX))
                Manager.Sound.PlaySFX(_endSFX);
            OnStopEffect?.Invoke();
        }

        public virtual void DestroyEffect() => Destroy(gameObject);

        public void Reset()
        {
            _currentFrame = 0;
            _spriteRenderer.sprite = _sprites[_currentFrame];
            _timer = 0;
        }
    }

    [Serializable]
    public class FrameSFXInfo
    {
        public string SFXName;
        public int PlayFrame;
    }
}
