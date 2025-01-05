using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class BungeoppangWrapperTray : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private List<Image> _wrappersInTray;
        [SerializeField]
        private BungeoppangWrapper _wrapper;

        private int _count;
        private bool _isResseting = false;

        private void Awake()
        {
            _count = _wrappersInTray.Count;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_count == 0) return;

            if (!_wrapper.IsReady)
            {
                Manager.Sound.PlaySFX("m9sfx_wrapper");
                _wrapper.IsReady = true;
                _count--;
                _wrappersInTray[_count].enabled = false;
            }
        }

        public void Reset()
        {
            if (_isResseting || _count == _wrappersInTray.Count) return;
            _isResseting = true;

            var rectTr = (RectTransform)transform;
            var width = rectTr.rect.width;

            // Pivot을 고려한 목표 위치 계산
            var targetRightPosition = rectTr.localPosition.x + width;
            var targetOriginalPosition = rectTr.localPosition.x;

            Sequence resetSequence = DOTween.Sequence();
            resetSequence.Append(rectTr.DOLocalMoveX(targetRightPosition, 0.5f)); // 오른쪽 이동
            resetSequence.AppendCallback(() =>
            {
                Manager.Sound.PlaySFX("m9sfx_wrapper");
                _count = _wrappersInTray.Count;
                foreach (var wrapper in _wrappersInTray)
                {
                    wrapper.enabled = true; // 모든 wrapper 활성화
                }
            });
            resetSequence.AppendInterval(1f);
            resetSequence.Append(rectTr.DOLocalMoveX(targetOriginalPosition, 0.5f)); // 원래 위치로 복귀
            resetSequence.OnComplete(() =>
            {
                _isResseting = false;
            });
        }

    }
}
