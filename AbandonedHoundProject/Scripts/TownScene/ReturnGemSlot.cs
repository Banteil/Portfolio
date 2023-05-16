using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ReturnGemSlot : MonoBehaviour, IDropHandler
{
    [HideInInspector]
    public Transform content;

    public abstract void OnDrop(PointerEventData eventData);
}
