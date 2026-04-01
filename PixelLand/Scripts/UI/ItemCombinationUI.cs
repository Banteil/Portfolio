using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCombinationUI : MonoBehaviour
{
    private void OnEnable()
    {
        if (UIManager.Instance.DragObject != null)
            UIManager.Instance.DragObject.CancelDrag();
        UIManager.Instance.GetUI("ShortcutFrame").SetActive(false);
    }

    private void OnDisable()
    {
        Inventory inventory = UIManager.Instance.GetUI("Inventory").GetComponent<Inventory>();
        if (UIManager.Instance.DragObject != null)
        {
            for (int i = 0; i < inventory.BelongingBoxes.Count; i++)
            {
                if (!ItemManager.Instance.CheckOverlap(UIManager.Instance.DragObject, inventory.BelongingBoxes[i].ItemObject))
                {
                    if(inventory.BelongingBoxes[i].ItemObject.Item == null)
                    {
                        inventory.BelongingBoxes[i].ItemObject.Item = UIManager.Instance.DragObject.Item;
                        UIManager.Instance.DragObject.IsSelect = false;
                        break;
                    }
                }
            }            
        }
        
        for (int i = 0; i < inventory.CombinationBoxes.Count; i++)
        {
            if(inventory.CombinationBoxes[i].ItemObject.Item != null)
            {
                for (int j = 0; j < inventory.BelongingBoxes.Count; j++)
                {
                    if (ItemManager.Instance.CheckOverlap(inventory.CombinationBoxes[i].ItemObject, inventory.BelongingBoxes[j].ItemObject)) break;
                    else if (inventory.BelongingBoxes[j].ItemObject.Item == null)
                    {
                        inventory.BelongingBoxes[j].ItemObject.Item = inventory.CombinationBoxes[i].ItemObject.Item;
                        break;
                    }
                }
            }
        }
        inventory.SyncBelongingsAll();
        UIManager.Instance.GetUI("ShortcutFrame").SetActive(true);
    }
}
