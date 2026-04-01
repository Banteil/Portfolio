using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDropPanel : MonoBehaviour, IPointerClickHandler, IDropHandler
{
    Image panelImage;
    public Image PanelImage { get { return panelImage; } }

    private void Awake()
    {
        panelImage = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIManager.Instance.DragObject == null) return;

        if(eventData.button.Equals(PointerEventData.InputButton.Left))
            ItemDrop();
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemDrop();
    }

    void ItemDrop()
    {
        if (UIManager.Instance.DragObject == null) return;
        else if (UIManager.Instance.GetUI("Inventory").GetComponent<Inventory>().Infos[1].activeSelf)
        {
            UIManager.Instance.DragObject.IsSelect = false;
            return;
        }

        ItemManager.Instance.InventoryItemDrop(GameManager.Instance.Player.transform.position, UIManager.Instance.DragObject.Item);

        switch (UIManager.Instance.DragObject.ParentBox.ItemBoxType)
        {
            case BoxType.EQUIPMENTS:
                ItemType key = ItemManager.Instance.GetMountingKey(UIManager.Instance.DragObject.Item.Info.ItemType);
                ItemManager.Instance.EquipmentsMounting[key](null, GameManager.Instance.Player);
                break;
            case BoxType.BELONGING:
                GameManager.Instance.Player.Items.Belongings[UIManager.Instance.DragObject.ParentBox.Index] = null;
                break;
            case BoxType.SHORTCUT:
                GameManager.Instance.Player.Items.ShortcutItems[UIManager.Instance.DragObject.ParentBox.Index] = null;
                break;
            default:
                return;
        }
        UIManager.Instance.DragObject.Item = null;
        UIManager.Instance.DragObject.IsSelect = false;
    }
}
