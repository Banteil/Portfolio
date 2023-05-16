using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int actValue;

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData)
    {
        BattleManager.Instance.SelectAct(actValue);
    }
}
