using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemCode
{
    public const string copper = "G0000001";
    public const string silver = "G0000002";
    public const string gold = "G0000003";
}

public static class EquipmentType
{
    public const int Weapon = 0;
    public const int Halem = 1;
    public const int BodyArmor = 2;
    public const int Shoes = 3;
    public const int Accessories = 4;
}

public class Item
{
    public Item() { }
    public Item(ItemInfo info) { Info = info; }
    public Item(Item otherItem) 
    { 
        _info = otherItem.Info;
        _currentCount = otherItem.CurrentCount;
        _currentDurability = otherItem.CurrentDurability;
        UseItem = otherItem.UseItem;
    }

    ItemInfo _info;
    public ItemInfo Info
    {
        get { return _info; }
        set
        {
            _info = value;
            if (_info == null) return;
            _currentCount = 1;
            _currentDurability = _info.Durability;
        }
    }

    int _currentCount;
    public int CurrentCount 
    { 
        get { return _currentCount; } 
        set 
        { 
            _currentCount = value;
            if(ChangeCount != null) ChangeCount(_currentCount);
        } 
    }

    float _currentDurability;
    public float CurrentDurability { get { return _currentDurability; } set { _currentDurability = value; } }

    public Action UseItem;
    public Action<int> ChangeCount;

    public ItemType ConversionItemType()
    {
        if (_info.ItemType >= ItemType.GOODS && _info.ItemType < ItemType.EQUIPMENT)
            return ItemType.GOODS;
        else if(_info.ItemType > ItemType.EQUIPMENT && _info.ItemType < ItemType.COMSUMABLE)
            return ItemType.EQUIPMENT;
        else if (_info.ItemType > ItemType.COMSUMABLE && _info.ItemType < ItemType.RESOURCES)
            return ItemType.COMSUMABLE;
        else if(_info.ItemType > ItemType.RESOURCES && _info.ItemType < ItemType.PETS)
            return ItemType.RESOURCES;

        return ItemType.NULL;
    }
    public int ConversionEquipmentType()
    {
        if (_info.ItemType > ItemType.WEAPON && _info.ItemType < ItemType.ARMOR)
            return EquipmentType.Weapon;
        else if (_info.ItemType >= ItemType.HALEM && _info.ItemType <= ItemType.HALEM)
            return EquipmentType.Halem;
        else if (_info.ItemType >= ItemType.BODYARMOR && _info.ItemType <= ItemType.BODYARMOR)
            return EquipmentType.BodyArmor;
        else if (_info.ItemType >= ItemType.SHOES && _info.ItemType <= ItemType.SHOES)
            return EquipmentType.Shoes;
        else if (_info.ItemType >= ItemType.ACCESSORIES && _info.ItemType <= ItemType.ACCESSORIES)
            return EquipmentType.Accessories;

        return -1;
    }
}

//Dictionary<ItemType, Action<CharacterBasic, >

// id
// 이름
// 설명
// 이미지
// 착용가능여부
// 착용부위
// 드랍여부 (버리기)
// stack여부
// 판매여부
// 가격

public enum ElementalProperties
{
    NONE,
    WOOD,
    FIRE,
    EARTH,
    IRON,
    WATER,
}

public enum ItemType
{
    [InspectorName("재화")]
    GOODS = 0,

    [InspectorName("==착용==")]
    EQUIPMENT = 1000,

    [InspectorName("<<무기>>")]
    WEAPON = 1001,
    [InspectorName("한손 검")]
    SWORD_ONEHAND = 1002,
    [InspectorName("한손 창")]
    SPEAR_ONEHAND = 1003,
    [InspectorName("한손 도끼")]
    AXE_ONEHAND = 1004,

    [InspectorName("<<방어구>>")]
    ARMOR = 1100,
    [InspectorName("머리")]
    HALEM = 1101,
    [InspectorName("갑옷")]
    BODYARMOR = 1102,
    [InspectorName("신발")]
    SHOES = 1103,

    [InspectorName("<<악세사리>>")]
    ACCESSORIES = 1200,
    [InspectorName("목걸이")]
    NECKLACE = 1201,

    [InspectorName("코스튬")]
    COSTUME = 1300,

    [InspectorName("==소모성==")]
    COMSUMABLE = 2000,
    [InspectorName("포션")]
    POTION = 2001,
    [InspectorName("에너지 음료")]
    ENERGYDRINK = 2002,

    [InspectorName("재료")]
    RESOURCES = 3000,

    [InspectorName("펫")]
    PETS = 4000,

    [InspectorName("Null")]
    NULL = 99999,
}

//public enum Danny_ItemType
//{
//    NONE,
//    WEAPON,             // 무기
//    ARMOR,              // 방어구
//    ACCESSORIES,        // 악세사리
//    COSTUME,            // 코스튬
//}

//public enum Danny_ComsumableType
//{
//    POTION,             // 포션
//}

//public enum Danny_ItemQuality
//{
//    [InspectorName("없음")]
//    NONE,
//    [InspectorName("화이트")]
//    WHITE,
//    [InspectorName("파랑")]
//    BLUE,
//    [InspectorName("초록")]
//    GREEN,
//    [InspectorName("보라")]
//    PURPLE,
//    [InspectorName("빨강")]
//    RED,
//    [InspectorName("오렌지")]
//    ORANGE,
//}

[Serializable]
public class ItemInfo
{
    public string ID; //고유ID
    public string DisplayName; //표기 이름
    public Sprite Sprite; //스프라이트
    public string Description; //아이템 설명
    public ItemType ItemType; //아이템 타입
    public ElementalProperties Elemental; //아이템 속성
    public int Quality; //아이템 퀄리티(0 ~ n)
    public float Durability; //내구도(최대)
    public float Value; //고유 수치(공격력, 
    public int MaxStackCount; //최대 개수
    public float Cost; //스태미나 등 사용 시 코스트
    public string[] CombinationInfo = new string[9]; //조합식 정보(아이템 ID저장)

    //public bool IsStackable;
    //public bool IsWearable;
    public bool IsRepairable; //수리 가능 여부
    public bool IsSellable; //판매 가능 여부
    public bool IsDroppable = true; //인벤토리에서 드랍 가능 여부

    public Skill ItemSkill;
}


[Serializable]
public class ShopItemInfo
{
    //public string ID;
    public List<ItemData> SellItem;
    public List<ItemData> BuyItem;
}

//[System.Serializable]
//public class ItemInfo
//{
//    //public string ID;
//    //public string Name;
//    //public string Description;
//    //public Sprite Sprite;

//    public Danny_ItemType ItemType;
//    public Danny_ItemQuality Quality;
//    public int MaxStackCount;

//    public virtual bool IsStackable { get; protected set; }
//    public virtual float Durability { get; protected set; }
//    public virtual bool IsRepairable { get; protected set; }
//    public virtual bool IsWearable { get; protected set; }
//    public virtual bool IsDroppable { get; protected set; }
//    public virtual bool IsSellable { get; protected set; }

//    // 조합식 없으면 조합 불가능
//    public bool IsCombinable;
//}

//[System.Serializable]
//public class GoodsInfo : ItemInfo
//{

//}

//[System.Serializable]
//public class EquipmentItemInfo : ItemInfo
//{
//    public Danny_ItemType EquipType;
//}

//[System.Serializable]
//public class ConsumableItemInfo : ItemInfo
//{

//}

//[System.Serializable]
//public class ResourcesItemInfo : ItemInfo
//{

//}

//[System.Serializable]
//public class PetInfo : ItemInfo
//{
//    public Danny_ItemType EquipType;
//}