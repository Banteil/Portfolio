using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipItemUI : MonoBehaviour
{
    public Image ItemImage;
    public Text ItemNameText;

    private void Start()
    {
        ItemHandleAbility itemHandleAbility = GameManager.Instance.PlayerCharacter.GetAbility<ItemHandleAbility>();
        itemHandleAbility.OnHandleCallback += SetItemData;
    }

    public void SetItemData(ItemData itemData)
    {
        ItemImage.sprite = itemData.ItemSprite;
        ItemNameText.text = itemData.ItemName;
    }
}
