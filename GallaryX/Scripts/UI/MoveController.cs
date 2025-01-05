using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.gallaryx
{
    public class MoveController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private RectTransform _rectTr, _handle;
        private VirtualController _controller;

        private Vector2 _inputVector;
        private float _maxRadius;


        private void Awake()
        {
            _rectTr = GetComponent<RectTransform>();
            _handle = transform.GetChild(0).GetComponent<RectTransform>();
            _controller = Util.FindComponentInParents<VirtualController>(transform);
            _maxRadius = _rectTr.sizeDelta.x * 0.5f;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            UIManager.Instance.InteractUI = true;
            MoveHandle(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UIManager.Instance.InteractUI = true;
            MoveHandle(eventData);            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            UIManager.Instance.InteractUI = false;
            _handle.anchoredPosition = Vector2.zero;
            _inputVector = Vector2.zero;
            _controller.OnDragMoveHandle(_inputVector);
        }

        private void MoveHandle(PointerEventData eventData)
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

            _controller.OnDragMoveHandle(_inputVector);
        }
    }
}