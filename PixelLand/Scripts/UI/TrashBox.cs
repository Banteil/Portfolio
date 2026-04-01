using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TrashBox : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    GameObject warningUI;
    ItemObject waitingForDeletion;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIManager.Instance.DragObject != null)
        {
            //끼고 있던 장비나 낱개 아이템을 못버리게(임시 처리)
            if (UIManager.Instance.DragObject.ParentBox.ItemBoxType.Equals(BoxType.EQUIPMENTS) || UIManager.Instance.DragObject.IsIndividual) return; 
            WarningPopup();
        }
    }

    void WarningPopup()
    {
        waitingForDeletion = UIManager.Instance.DragObject.ParentBox.ItemObject;
        waitingForDeletion.Item = UIManager.Instance.DragObject.Item;        
        
        warningUI.SetActive(true);
        UIManager.Instance.DragObject.IsSelect = false;
    }

    public void Disposal()
    {
        waitingForDeletion.Item = null;
        waitingForDeletion.LinkedItem?.Invoke();
        warningUI.SetActive(false);
    }
}
