using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType { PLAYERBLE, NPC, MONSTER }

[System.Serializable]
public class CharacterInfo
{
    public int CharacterID;
    public CharacterType Type;
    public string CharacterName;
    public float MaxHP;
    public float MaxStamina;
    public float Speed;
    public float Power;
    public float Sight;
    public float AttackRange;
    public string WeaponID;
}
