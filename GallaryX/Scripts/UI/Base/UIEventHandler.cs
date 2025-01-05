using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io
{
    public class UIEventHandler : MonoBehaviour, IPointerClickHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Action<PointerEventData> OnClickHandler = null;
        public Action<PointerEventData> OnDragHandler = null;
        public Action<PointerEventData> OnDownHandler = null;
        public Action<PointerEventData> OnUpHandler = null;

        public void OnPointerClick(PointerEventData eventData) => OnClickHandler?.Invoke(eventData);

        public void OnDrag(PointerEventData eventData) => OnDragHandler?.Invoke(eventData);

        public void OnPointerDown(PointerEventData eventData) => OnDownHandler?.Invoke(eventData);

        public void OnPointerUp(PointerEventData eventData) => OnUpHandler?.Invoke(eventData);
    }
}
