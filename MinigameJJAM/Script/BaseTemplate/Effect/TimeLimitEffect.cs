using DG.Tweening;
using UnityEngine;

namespace starinc.io
{
    public class TimeLimitEffect : BaseEffect
    {
        #region Cache
        public float Limit { get; set; } = 1f;
        public float CloseTime { get; set; } = 0f;
        [SerializeField]
        protected bool _allowDuplicateObjects = false;

        private float _limitTimer = 0f;

        private Tween _closeTween;
        #endregion

        protected override void OnAwake()
        {
            _startPlayback = true;
            _onProcess += CheckTimeLimit;
            OnStopEffect += DestroyEffect;
        }

        protected override void OnStart()
        {
            CheckDuplicate();
            base.OnStart();
        }

        private void CheckDuplicate()
        {
            if (!_allowDuplicateObjects)
            {
                var objects = FindObjectsByType<TimeLimitEffect>(FindObjectsSortMode.None);
                foreach (var obj in objects)
                {
                    if (obj != this && obj.name == this.name)
                    {
                        if (_closeTween != null)
                            _closeTween.Kill();
                        Destroy(obj.gameObject);
                    }
                }
            }
        }

        private void CheckTimeLimit()
        {
            _limitTimer += Time.deltaTime;

            if (_limitTimer >= Limit)
            {
                if (_closeTween != null)
                    _closeTween.Kill();
                _onProcess -= CheckTimeLimit;
                Stop();
                return;
            }
            else if (_closeTween == null && _limitTimer >= Limit - CloseTime)
            {
                _closeTween = _spriteRenderer.DOFade(0f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
    }
}
