using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestInfomation : MonoBehaviour
{
    [SerializeField]
    Text questNameText;
    [SerializeField]
    Text questDescriptionText;
    [SerializeField]
    RewardObject rewardObject;

    public void SetQuestInfo(QuestData data)
    {
        questNameText.text = data.questName;
        questDescriptionText.text = data.description;
        Item reward = new Item();
        reward.Info = ItemManager.Instance.GetItemInfoSameID(data.rewardCode);
        rewardObject.Item = reward;
    }
}
