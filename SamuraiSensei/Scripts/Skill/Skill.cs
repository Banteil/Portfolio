using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType { ATTACK, RECOVERY, BUFF, FUNCTIONAL, DEBUFF }
public enum ControlType { FLEXIBLE, FIXED, LINE }

[System.Serializable]
public class Skill
{
    public int id;
    public int characterID;
    public SkillType type;
    public ControlType control;
    public ElementalProperties propertie;
    public int grade;
    public int level;
    public string skillName;
    public string description;
    public string formula;
    public string cost;
    public string rigidTime;
    public string knockBackPower;
}
