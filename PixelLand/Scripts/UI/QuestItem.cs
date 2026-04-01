using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestItem : MonoBehaviour
{
    [SerializeField]
    Text questNameText;
    QuestData data;
    public QuestData Data 
    { 
        get { return data; } 
        set 
        { 
            data = value;
            questNameText.text = data.questName;
        } 
    }

    void Awake()
    {
        questNameText = transform.GetChild(1).GetComponent<Text>();
    }

    public void DisplayQuestInfomation()
    {
        UIManager.Instance.GetUI("QuestUI").GetComponent<QuestUI>().QuestInfo.SetQuestInfo(data);
        UIManager.Instance.GetUI("QuestUI").GetComponent<QuestUI>().QuestInfo.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        UIManager.Instance.GetUI("QuestUI").GetComponent<QuestUI>().QuestItemList.Enqueue(this);
    }
}
