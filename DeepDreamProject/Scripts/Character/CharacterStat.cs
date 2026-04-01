using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterStat : MonoBehaviour
{
    protected Character _character;

    [SerializeField]
    protected int _vitality;
    public Stat VitalityStat;
    [SerializeField]
    protected int _strength;
    public Stat StrengthStat;
    [SerializeField]
    protected int _agility;
    public Stat AgilityStat;

    bool _settingComplete;

    public float MoveSpeed
    {
        get
        {
            float value = AgilityStat.Value;
            return value;
        }
    }

    protected void Awake()
    {
        _character = GetComponent<Character>();
        VitalityStat = new Stat(_vitality);
        StrengthStat = new Stat(_strength);
        AgilityStat = new Stat(_agility);
        _settingComplete = true;
    }

    void OnValidate()
    {
        if (!_settingComplete) return;
        VitalityStat.BaseValue = _vitality;
        StrengthStat.BaseValue = _strength;
        AgilityStat.BaseValue = _agility;
    }
}
