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

            // Pivot�� ����� ��ǥ ��ġ ���
            var targetRightPosition = rectTr.localPosition.x + width;
            var targetOriginalPosition = rectTr.localPosition.x;

            Sequence resetSequence = DOTween.Sequence();
            resetSequence.Append(rectTr.DOLocalMoveX(targetRightPosition, 0.5f)); // ������ �̵�
            resetSequence.AppendCallback(() =>
            {
                Manager.Sound.PlaySFX("m9sfx_wrapper");
                _count = _wrappersInTray.Count;
                foreach (var wrapper in _wrappersInTray)
                {
                    wrapper.enabled = true; // ��� wrapper Ȱ��ȭ
                }
            });
            resetSequence.AppendInterval(1f);
            resetSequence.Append(rectTr.DOLocalMoveX(targetOriginalPosition, 0.5f)); // ���� ��ġ�� ����
            resetSequence.OnComplete(() =>
            {
                _isResseting = false;
            });
        }

    }
}
