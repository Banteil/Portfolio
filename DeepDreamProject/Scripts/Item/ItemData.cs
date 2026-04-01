using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Data/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Base Info")]
    public string ID;
    public string ItemName;
    public Sprite ItemSprite;
    public string ShortDescription;
    [TextArea]
    public string Description;
    public GameObject ItemPrefab;

    [Header("Stat Info")]
    public float Durability;
    public float ThrowDamage;
    public float ThrowPower;
    public float AttackDamage;
    public float AttackSpeed;

    [Header("Special Info")]
    public bool IsUnbreakable;
    public bool IsBind;
}
