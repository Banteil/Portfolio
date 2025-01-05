using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    public class Hands : MonoBehaviour
    {
        #region Cache
        private const float MIN_KNIT_HEIGHT = 300f;
        private const float MAX_KNIT_HEIGHT = 840f;
        private const float KNIT_SIZE = 60f;

        [SerializeField]
        private RectTransform _knit;

        [SerializeField]
        private Transform _leftHand, _rightHand;
        [SerializeField]
        private List<GameObject> _leftStitches, _rightStitches;

        private Animator _animator;

        private Vector2 _originalOffsetMin, _originalOffsetMax;
        private Tween _moveTween;
        
        private bool _isLeftHandFocused = true;
        private int _activeStitchCount = 0;

        public bool IsHiding { get; private set; } = false;
        public bool IsKnitting { get; private set; } = false;
        #endregion

        #region Callback
        public event Action OnKnittingSuccess;
        public event Action OnKnittingFail;
        public event Action OnSwitchSuccess;
        public event Action OnSwitchFail;
        #endregion

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            var handsRT = (RectTransform)transform;
            _originalOffsetMin = handsRT.offsetMin;
            _originalOffsetMax = handsRT.offsetMax;

            foreach(var stitch in _leftStitches)
            {
                stitch.SetActive(false);
            }

            foreach (var stitch in _rightStitches)
            {
                stitch.SetActive(false);
            }

            _knit.sizeDelta = new Vector2(_knit.sizeDelta.x, MIN_KNIT_HEIGHT);
            SettingStitch();
        }

        public void SwitchHand()
        {
            if(_activeStitchCount > 0)
            {
                OnSwitchFail?.Invoke();
                return;
            }

            var swapHand = _isLeftHandFocused ? _rightHand : _leftHand;
            swapHand.SetAsLastSibling();
            _isLeftHandFocused = !_isLeftHandFocused;
            SettingStitch();
            OnSwitchSuccess?.Invoke();
        }

        private void SettingStitch()
        {
            var stitches = _isLeftHandFocused ? _leftStitches : _rightStitches;
            var rand = UnityEngine.Random.Range(3, stitches.Count + 1);
            for (int i = 0; i < rand; i++)
            {
                stitches[i].SetActive(true);
            }
            _activeStitchCount = rand;
        }

        public void Knitting()
        {
            if (IsKnitting) return;
            if(_activeStitchCount <= 0)
            {
                OnKnittingFail?.Invoke();
                return;
            }

            _animator.SetTrigger("Knit");
            var stitches = _isLeftHandFocused ? _leftStitches : _rightStitches;
            for (int i = stitches.Count - 1; i >= 0; i--)
            {
                if (stitches[i].activeSelf)
                {
                    stitches[i].SetActive(false);
                    break;
                }
            }
            _activeStitchCount--;            
            ResizeKnit();
            OnKnittingSuccess?.Invoke();
        }

        private void ResizeKnit()
        {           
            float targetHeight = _knit.sizeDelta.y + KNIT_SIZE;
            _knit.DOSizeDelta(new Vector2(_knit.sizeDelta.x, targetHeight), 0.3f)
                .OnComplete(() =>
                {
                    if (_knit.sizeDelta.y > MAX_KNIT_HEIGHT)
                        _knit.sizeDelta = new Vector2(_knit.sizeDelta.x, MAX_KNIT_HEIGHT);
                });
        }

        public void StartKnitting() => IsKnitting = true;
        public void EndKnitting() => IsKnitting = false;

        public void HideHands()
        {
            if (IsHiding) return;
            _moveTween?.Kill();

            Vector2 targetOffsetMin = new Vector2(_originalOffsetMin.x, _originalOffsetMin.y - 300);
            Vector2 targetOffsetMax = new Vector2(_originalOffsetMax.x, _originalOffsetMax.y - 300);
            var handsRT = (RectTransform)transform;

            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => handsRT.offsetMin, x => handsRT.offsetMin = x, targetOffsetMin, 0.1f));
            sequence.Join(DOTween.To(() => handsRT.offsetMax, x => handsRT.offsetMax = x, targetOffsetMax, 0.1f));
            sequence.OnComplete(() => IsHiding = true);
            _moveTween = sequence;
        }

        public void ShowHands()
        {
            if (!IsHiding) return;
            _moveTween?.Kill();

            IsHiding = false;
            var handsRT = (RectTransform)transform;
            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => handsRT.offsetMin, x => handsRT.offsetMin = x, _originalOffsetMin, 0.1f));
            sequence.Join(DOTween.To(() => handsRT.offsetMax, x => handsRT.offsetMax = x, _originalOffsetMax, 0.1f));

            _moveTween = sequence;
        }

        public void StopHands() => _moveTween?.Kill();
    }
}
