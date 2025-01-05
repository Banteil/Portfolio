using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io
{
    public class MovePad : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region Cache
        private RectTransform _rectTr, _handle;
        private VirtualController _controller;

        private Vector2 _inputVector;
        private float _maxRadius;

        private bool _movementPermission;
        public bool IsActivated
        {
            get { return _controller != null && _movementPermission; }
            set { _movementPermission = value; }
        }

        private Coroutine _dragCoroutine;
        #endregion

        private void Awake()
        {
            _rectTr = GetComponent<RectTransform>();
            _handle = transform.GetChild(0).GetComponent<RectTransform>();
            _controller = Util.FindComponentInParents<VirtualController>(transform);
            _maxRadius = _rectTr.sizeDelta.x * 0.5f;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsActivated)
            {
                MoveHandleProcess(eventData);
                if (_dragCoroutine != null)
                    StopCoroutine(_dragCoroutine);
                _dragCoroutine = StartCoroutine(UpdateInput());
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsActivated) MoveHandleProcess(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (IsActivated)
            {
                Reset();
            }
        }

        private IEnumerator UpdateInput()
        {
            while (true)
            {
                _controller.MoveHandle(_inputVector);
                yield return null;
            }
        }

        private void MoveHandleProcess(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTr, eventData.position, eventData.pressEventCamera, out localPoint);

            Vector2 direction = localPoint;
            if (direction.magnitude > _maxRadius)
            {
                direction = direction.normalized * _maxRadius;
            }

            _handle.anchoredPosition = direction;
            _inputVector = direction / _maxRadius;
        }

        public void Reset()
        {
            _handle.anchoredPosition = Vector2.zero;
            _inputVector = Vector2.zero;
            _controller.MoveHandle(_inputVector);

            if (_dragCoroutine != null)
            {
                StopCoroutine(_dragCoroutine);
                _dragCoroutine = null;
            }
        }
    }
}
