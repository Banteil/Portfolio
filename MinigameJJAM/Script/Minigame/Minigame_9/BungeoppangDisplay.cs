using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class BungeoppangDisplay : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IBungeoppang
    {
        private const int LIMIT_COUNT = 8;

        [SerializeField]
        private List<Image> _cookBunList;
        private Image _currentDragImage;

        private int _count = 0;
        public int Count { get { return _count; } set { _count = value; } }
        private int _order;
        private Vector3 _initialPosition;

        public bool IsDrag { get; set; }
        private event Action<PointerEventData> _onDrop, _onBeginDrag, _onDrag, _onEndDrag;

        public IBungeoppang.BungeoppangTool ToolType { get; set; }

        private void Awake()
        {
            ToolType = IBungeoppang.BungeoppangTool.Display;
        }

        public void SetActionDisplay(bool active)
        {
            if (active)
            {
                _onDrop = DropProcess;
                _onBeginDrag = BeginDragProcess;
                _onDrag = DragProcess;
                _onEndDrag = EndDragProcess;
            }
            else
            {
                _onDrop = null;
                _onBeginDrag = null;
                _onDrag = null;
                _onEndDrag = null;
            }
        }

        public Sprite DragSprite() => _currentDragImage.sprite;

        public void Reset()
        {
            _currentDragImage.enabled = false;
            _count--;
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
                if (bungeoppang == null || bungeoppang.ToolType == IBungeoppang.BungeoppangTool.Wrapper) return;
                HandleDrop(bungeoppang);
            }
        }

        private void HandleDrop(IBungeoppang bungeoppang)
        {
            if (IsDrag) return;
            if (bungeoppang.IsDrag)
            {
                if (_count >= LIMIT_COUNT) return;
                Manager.Sound.PlaySFX("m9sfx_bunDrop");
                _cookBunList[_count].enabled = true;
                bungeoppang.Reset();
                _count++;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _onBeginDrag?.Invoke(eventData);
        }

        private void BeginDragProcess(PointerEventData eventData)
        {
            if (_count <= 0) return;
            Manager.Sound.PlaySFX("m9sfx_bunClick");
            IsDrag = true;
            _currentDragImage = _cookBunList[_count - 1];
            var dragCanvas = _currentDragImage.GetComponent<Canvas>();
            _order = dragCanvas.sortingOrder;
            dragCanvas.sortingOrder = 5;
            _initialPosition = _currentDragImage.rectTransform.localPosition;
            _currentDragImage.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _onDrag?.Invoke(eventData);
        }

        private void DragProcess(PointerEventData eventData)
        {
            if (!IsDrag) return;
            Vector3 screenPosition = eventData.position;
            screenPosition.z = Camera.main.nearClipPlane;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            _currentDragImage.rectTransform.position = worldPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _onEndDrag?.Invoke(eventData);
        }

        private void EndDragProcess(PointerEventData eventData)
        {
            if (!IsDrag) return;
            var dragCanvas = _currentDragImage.GetComponent<Canvas>();
            dragCanvas.sortingOrder = _order;
            _currentDragImage.rectTransform.localPosition = _initialPosition;
            _currentDragImage.rectTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
            _currentDragImage = null;
            IsDrag = false;
        }
    }
}