using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemCount
{
    public const int EquipmentCount = 5;
    public const int BelongingCount = 20;
    public const int ShortcutCount = 9;
}

public class CharacterItem : MonoBehaviour
{
    //장비 아이템
    List<Item> _equipments = new List<Item>(ItemCount.EquipmentCount);
    public List<Item> Equipments { get { return _equipments; } set { _equipments = value; } }

    //소지품 리스트
    List<Item> _belongings = new List<Item>(ItemCount.BelongingCount);
    public List<Item> Belongings { get { return _belongings; } }

    List<Item> _shortcuts = new List<Item>(ItemCount.ShortcutCount);
    public List<Item> ShortcutItems { get { return _shortcuts; } }

    [SerializeField]
    int coin = 0;
    public int Coin
    {
        get { return coin; }
        set
        {
            coin = value;
        }
    }

    private void Awake()
    {
        for (int i = 0; i < ItemCount.EquipmentCount; i++)
        {
            _equipments.Add(null);
        }

        for (int i = 0; i < ItemCount.BelongingCount; i++)
        {
            _belongings.Add(null);
        }

        for (int i = 0; i < ItemCount.ShortcutCount; i++)
        {
            _shortcuts.Add(null);
        }
    }

    public List<Item> GetItemsInfo(BoxType type)
    {
        switch (type)
        {
            case BoxType.EQUIPMENTS: return _equipments;
            case BoxType.BELONGING: return _belongings;
            case BoxType.SHORTCUT: return _shortcuts;
            default: return null;
        }
    }
}
