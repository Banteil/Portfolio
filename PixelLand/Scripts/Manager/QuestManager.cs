using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    private static QuestManager instance = null;

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            TextAsset qJson = Resources.Load<TextAsset>("Json/QuestTable");
            questTable = JsonUtility.FromJson<QuestTable>(qJson.ToString());

            TextAsset qCJson = Resources.Load<TextAsset>("Json/QuestConditionTable");
            questConditionTable = JsonUtility.FromJson<QuestConditionTable>(qCJson.ToString());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static QuestManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Start()
    {
        //임시로 하나 넣음
        AcceptQuest(0);
    }

    //퀘스트 테이블
    QuestTable questTable;
    public QuestTable QTable { get { return questTable; } }
    //퀘스트 조건 테이블
    QuestConditionTable questConditionTable;
    public QuestConditionTable QCTable { get { return questConditionTable; } }

    //현재 진행 중인 퀘스트 목록
    List<QuestData> acceptedQuestList = new List<QuestData>();
    public List<QuestData> AcceptedQuestList { get { return acceptedQuestList; } }

    //현재 퀘스트 진행상황
    List<CurrentQuestInfo> currentQuestInfo = new List<CurrentQuestInfo>();
    public List<CurrentQuestInfo> CurrentInfo { get { return currentQuestInfo; } }

    public void AcceptQuest(int questID)
    {
        acceptedQuestList.Add(questTable.tableList[questID]);
        CurrentQuestInfo currentInfo = new CurrentQuestInfo();
        currentInfo.questID = questID;
        currentInfo.currentQuestContionID = questTable.tableList[questID].questConditionIDList[0];
        currentInfo.currentCount = 0;
        currentQuestInfo.Add(currentInfo);
    }

    //특정 타입의 NPC나 몬스터를 죽였을 때 현재 퀘스트 조건 체크하여 진행상황 업데이트
    public void QuestObserve(ConditionType type, int id, string objectID)
    {
        for (int i = 0; i < currentQuestInfo.Count; i++)
        {
            QuestCondition currentCondition = questConditionTable.tableList[currentQuestInfo[i].currentQuestContionID];
            //현재 진행중인 퀘스트 조건과 같지 않으면 패스
            if (!currentCondition.type.Equals(type)) continue;
            //타겟 ID와 동일하면 카운트 증가
            if (currentCondition.targetID.Equals(id))
            {
                //현재 퀘스트 조건이 오브젝트ID까지 체크하는지 여부 확인, null일 경우 패스
                if (currentCondition.targetObjectID.Equals("null"))
                {
                    currentQuestInfo[i].currentCount++;
                    //목표 수치에 다다랐으면 다음 퀘스트로 넘어감
                    if (currentCondition.objectiveCount.Equals(currentQuestInfo[i].currentCount))
                    {
                       NextCondition(currentQuestInfo[i]);
                        break;
                    }
                }
                else if (currentCondition.targetObjectID.Equals(objectID))
                {
                    currentQuestInfo[i].currentCount++;
                    //목표 수치에 다다랐으면 다음 퀘스트로 넘어감
                    if (currentCondition.objectiveCount.Equals(currentQuestInfo[i].currentCount))
                    {
                        NextCondition(currentQuestInfo[i]);
                        break;
                    }
                }
            }
        }        
    }

    void NextCondition(CurrentQuestInfo questInfo)
    {
        int lastConditionID = questTable.tableList[questInfo.questID].questConditionIDList[questTable.tableList[questInfo.questID].questConditionIDList.Count - 1];

        if (questInfo.currentQuestContionID.Equals(lastConditionID))
        {
            for (int i = 0; i < acceptedQuestList.Count; i++)
            {
                if (acceptedQuestList[i].questID.Equals(questInfo.questID))
                {
                    Debug.Log(acceptedQuestList[i].rewardCode);
                    ItemManager.Instance.GetQuestReward(acceptedQuestList[i].rewardCode, acceptedQuestList[i].rewardCount);
                    UIManager.Instance.GetUI("LogInfoUI").GetComponent<LogInfo>().DisplayLogInfo("'" + acceptedQuestList[i].questName + "' 퀘스트 클리어!");
                    for (int j = 0; j < currentQuestInfo.Count; j++)
                    {
                        if (currentQuestInfo[j].questID.Equals(questInfo.questID))
                        {
                            currentQuestInfo.RemoveAt(j);
                            break;
                        }
                    }
                    acceptedQuestList.RemoveAt(i);
                    break;
                }
            }
        }
        else
        {
            Debug.Log("다음 퀘스트 조건으로 진행");
            questInfo.currentQuestContionID++;
            questInfo.currentCount = 0;
        }
    }
}
