using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectGem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static GameObject selectedGem;
    public static Transform parentSlot;
    [HideInInspector] public Gem gem;

    public abstract void OnPointerDown(PointerEventData eventData);
    public abstract void OnPointerUp(PointerEventData eventData);
    public abstract void CancleTouch();
}
