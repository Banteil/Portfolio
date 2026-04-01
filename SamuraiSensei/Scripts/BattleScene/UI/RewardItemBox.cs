using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemBox : MonoBehaviour
{    
    [SerializeField]
    Image iconImage;
    [SerializeField]
    TextMeshProUGUI numberText;

    public void SettingItemInfo(RewardType type, int value)
    {
        switch (type)
        {
            case RewardType.GOLD:
                iconImage.sprite = ResourceManager.Instance.GoldIcon;                
                numberText.text = value.ToString();
                break;
            case RewardType.GEM:
                iconImage.sprite = ResourceManager.Instance.GemIcon;
                numberText.text = value.ToString();
                break;
            case RewardType.ITEM:
                //추가 예정
                break;
        }
        iconImage.enabled = true;
    }
}
