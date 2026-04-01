using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ItemObject : MonoBehaviour
{
    Image _itemImage;
    public Image ItemImage { get { return _itemImage; } set { _itemImage = value; } }
    Text _countText;
    public Text CountText { get { return _countText; } set { _countText = value; } }

    Item _item;
    public Item Item
    {
        get { return _item; }
        set
        {
            //if (!isSetting) Awake();
            _item = value;

            if (_item == null)
            {
                if (_countText != null)
                    _countText.gameObject.SetActive(false);
                _itemImage.enabled = false;                
                return;
            }

            _itemImage.sprite = _item.Info.Sprite;
            Count = _item.CurrentCount;
            _item.ChangeCount = (int count) => Count = count;

            _item.UseItem = ItemAction[_item.ConversionItemType()];
            _itemImage.enabled = true;            
        }
    }

    ItemBox parentBox;
    public ItemBox ParentBox { get { return parentBox; } set { parentBox = value; } }

    bool _isSelect;
    public bool IsSelect { get { return _isSelect; } set { _isSelect = value; } }
    bool _isIndividual;
    public bool IsIndividual { get { return _isIndividual; } set { _isIndividual = value; } }
    bool _isSetting;

    public int Count
    {
        get
        {
            int count = int.Parse(_countText.text);
            return count;
        }
        set
        {
            _countText.text = value.ToString();
            if (value < 1)
            {
                _countText.gameObject.SetActive(false);
                _itemImage.enabled = false;
            }
            else if (value.Equals(1))
            {
                _countText.gameObject.SetActive(false);
                _itemImage.enabled = true;
            }
            else
            {
                _countText.gameObject.SetActive(true);
                _itemImage.enabled = true;
            }
        }
    }

    Action _linkedItem;
    public Action LinkedItem { get { return _linkedItem; } set { _linkedItem = value; } }

    Dictionary<ItemType, Action> _itemAction;
    public Dictionary<ItemType, Action> ItemAction { get { return _itemAction; } }

    private void Awake()
    {
        ObjectSetting();
    }

    public void ObjectSetting()
    {
        if (_isSetting) return;
        _itemImage = GetComponent<Image>();
        _itemImage.preserveAspect = true;
        _itemImage.enabled = false;
        _countText = transform.GetChild(0).GetComponent<Text>();
        _countText.gameObject.SetActive(false);
        parentBox = transform.parent.GetComponent<ItemBox>();

        Inventory inventory = UIManager.Instance.GetUI("Inventory").GetComponent<Inventory>();
        //UseItem 발동 시 사용될 액션
        _itemAction = new Dictionary<ItemType, Action>
        {            
            { ItemType.EQUIPMENT , ( ) => {
                if (inventory.isCombination) return;
                if(parentBox.ItemBoxType.Equals(BoxType.EQUIPMENTS)) //장비템 해제 후 인벤토리에 넣음
                {
                    for (int i = 0; i < inventory.BelongingBoxes.Count; i++)
                    {
                        if(inventory.BelongingBoxes[i].ItemObject.Item == null)
                        {
                            inventory.BelongingBoxes[i].ItemObject.Item = _item;
                            Item = null;

                            inventory.BelongingBoxes[i].ItemObject.LinkedItem?.Invoke();
                            LinkedItem?.Invoke();
                            break;
                        }
                    }
                }
                else //장착템과 스왑
                {
                    int index = _item.ConversionEquipmentType();
                    Item tempItem = new Item(_item);
                    Item = inventory.EquipmentBoxes[index].ItemObject.Item;
                    inventory.EquipmentBoxes[index].ItemObject.Item = tempItem;

                    _linkedItem?.Invoke();
                    inventory.EquipmentBoxes[index].ItemObject.LinkedItem?.Invoke();
                }
            } },
            { ItemType.COMSUMABLE, ( ) => {
                if (inventory.isCombination) return;
                switch (_item.Info.ItemType)
                {
                    case ItemType.POTION:
                        if (GameManager.Instance.Player.HP.Equals(GameManager.Instance.Player.Info.MaxHP)) return;
                        GameManager.Instance.Player.HP += _item.Info.Value;
                        if (GameManager.Instance.Player.HP > GameManager.Instance.Player.Info.MaxHP)
                            GameManager.Instance.Player.HP = GameManager.Instance.Player.Info.MaxHP;
                        UIManager.Instance.PlayerHpFillAmount();
                        break;
                    case ItemType.ENERGYDRINK:
                        if (GameManager.Instance.Player.Stamina.Equals(GameManager.Instance.Player.Info.MaxStamina)) return;
                        GameManager.Instance.Player.Stamina += _item.Info.Value;
                        if (GameManager.Instance.Player.Stamina > GameManager.Instance.Player.Info.MaxStamina)
                            GameManager.Instance.Player.Stamina = GameManager.Instance.Player.Info.MaxStamina;
                        UIManager.Instance.PlayerStaminaFillAmount();
                        break;
                }

                _item.CurrentCount--;
                 if (_item.CurrentCount <= 0)
                    Item = null;
                Item = _item;                
            } },
            { ItemType.NULL, ( ) => { } }
        };
        _isSetting = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    /// <summary>
    /// 클릭으로 이동 시 사용 함수
    /// </summary>
    public void StartClickMove() => StartCoroutine(DragUpdate());

    IEnumerator DragUpdate()
    {
        _isSelect = true;
        while (_isSelect)
        {
            transform.position = Input.mousePosition;
            if (Input.GetMouseButtonUp(1))
            {
                CancelDrag();
                break;
            }            
            yield return null;
        }

        UIManager.Instance.DragObject = null;
        Destroy(gameObject);
    }

    public void CancelDrag()
    {
        if (_isIndividual)
            parentBox.ItemObject.Item.CurrentCount += _item.CurrentCount;
        else
            parentBox.ItemObject.Item = _item;
        parentBox.ItemObject.LinkedItem();
        _isSelect = false;
    }

    public void GetCount(ItemObject otherObject)
    {
        if (otherObject.Item == null) return;

        Item otherItem = new Item(otherObject.Item);
        otherItem.CurrentCount--;
        if (otherItem.CurrentCount <= 0)
            otherItem = null;
        otherObject.Item = otherItem;

        Item myItem = new Item(Item);
        myItem.CurrentCount++;
        Item = myItem;
    }
}
