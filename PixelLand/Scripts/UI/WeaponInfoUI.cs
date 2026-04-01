using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInfoUI : MonoBehaviour
{
    [SerializeField]
    Image weaponImage;
    [SerializeField]
    Text durabilityText;

    public void SetWeaponInfo(Item item)
    {
        if (item == null)
            weaponImage.enabled = false;
        else
        {
            weaponImage.enabled = true;
            weaponImage.sprite = item.Info.Sprite;
            durabilityText.text = $"{item.CurrentDurability} / {item.Info.Durability}";
        }
    }
}
