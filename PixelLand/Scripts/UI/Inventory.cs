using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    GameObject[] infos;
    public GameObject[] Infos { get { return infos; } }
    [SerializeField]
    Image characterImage;

    [SerializeField]
    Transform equipmentsTr;
    List<ItemBox> _equipmentBoxes = new List<ItemBox>();
    public List<ItemBox> EquipmentBoxes { get { return _equipmentBoxes; } }

    [SerializeField]
    Transform belongingsTr;
    List<ItemBox> _belongingBoxes = new List<ItemBox>();
    public List<ItemBox> BelongingBoxes { get { return _belongingBoxes; } }

    [SerializeField]
    Transform shortcutTr;
    List<ItemBox> _shortcutBoxes = new List<ItemBox>();
    public List<ItemBox> ShortcutBoxes { get { return _shortcutBoxes; } }

    [SerializeField]
    Image combinationItemImage;
    public Image CombinationItemImage { get { return combinationItemImage; } }
    [SerializeField]
    Transform combinationTr;
    List<ItemBox> combinationBoxes = new List<ItemBox>();
    public List<ItemBox> CombinationBoxes { get { return combinationBoxes; } }

    [SerializeField]
    ItemDropPanel dropPanel;
    public ItemDropPanel DropPanel { get { return dropPanel; } }

    [SerializeField]
    Toggle[] bookmarks;

    List<ItemInfo> _combinationResultList;
    public List<ItemInfo> CombinationResultList { get { return _combinationResultList; } set { _combinationResultList = value; } }

    public bool isCombination { get { return infos[1].activeSelf; } }

    public void SetInventoryInfo()
    {
        for (int i = 0; i < equipmentsTr.childCount; i++)
        {
            ItemBox box = equipmentsTr.GetChild(i).GetComponent<ItemBox>();
            box.SettingBox(i, BoxType.EQUIPMENTS, GameManager.Instance.Player.Items.Equipments);
            _equipmentBoxes.Add(box);
        }
         
        for (int i = 0; i < belongingsTr.childCount; i++)
        {
            ItemBox box = belongingsTr.GetChild(i).GetComponent<ItemBox>();
            box.SettingBox(i, BoxType.BELONGING, GameManager.Instance.Player.Items.Belongings);
            _belongingBoxes.Add(box);
        }


        for (int i = 0; i < shortcutTr.childCount; i++)
        {
            ItemBox box = shortcutTr.GetChild(i).GetComponent<ItemBox>();
            box.SettingBox(i, BoxType.SHORTCUT, GameManager.Instance.Player.Items.ShortcutItems);
            _shortcutBoxes.Add(box);
        }

        for (int i = 0; i < combinationTr.childCount; i++)
        {
            ItemBox box = combinationTr.GetChild(i).GetComponent<ItemBox>();
            box.SettingBox(i, BoxType.COMBINATION, null);
            combinationBoxes.Add(box);            
        }
    }

    private void OnEnable()
    {
        transform.SetAsLastSibling();
        SyncEquipmentsAll();
        SyncBelongingsAll();
    }

    private void OnDisable()
    {
        UIManager.Instance.GetUI("ItemInfoUI").gameObject.SetActive(false);
        if (UIManager.Instance.DragObject != null)
        {
            ItemManager.Instance.InventoryItemDrop(GameManager.Instance.Player.transform.position, UIManager.Instance.DragObject.Item);
            UIManager.Instance.DragObject.IsSelect = false;
        }
    }

    public void SyncBelongingsAll()
    {
        if (GameManager.Instance.Player == null) return;

        for (int i = 0; i < GameManager.Instance.Player.Items.Belongings.Count; i++)
        {            
            _belongingBoxes[i].ItemObject.Item = GameManager.Instance.Player.Items.Belongings[i];
        }
    }

    public void SyncEquipmentsAll()
    {
        if (GameManager.Instance.Player == null) return;

        for (int i = 0; i < GameManager.Instance.Player.Items.Equipments.Count; i++)
        {
            _equipmentBoxes[i].ItemObject.Item = GameManager.Instance.Player.Items.Equipments[i];
        }
    }

    public void SelectBookmark(int index)
    {
        if (!gameObject.activeSelf) return;

        for (int i = 0; i < infos.Length; i++)
        {
            if (i.Equals(index))
                infos[i].SetActive(true);
            else
                infos[i].SetActive(false);
        }
        SyncBelongingsAll();
    }

    public void StartCombination()
    {
        if (_combinationResultList == null || _combinationResultList.Count > 1)
        {
            UIManager.Instance.GetUI("LogInfoUI").GetComponent<LogInfo>().DisplayLogInfo("해당 조합식으로 만들 수 있는 아이템이 없습니다.");
            return;
        }

        for (int i = 0; i < _belongingBoxes.Count; i++)
        {
            if (_belongingBoxes[i].ItemObject.Item == null) continue;

            if (_belongingBoxes[i].ItemObject.Item.Info.ItemType.Equals(ItemType.RESOURCES))
            {
                _belongingBoxes[i].ItemObject.Item.CurrentCount = _belongingBoxes[i].ItemObject.Count;
                if (_belongingBoxes[i].ItemObject.Item.CurrentCount <= 0) _belongingBoxes[i].ItemObject.Item = null;
                GameManager.Instance.Player.Items.Belongings[i] = _belongingBoxes[i].ItemObject.Item;
            }
        }

        for (int i = 0; i < combinationBoxes.Count; i++)
        {
            combinationBoxes[i].ItemObject.Item = null;
        }

        Item item = new Item(_combinationResultList[0]);
        ItemManager.Instance.ItemAcquisition(GameManager.Instance.Player, item);
        _combinationResultList = null;
        combinationItemImage.sprite = null;
        combinationItemImage.enabled = false;
        SyncBelongingsAll();
    }

    void Update()
    {
        characterImage.sprite = GameManager.Instance.Player.SR.sprite;
    }
}
