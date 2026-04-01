using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardObject : MonoBehaviour
{
    [SerializeField]
    Image rewardImage;
    public Image RewardImage { get { return rewardImage; } set { rewardImage = value; } }
    [SerializeField]
    Text countText;
    public Text CountText { get { return countText; } set { countText = value; } }

    Item _item;

    ItemInfomation itemInfoUI;

    private void Start()
    {
        itemInfoUI = UIManager.Instance.GetUI("ItemInfoUI").GetComponent<ItemInfomation>();
    }
    public Item Item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (_item == null)
            {
                if (countText != null)
                    countText.gameObject.SetActive(false);
                rewardImage.enabled = false;
                return;
            }

            rewardImage.sprite = _item.Info.Sprite;
            if (countText != null)
            {
                if (_item.CurrentCount > 1)
                {
                    countText.gameObject.SetActive(true);
                    countText.text = _item.CurrentCount.ToString();
                }
                else
                {
                    countText.text = _item.CurrentCount.ToString();
                    countText.gameObject.SetActive(false);
                }
            }
            rewardImage.enabled = true;
        }
    }

    public int Count
    {
        get
        {
            int count = int.Parse(countText.text);
            return count;
        }
        set
        {
            countText.text = value.ToString();
            if (value < 1)
            {
                countText.gameObject.SetActive(false);
                rewardImage.enabled = false;
            }
            else if (value.Equals(1))
            {
                countText.gameObject.SetActive(false);
                rewardImage.enabled = true;
            }
            else
            {
                countText.gameObject.SetActive(true);
                rewardImage.enabled = true;
            }
        }
    }

    private void OnMouseEnter()
    {
        Debug.Log("마우스 엔터!");
        if(_item != null)
            itemInfoUI.SetItemInfo(_item, transform.position);
    }

    private void OnMouseExit()
    {
        itemInfoUI.gameObject.SetActive(false);
    }
}
