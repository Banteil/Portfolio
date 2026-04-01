using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat
{
    private float _baseValue;
    private float _value;
    private bool _isDirty = false;
    private readonly List<StatModifier> _modifiers;

    public float BaseValue 
    { 
        get { return _baseValue; } 
        set 
        { 
            _baseValue = value;
            if (_baseValue < 1) _baseValue = 1;
            _isDirty = true; 
        } 
    }
    public float Value
    {
        get
        {
            if (_isDirty)
            {
                _value = CalculateModifier();
                _isDirty = false;
            }
            return _value;
        }
    }

    public static float operator +(Stat p1, float p2)
    {
        float _operStat = p1.Value + p2;
        return _operStat;
    }

    public static float operator -(Stat p1, float p2)
    {
        float _operStat = p1.Value - p2;
        return _operStat;
    }

    public static float operator *(Stat p1, float p2)
    {
        float _operStat = p1.Value * p2;
        return _operStat;
    }

    public Stat()
    {
        _isDirty = true;
        _baseValue = 1f;
        _modifiers = new List<StatModifier>();
    }

    public Stat(float baseValue)
    {
        _isDirty = true;
        BaseValue = baseValue;
        _modifiers = new List<StatModifier>();
    }

    public void AddModifier(StatModifier modifier)
    {
        _isDirty = true;
        _modifiers.Add(modifier);
        _modifiers.Sort(SortOrder);
    }

    public bool RemoveModifier(StatModifier modifier)
    {
        _isDirty = true;
        return _modifiers.Remove(modifier);
    }

    public float CalculateModifier()
    {
        float value = 0;
        int index = 0;
        value = FlatCalculation(value, ref index);
        value = PercentValueCalculation(_baseValue + value, ref index);
        value *= TimesCalculation(ref index);
        value += FixedCalculation(ref index);

        return (float)Math.Round(value, 3);
    }

    float FlatCalculation(float value, ref int index)
    {
        for (int i = index; i < _modifiers.Count; i++)
        {
            if (_modifiers[i].Type == StatModType.Flat) value += _modifiers[i].Value;
            else
            {
                index = i;
                break;
            }
        }
        return value;
    }

    float PercentValueCalculation(float value, ref int index)
    {
        for (int i = index; i < _modifiers.Count; i++)
        {
            if (_modifiers[i].Type == StatModType.Percent) value *= (Math.Abs(_modifiers[i].Value) * 0.01f);
            else
            {
                index = i;
                break;
            }
        }
        return value;
    }

    float TimesCalculation(ref int index)
    {
        float timesValue = 1;
        for (int i = index; i < _modifiers.Count; i++)
        {
            if (_modifiers[i].Type == StatModType.Times) timesValue += _modifiers[i].Value;
            else
            {
                index = i;
                break;
            }
        }
        return timesValue;
    }

    float FixedCalculation(ref int index)
    {
        float fixedValue = 0;
        for (int i = index; i < _modifiers.Count; i++)
        {
            if (_modifiers[i].Type == StatModType.Fixed) fixedValue += _modifiers[i].Value;
        }
        return fixedValue;
    }

    public int SortOrder(StatModifier a, StatModifier b)
    {
        if (a.Type < b.Type) return -1;
        if (a.Type > b.Type) return 1;
        else return 0;
    }
}
