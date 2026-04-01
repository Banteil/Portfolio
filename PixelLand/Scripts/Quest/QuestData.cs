using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestData
{
    public int questID;
    public string questName;
    public string description;
    public List<int> questConditionIDList = new List<int>();
    public string rewardCode;
    public int rewardCount;
}
