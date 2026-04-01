using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterInfo
{
    public int id;
    public string objectName;
    public ElementalProperties propertie;
    public float maxHP;
    public float damage;
    public float attackSpeed;
    public float attackRange;
    public int criticalChance;
    public int criticalDamage;
}
