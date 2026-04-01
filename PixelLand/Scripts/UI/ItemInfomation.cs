using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ItemInfomation : MonoBehaviour
{
    [SerializeField]
    Text itemNameText;
    [SerializeField]
    Text itemValueText;
    [SerializeField]
    Text itemDescriptionText;

    public void SetItemInfo(Item item, Vector3 pos)
    {
        itemNameText.text = item.Info.DisplayName;
        itemDescriptionText.text = item.Info.Description + "\n" + item.CurrentCount + "개 소지";
        switch (item.Info.ItemType)
        {
            case ItemType.WEAPON:
                itemValueText.text = "공격력 : " + item.Info.Value;
                break;
            case ItemType.EQUIPMENT:
                itemValueText.text = "방어력 : " + item.Info.Value;
                break;
            default:
                itemValueText.text = "";
                break;
        }
        transform.position = pos;
        gameObject.SetActive(true);
    }
}
