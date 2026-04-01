using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConditionType { TALK, POSSESSIONITEM, KILL }

[System.Serializable]
public class QuestCondition
{
    public ConditionType type;
    public int conditionID;
    public string targetObjectID;
    public int targetID;
    public int objectiveCount;
}
