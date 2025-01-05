using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventHandler : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler
{
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public Action<PointerEventData> OnEndDragHandler = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Input.touchCount > 1) return;
        OnClickHandler?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData) => OnDragHandler?.Invoke(eventData);

    public void OnEndDrag(PointerEventData eventData) => OnEndDragHandler?.Invoke(eventData);
}
