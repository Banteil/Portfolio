using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField]
    Transform content;
    public Transform Content { get { return content; } }

    [SerializeField]
    QuestInfomation questInfo;
    public QuestInfomation QuestInfo { get { return questInfo; } }

    Queue<QuestItem> questItemList = new Queue<QuestItem>();
    public Queue<QuestItem> QuestItemList { get { return questItemList; } }


    void Awake()
    {
        for (int i = 0; i < 20; i++)
        {
            questItemList.Enqueue(Instantiate(ResourceManager.Instance.QuestItemObj, content, false).GetComponent<QuestItem>());
        }
    }

    private void OnEnable()
    {
        transform.SetAsLastSibling();
        QuestItemSetting();
    }

    private void OnDisable()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            content.GetChild(i).gameObject.SetActive(false);
        }
        questInfo.gameObject.SetActive(false);
    }

    void QuestItemSetting()
    {
        for (int i = 0; i < QuestManager.Instance.AcceptedQuestList.Count; i++)
        {
            QuestItem item = null;
            if (questItemList.Count <= 0)
                item = Instantiate(ResourceManager.Instance.QuestItemObj, content, false).GetComponent<QuestItem>();
            else
                item = questItemList.Dequeue();
            item.Data = QuestManager.Instance.AcceptedQuestList[i];
            item.gameObject.SetActive(true);
        }
    }
}
