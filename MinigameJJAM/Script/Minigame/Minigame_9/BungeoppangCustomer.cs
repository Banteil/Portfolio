using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class BungeoppangCustomer : MonoBehaviour, IDropHandler
    {
        [SerializeField]
        private CustomerData _data;
        [SerializeField]
        private GameObject _infoBubble;
        [SerializeField]
        private TextMeshProUGUI _countText;
        [SerializeField]
        private Image _timeGauge;
        [SerializeField]
        private Image _reaction;

        public BungeoppangGame BaseGame { private get; set; }
        private Image _customerImage;
        private RectTransform _rectTr;
        private int _requestCount;
        private float _watingTimer;
        private float _maxWatingTime;

        private Vector3 _initialPosition, _disapperPosition;
        private Coroutine _orderRoutine;

        private event Action<PointerEventData> _onDrop;
        public event Action<int> OnOrderSuccess;
        public event Action OnOrderMistake;

        private void Awake()
        {
            _rectTr = (RectTransform)transform;
            _customerImage = GetComponent<Image>();

            _initialPosition = _rectTr.anchoredPosition;
            _disapperPosition = new Vector3(_initialPosition.x, _initialPosition.y - _rectTr.rect.height);
            _rectTr.anchoredPosition = _disapperPosition;
        }

        private void SettingCustomerInfo()
        {
            _customerImage.sprite = _data.GetRandomSprite();
            _requestCount = _data.GetRandomOrderCount();
            _countText.text = _requestCount.ToString();
            _maxWatingTime = _data.TimePerCount * _requestCount;
            _watingTimer = _maxWatingTime;
            _timeGauge.fillAmount = _watingTimer / _maxWatingTime;
        }

        public void Appear()
        {
            SettingCustomerInfo();
            Manager.Sound.PlaySFX("m9sfx_customer");
            _rectTr.DOAnchorPos(_initialPosition, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    if (_orderRoutine != null)
                    {
                        StopCoroutine(_orderRoutine);
                        _orderRoutine = null;
                    }
                    _orderRoutine = StartCoroutine(OrderProcess());
                });
        }

        private IEnumerator OrderProcess()
        {
            _infoBubble.SetActive(true);
            while(_watingTimer > 0)
            {
                _watingTimer -= Time.deltaTime;
                _timeGauge.fillAmount = _watingTimer / _maxWatingTime;
                yield return null;
            }

            OnOrderMistake?.Invoke();
            Disapper();
        }

        public void Disapper()
        {
            _infoBubble.SetActive(false);
            if (_orderRoutine != null)
            {
                StopCoroutine(_orderRoutine);
                _orderRoutine = null;
            }

            Sequence disapperSequence = DOTween.Sequence();
            disapperSequence.AppendInterval(1f);
            disapperSequence.AppendCallback(() =>
            {
                _reaction.enabled = false;
                Manager.Sound.PlaySFX("m9sfx_customer");
            });
            disapperSequence.Append(_rectTr.DOAnchorPos(_disapperPosition, 0.5f).SetEase(Ease.InCubic));
            disapperSequence.AppendInterval(1f);
            disapperSequence.OnComplete(() =>
            {
                if (BaseGame.IsGameOver) return;
                Appear();
            });
        }

        public void SetActionDisplay(bool active)
        {
            if (active)
            {
                _onDrop = DropProcess;
            }
            else
            {
                _onDrop = null;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            _onDrop?.Invoke(eventData);
        }

        private void DropProcess(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var bungeoppang = eventData.pointerDrag.GetComponent<IBungeoppang>();
                if (bungeoppang == null || bungeoppang.ToolType != IBungeoppang.BungeoppangTool.Wrapper) return;
                HandleDrop(bungeoppang);
            }
        }

        private void HandleDrop(IBungeoppang bungeoppang)
        {
            if (bungeoppang.IsDrag)
            {
                if (bungeoppang.Count == 0) return;
                _reaction.enabled = true;
                if (bungeoppang.Count == _requestCount)
                {
                    _reaction.sprite = BaseGame.GetSprite("customer_success");
                    _reaction.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    OnOrderSuccess?.Invoke(_requestCount);
                }
                else
                {
                    _reaction.sprite = BaseGame.GetSprite("customer_fail");
                    _reaction.transform.localScale = Vector3.one;
                    OnOrderMistake?.Invoke();
                }
                bungeoppang.Reset();
                Disapper();
            }
        }
    }
}