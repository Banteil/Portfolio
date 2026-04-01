using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private static ItemManager instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            _table = Resources.Load<ItemDataTable>("Table/ItemTable");
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static ItemManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    ItemDataTable _table;
    public ItemDataTable Table { get { return _table; } }

    public Item GetItemSameID(string itemID)
    {
        if (itemID.Equals("null")) return null;

        Item item = new Item(GetItemInfoSameID(itemID));
        if (item.Info == null) return null;
        else return item;
    }

    public ItemInfo GetItemInfoSameID(string itemID)
    {
        if (itemID.Equals("null")) return null;

        List<ItemData> itemDatas = _table.ItemTable;
        IEnumerable<ItemInfo> itemQuery =
            from item in itemDatas
            where item.ItemInfo.ID.Equals(itemID)
            select item.ItemInfo;
        return itemQuery.FirstOrDefault();
    }

    public List<ItemInfo> GetItemInfoSameCombination(List<ItemInfo> itemList, string info, int index)
    {
        IEnumerable<ItemInfo> itemQuery;
        if (itemList == null)
        {
            List<ItemData> itemDatas = _table.ItemTable;
            itemQuery =
                from item in itemDatas
                where item.ItemInfo.CombinationInfo[index].Equals(info)
                select item.ItemInfo;
        }
        else
        {
            itemQuery =
                from item in itemList
                where item.CombinationInfo[index].Equals(info)
                select item;
        }
        return itemQuery.ToList();
    }

    /// <summary>
    /// 캐릭터 사망 시 아이템 드랍 함수  (새 아이템 생성)
    /// </summary>
    /// <param name="dropPos"></param>
    /// <param name="dropInfo"></param>
    /// <param name="maxDrop"></param>
    public void DropOfItem(Vector2 dropPos, MyItemDropInfo dropInfo, int maxDrop)
    {
        if (dropInfo == null) return;

        //아이템 드랍 퍼센테이지
        List<float> percentList = new List<float>();
        for (int i = 0; i < dropInfo.DropInfos.Count; i++)
        {
            if (i.Equals(0))
                percentList.Add(dropInfo.DropInfos[i].Percentage);
            else
                percentList.Add(dropInfo.DropInfos[i].Percentage + percentList[i - 1]);
        }

        //최대 확률
        float maxPercent = percentList[percentList.Count - 1];
        //몇개를 드랍할 지 정함
        int dropNumber = Random.Range(0, maxDrop);
        for (int i = 0; i < dropNumber; i++)
        {
            GameObject obj = null;
            if (MapManager.Instance.DropItemObjects.Count <= 0)
                obj = Instantiate(ResourceManager.Instance.DropItemObj, MapManager.Instance.DropItemTr, false);
            else
                obj = MapManager.Instance.DropItemObjects.Dequeue();

            DropItem dropItem = obj.GetComponent<DropItem>();
            obj.transform.position = dropPos;

            float rand = Random.Range(0, maxPercent - 1f);
            int num = 0;
            for (int j = 0; j < percentList.Count; j++)
            {
                Debug.Log(rand + ", " + percentList[j]);
                if (rand < percentList[j])
                {
                    num = j;
                    break;
                }
                else
                    continue;
            }
            obj.SetActive(true);
            dropItem.Item = GetItemSameID(dropInfo.DropInfos[num].ItemData.ItemInfo.ID);
        }

        int dropCoin = Random.Range(dropInfo.MinMoney, dropInfo.MaxMoney + 1);
        GoldDrop(dropPos, dropCoin);
    }

    /// <summary>
    /// 캐릭터 사망 시 돈 드랍 함수  (새 아이템 생성)
    /// </summary>
    /// <param name="dropPos"></param>
    /// <param name="dropCoin"></param>
    void GoldDrop(Vector2 dropPos, int dropCoin)
    {
        int goldNum = dropCoin / 1000;
        dropCoin %= 1000;
        int silverNum = dropCoin / 100;
        dropCoin %= 100;
        int copperNum = dropCoin;

        for (int i = 0; i < goldNum; i++)
        {
            GameObject obj = null;
            if (MapManager.Instance.DropItemObjects.Count <= 0)
                obj = Instantiate(ResourceManager.Instance.DropItemObj, MapManager.Instance.DropItemTr, false);
            else
                obj = MapManager.Instance.DropItemObjects.Dequeue();

            DropItem dropItem = obj.GetComponent<DropItem>();
            obj.transform.position = dropPos;
            obj.SetActive(true);
            dropItem.Item = GetItemSameID(ItemCode.gold);
        }

        for (int i = 0; i < silverNum; i++)
        {
            GameObject obj = null;
            if (MapManager.Instance.DropItemObjects.Count <= 0)
                obj = Instantiate(ResourceManager.Instance.DropItemObj, MapManager.Instance.DropItemTr, false);
            else
                obj = MapManager.Instance.DropItemObjects.Dequeue();

            DropItem dropItem = obj.GetComponent<DropItem>();
            obj.transform.position = dropPos;
            obj.SetActive(true);
            dropItem.Item = GetItemSameID(ItemCode.silver);
        }

        for (int i = 0; i < copperNum; i++)
        {
            GameObject obj = null;
            if (MapManager.Instance.DropItemObjects.Count <= 0)
                obj = Instantiate(ResourceManager.Instance.DropItemObj, MapManager.Instance.DropItemTr, false);
            else
                obj = MapManager.Instance.DropItemObjects.Dequeue();

            DropItem dropItem = obj.GetComponent<DropItem>();
            obj.transform.position = dropPos;
            obj.SetActive(true);
            dropItem.Item = GetItemSameID(ItemCode.copper);
        }
    }

    /// <summary>
    /// 인벤토리에서 하나 떨굴 때 사용 함수
    /// </summary>
    /// <param name="dropPos"></param>
    /// <param name="item"></param>
    public void InventoryItemDrop(Vector2 dropPos, Item item)
    {
        GameObject obj = null;
        if (MapManager.Instance.DropItemObjects.Count <= 0)
            obj = Instantiate(ResourceManager.Instance.DropItemObj, MapManager.Instance.DropItemTr, false);
        else
            obj = MapManager.Instance.DropItemObjects.Dequeue();

        DropItem dropItem = obj.GetComponent<DropItem>();
        obj.transform.position = dropPos;
        obj.SetActive(true);
        dropItem.Item = item;
    }

    public void ItemAcquisition(CharacterBasic character, Item item)
    {
        if(character == null)
        {
            Debug.Log("아이템을 받을 대상이 존재하지 않음");
            return;
        }
        if(item == null)
        {
            Debug.Log("받을 아이템 정보가 없음");
            return;
        }

        if (item.Info.ItemType.Equals(ItemType.GOODS))
        {
            character.Items.Coin += (int)item.Info.Value;
        }
        else
        {
            if (!CheckOverlapList(character.Items.Belongings, item))
            {
                if (CheckIsFull(character.Items.Belongings, item))
                    InventoryItemDrop(character.transform.position, item);
            }

            Inventory inventory = UIManager.Instance.GetUI("Inventory").GetComponent<Inventory>();
            if (character.gameObject.CompareTag("Player") && inventory.gameObject.activeSelf)
                inventory.SyncBelongingsAll();
        }
    }

    /// <summary>
    /// 인벤토리 실제 정보에서 겹치는 아이템이 있는지 체크하고, 있으면 카운트를 올리는 함수, 겹치는 여부 반환함
    /// </summary>
    /// <param name="items"></param>
    /// <param name="myItem"></param>
    /// <returns></returns>
    public bool CheckOverlapList(List<Item> items, Item myItem)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null) //i 인덱스에 아이템이 있는데
            {
                if (items[i].Info.ID.Equals(myItem.Info.ID)) //동일 아이템일 경우 겹치기 체크
                {
                    int _maxStackCount = items[i].Info.MaxStackCount;
                    int _totalCount = items[i].CurrentCount + myItem.CurrentCount;
                    int _remainingCount = _totalCount - _maxStackCount;
                    if (_remainingCount < 0) _remainingCount = 0;
                    if (_maxStackCount >= _totalCount) //최대 스택 카운트가 합산 카운트와 같거나 크면 인풋
                    {
                        items[i].CurrentCount = _totalCount;
                        myItem.CurrentCount = _remainingCount;
                        if (myItem.CurrentCount <= 0)
                            myItem = null;

                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// ItemBox 리스트에서 겹치는 아이템이 있는지 체크하고, 있으면 카운트를 올리는 함수, 겹치는 여부 반환함
    /// </summary>
    /// <param name="itemBoxes"></param>
    /// <param name="myItemBox"></param>
    /// <returns></returns>
    public bool CheckOverlapList(List<ItemBox> itemBoxes, ItemBox myItemBox)
    {
        for (int i = 0; i < itemBoxes.Count; i++)
        {
            if (itemBoxes[i].ItemObject.Item != null) //i 인덱스에 아이템이 있는데
            {
                if (itemBoxes[i].ItemObject.Item.Info.ID.Equals(myItemBox.ItemObject.Item.Info.ID)) //동일 아이템일 경우 겹치기 체크
                {
                    int _maxStackCount = itemBoxes[i].ItemObject.Item.Info.MaxStackCount;
                    int _totalCount = itemBoxes[i].ItemObject.Item.CurrentCount + myItemBox.ItemObject.Item.CurrentCount;
                    int _remainingCount = _totalCount - _maxStackCount;
                    if (_remainingCount < 0) _remainingCount = 0;
                    if (_maxStackCount >= _totalCount) //최대 스택 카운트가 합산 카운트와 같거나 크면 인풋
                    {
                        itemBoxes[i].ItemObject.Item.CurrentCount = _totalCount;
                        myItemBox.ItemObject.Item.CurrentCount = _remainingCount;
                        if (myItemBox.ItemObject.Item.CurrentCount <= 0)
                            myItemBox.ItemObject.Item = null;

                        itemBoxes[i].ItemObject.LinkedItem?.Invoke();
                        myItemBox.ItemObject.LinkedItem?.Invoke();
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 드래그 한 아이템과 클릭한 위치의 아이템의 동일 여부를 체크하고, 맞으면 카운트를 올리는 함수, 겹치는 여부 반환함
    /// </summary>
    /// <param name="dragItem"></param>
    /// <param name="boxItem"></param>
    /// <returns></returns>
    public bool CheckOverlap(ItemObject dragItem, ItemObject boxItem)
    {
        if (boxItem.Item != null)
        {
            if (boxItem.Item.Info.ID.Equals(dragItem.Item.Info.ID)) //동일 아이템일 경우 겹치기 체크
            {
                int _maxStackCount = boxItem.Item.Info.MaxStackCount;
                int _totalCount = boxItem.Item.CurrentCount + dragItem.Item.CurrentCount;
                int _remainingCount = _totalCount - _maxStackCount;
                if (_remainingCount < 0) _remainingCount = 0;
                if (_maxStackCount >= _totalCount) //최대 스택 카운트가 합산 카운트와 같거나 크면 인풋
                {
                    ItemObject startObject = dragItem.ParentBox.ItemObject; //드래그를 시작한 오브젝트
                    boxItem.Item.CurrentCount = _totalCount;
                    if(startObject.Equals(boxItem)) //다시 원래 자리에 겹치는 경우
                    {
                        dragItem.Item = null;
                        dragItem.IsSelect = false;

                        boxItem.LinkedItem?.Invoke();
                    }
                    else //아닌 경우
                    {
                        if (startObject.Item != null)
                        {
                            if (dragItem.IsIndividual)
                            {
                                dragItem.Item.CurrentCount = _remainingCount;
                                if (dragItem.Item.CurrentCount <= 0)
                                {
                                    dragItem.Item = null;
                                    dragItem.IsSelect = false;
                                }
                                boxItem.LinkedItem?.Invoke();
                            }
                            else
                            {
                                startObject.Item.CurrentCount = _remainingCount;
                                dragItem.Item.CurrentCount = _remainingCount;
                                if (startObject.Item.CurrentCount <= 0)
                                {
                                    startObject.Item = null;
                                    dragItem.Item = null;
                                    dragItem.IsSelect = false;
                                }

                                boxItem.LinkedItem?.Invoke();
                                startObject.LinkedItem?.Invoke();
                            }
                        }
                        else
                        {
                            dragItem.Item = null;
                            dragItem.IsSelect = false;

                            boxItem.LinkedItem?.Invoke();
                            startObject.LinkedItem?.Invoke();
                        }                        
                    }

                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 인벤토리에 빈공간이 있는지 체크하고, 있으면 아이템 대입
    /// </summary>
    /// <param name="items"></param>
    /// <param name="myItem"></param>
    /// <returns></returns>
    public bool CheckIsFull(List<Item> items, Item myItem)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] = myItem;
                return false;
            }
        }

        return true;
    }

    public void GetQuestReward(string itemCode, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Item rewardItem = GetItemSameID(itemCode);
            ItemAcquisition(GameManager.Instance.Player, rewardItem);
        }
    }

    Dictionary<ItemType, System.Action<Item, CharacterBasic>> _equipmentsMounting = new Dictionary<ItemType, System.Action<Item, CharacterBasic>>
    {
        { ItemType.WEAPON, (Item item, CharacterBasic character) => { character.Items.Equipments[EquipmentType.Weapon] = item; } },
        { ItemType.HALEM, (Item item, CharacterBasic character) => { character.Items.Equipments[EquipmentType.Halem] = item; } },
        { ItemType.BODYARMOR, (Item item, CharacterBasic character) => { character.Items.Equipments[EquipmentType.BodyArmor] = item; } },
        { ItemType.SHOES, (Item item, CharacterBasic character) => { character.Items.Equipments[EquipmentType.Shoes] = item; } },
        { ItemType.ACCESSORIES, (Item item, CharacterBasic character) => { character.Items.Equipments[EquipmentType.Accessories] = item; } }
    };
    public Dictionary<ItemType, System.Action<Item, CharacterBasic>> EquipmentsMounting => _equipmentsMounting;

    public ItemType GetMountingKey(ItemType type)
    {
        ItemType mountingKey = type;
        if (mountingKey > ItemType.WEAPON && mountingKey < ItemType.ARMOR)
            mountingKey = ItemType.WEAPON;
        else if (mountingKey > ItemType.COMSUMABLE && mountingKey < ItemType.RESOURCES)
            mountingKey = ItemType.COMSUMABLE;

        return mountingKey;
    }
}
