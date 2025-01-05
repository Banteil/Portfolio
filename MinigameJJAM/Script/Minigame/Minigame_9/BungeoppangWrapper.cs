using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class BungeoppangWrapper : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IBungeoppang
    {
        private const int LIMIT_COUNT = 8;

        [SerializeField]
        private List<Image> _cookBunList;
        [SerializeField]
        private Canvas _wrapper;
        [SerializeField]
        private Image _wrapperImage;

        private int _count = 0;
        public int Count { get { return _count; } set { _count = value; } }
        private int _order;
        private Vector3 _initialPosition;

        private event Action<PointerEventData> _onDrop, _onBeginDrag, _onDrag, _onEndDrag;

        public bool IsDrag { get; set; }
        public bool IsReady
        {
            get { return _wrapperImage.enabled; }
            set { _wrapperImage.enabled = value; }
        }

        public IBungeoppang.BungeoppangTool ToolType { get; set; }

        private void Awake()
        {
            var wrapperRect = (RectTransform)_wrapper.transform;
            _initialPosition = wrapperRect.localPosition;
            ToolType = IBungeoppang.BungeoppangTool.Wrapper;
        }

        public void Reset()
        {
            foreach (var bun in _cookBunList)
            {
                bun.enabled = false;
            }
            _count = 0;
            IsReady = false;
        }

        public void SetActionWrapper(bool active)
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

        public void OnDrop(PointerEventData eventData)
        {
            _onDrop?.Invoke(eventData);
        }

        private void DropProcess(PointerEventData eventData)
        {
            if (!IsReady) return;
            if (eventData.pointerDrag != null)
            {
                var bungeoppang = eventData.pointerDrag.GetComponent<IBungeoppang>();
                if (bungeoppang == null) return;
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
            _order = _wrapper.sortingOrder;
            _wrapper.sortingOrder = 5;
            var wrapperRect = (RectTransform)_wrapper.transform;
            wrapperRect.localScale = new Vector3(0.8f, 0.8f);
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

            var wrapperRect = (RectTransform)_wrapper.transform;
            wrapperRect.position = worldPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _onEndDrag?.Invoke(eventData);
        }

        private void EndDragProcess(PointerEventData eventData)
        {
            if (!IsDrag) return;
            _wrapper.sortingOrder = _order;

            var wrapperRect = (RectTransform)_wrapper.transform;
            wrapperRect.localPosition = _initialPosition;
            wrapperRect.localScale = Vector3.one;
            IsDrag = false;
        }
    }
}
