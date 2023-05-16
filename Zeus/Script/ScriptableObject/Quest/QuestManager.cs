using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class QuestManager : UnitySingleton<QuestManager>
    {
        //public List<int> ClearedIDs;

        // 진행중인 퀘스트
        public List<QuestProcess> CurrentProcess = new List<QuestProcess>();
        public List<QuestProcess> DependenceProcess = new List<QuestProcess>();
        public List<QuestProcess> RemovableProcess = new List<QuestProcess>();

        public event UnityAction<QuestSO> CallAddQuest = default;

        private void Start()
        {
            var questTable = TableManager.GetQuestTable();
            var curPlayer = TableManager.CurrentPlayerData;
            foreach (var serializeData in curPlayer.ProgressQuestDatas)
            {
                var quest = questTable.GetData(serializeData.ID);
                AddProcess(quest, serializeData);
            }
        }

        public void AddQuest(int questID)
        {
            AddQuest(TableManager.GetQuestTable()?.GetData(questID));
        }
        // 메인퀘스트와 사이드퀘스트의 표현방식 다르게
        // isDependence가 true일 경우 특정 퀘스트에 종속되어 있으므로 DependenceProcess에 추가됨
        public void AddQuest(QuestSO quest)
        {
            if (quest == null) return;

            var curPlayer = TableManager.CurrentPlayerData;

            // 해당 퀘스트의 선행퀘스트 ID가 0이나 -1이라면 선행퀘스트 클리어여부 상관없음
            if (quest.PrevQuestID != 0 && quest.PrevQuestID != -1)
            {
                // 선행퀘스트 클리어내역이 없다면 현재 퀘스트를 추가하지 못함
                if (!curPlayer.IsQuestClear(quest.PrevQuestID)) return;
            }

            // 이미 진행중인 같은 퀘스트를 진행중
            if (curPlayer.HasQuest(quest.ID)) return;

            // 퀘스트 기록
            // 실패시 Processer 생성하지 않음
            if (!curPlayer.AddQuest(quest, out var serializeQuest)) return;

            // 기록된 퀘스트 프로세스 등록
            AddProcess(quest, serializeQuest);
            CallAddQuest?.Invoke(quest);
        }
        private void AddProcess(QuestSO quest, QuestSerializeData serializeData)
        {
            var targetList = quest.DependenceQuestID != -1 ? DependenceProcess : CurrentProcess;
            targetList = serializeData.IsDone ? RemovableProcess : targetList;
            // 이미 진행중
            if (targetList.Exists(x => x.ID == serializeData.ID)) return;

            // 프로세스 생성
            targetList.Add(new QuestProcess(serializeData));

            // 하위퀘스트 추가 (종속퀘스트가 없었기 때문에 기존 목록에 존재하면 안됨)
            var questTable = TableManager.GetQuestTable();
            var dependenceQuests = questTable.Quests.Where(x => x.DependenceQuestID == serializeData.ID).ToList();
            for (int i = 0; i < dependenceQuests.Count; i++)
            {
                // 하위퀘스트 추가
                AddQuest(dependenceQuests[i]);
            }
        }
        public void UpdateQuest(TypeQuestStep type, int targetID, int targetValue = 1)
        {
            UpdateProcess(CurrentProcess, type, targetID, targetValue);
            UpdateProcess(DependenceProcess, type, targetID, targetValue);
        }
        private void UpdateProcess(List<QuestProcess> targetList, TypeQuestStep type, int targetID, int targetValue = 1)
        {
            var questTable = TableManager.GetQuestTable();
            if (questTable == null) return;

            var curPlayer = TableManager.CurrentPlayerData;

            for (int i = targetList.Count - 1; i > -1; i--)
            {
                // 프로세스 업데이트
                var process = targetList[i];
                var curQuest = questTable.GetData(process.ID);
                process.UpdateStep(type, targetID, targetValue);
                // 세이브데이터에 기록
                var serializeData = curPlayer.GetProgressQuestData(process.ID);
                serializeData.Values = process.StepStates;
                serializeData.IsDone = process.IsDone;

                if (!process.IsDone) continue;

                // 즉시완료라면 해당 Method에서 삭제처리
                if (curQuest.ClearType != TypeQuestClear.AUTO)
                {
                    targetList.Remove(process);
                    RemovableProcess.Add(process);
                    continue;
                }

                RemoveQuest(process);
            }
        }
        public void RemoveQuest(QuestProcess process, List<QuestProcess> targetProcessList = null)
        {
            if (process == null) return;

            var questTable = TableManager.GetQuestTable();
            var curPlayer = TableManager.CurrentPlayerData;
            var curQuest = questTable.GetData(process.ID);

            List<QuestProcess> targetList = null;
            if (targetProcessList == null)
                targetList = curQuest.DependenceQuestID != -1 ? DependenceProcess : CurrentProcess;
            else
                targetList = targetProcessList;

            if (targetList == null) return;

            // 현재퀘스트 삭제
            targetList.Remove(process);
            if (process.IsDone)
            {
                curPlayer.ClearQuest(process.ID);
                //TableManager.CurrentPlayerData.AddWeapon(curQuest.RewardItemID);
                //var rewardUI = PlayerUIManager.Get().GetUI<PlayerRewardTypeUI>(TypePlayerUI.REWARD);
                //rewardUI.AddItem(TableManager.GetString(curQuest.RewardItemID), null);

                if (curQuest.RewardItemType == TypeItem.WEAPON)
                    TableManager.CurrentPlayerData.AddWeapon(curQuest.RewardItemID);
                if (curQuest.RewardItemType == TypeItem.CONSUME)
                    TableManager.CurrentPlayerData.AddConsume(curQuest.RewardItemID);

                var rewardUI = PlayerUIManager.Get().GetUI<PlayerRewardTypeUI>(TypePlayerUI.REWARD);
                rewardUI.AddItem(TableManager.GetString(curQuest.RewardItemID), null);
            }
            else
                curPlayer.RemoveQuest(process.ID);

            // 하위퀘스트 삭제
            // WARNING : 하위퀘스트의 하위퀘스트도 DependenceProcess에 기록?
            // 위의 경우 리스트가 꼬일 수 있음. 다른 방법이 필요함.
            // dependence의 depth를 기록?
            var dependenceQuests = questTable.Quests.Where(x => x.DependenceQuestID == process.ID).ToList();
            foreach (var dependenceQuest in dependenceQuests)
            {
                var dependenceProcess = DependenceProcess.Find(x => x.ID == dependenceQuest.ID);
                RemoveQuest(dependenceProcess);
            }

            // 다음퀘스트 추가
            var nextQuest = questTable.GetData(curQuest.NextQuestID);
            AddQuest(nextQuest);
        }
        public void InteractionTarget(int actorID)
        {
            for (int i = RemovableProcess.Count - 1; i > -1; i--)
            {
                var process = RemovableProcess[i];
                if (process.ID != actorID) continue;
                RemoveQuest(process, RemovableProcess);
            }
        }
    }

    [System.Serializable]
    public class QuestSerializeData
    {
        public int ID;
        public int[] Values;
        public bool IsDone;
    }

    [System.Serializable]
    public class QuestProcess
    {
        // 퀘스트 ID
        public int ID;
        // 스탭 정보
        public TypeQuestStepCondition ClearCondition;
        public QuestStepProcess[] Steps;

        // 필요한가?
        public bool IsDone => ClearCheck();
        public int[] StepStates => Steps.Select(x => x.CurrentValue).ToArray();

        public QuestProcess(QuestSerializeData serializeData)
        {
            var quest = TableManager.GetQuestTable().GetData(serializeData.ID);
            ID = quest.ID;
            ClearCondition = quest.ClearCondition;
            Steps = new QuestStepProcess[quest.Steps.Length];
            for (int i = 0; i < quest.Steps.Length; i++)
            {
                Steps[i] = new QuestStepProcess(i, quest.Steps[i], serializeData.Values[i], serializeData.IsDone);
            }
        }

        public void UpdateStep(TypeQuestStep type, int targetID, int targetValue)
        {
            for (int i = 0; i < Steps.Length; i++)
            {
                var step = Steps[i];
                step.UpdateStep(type, targetID, targetValue);
            }
        }

        private bool ClearCheck()
        {
            // and
            if (ClearCondition == TypeQuestStepCondition.AND)
            {
                // 미완료 스탭이 하나라도 존재하면 false
                if (Steps.Where(x => !x.IsDone)?.Count() > 0) return false;
                else return true;
            }
            // or
            else
            {
                // 완료 스탭이 하나라도 존재하면 true
                if (Steps.Where(x => x.IsDone)?.Count() > 0) return true;
                else return false;
            }
        }
    }

    [System.Serializable]
    public class QuestStepProcess
    {
        // 타입 ?? 퀘스트 진행여부 체크 어디서?
        public int Index = -1;
        // 완료여부
        public bool IsDone = false;

        public QuestStepData StepData = default;
        // 진행값
        public int CurrentValue;

        public QuestStepProcess(int index, QuestStepData stepData, int curValue, bool isDone)
        {
            Index = index;
            IsDone = false;

            StepData = new QuestStepData()
            {
                StepType = stepData.StepType,
                TargetID = stepData.TargetID,
                StepTargetValue = stepData.StepTargetValue,
            };

            CurrentValue = curValue;
            IsDone = isDone;
        }

        public void UpdateStep(TypeQuestStep type, int targetID, int targetValue)
        {
            if (StepData.StepType != type) return;

            // ID가 -1일경우 무조건 return
            if (StepData.TargetID == -1 || targetID == -1) return;

            // ID가 0일경우 무조건 true
            if ((targetID != 0 && StepData.TargetID != 0) && (StepData.TargetID != targetID)) return;

            if (type == TypeQuestStep.CONTACT_TARGET)
                UpdateContactTarget(targetValue);
            if (type == TypeQuestStep.KILL_TARGET)
                UpdateKillTarget(targetValue);
            if (type == TypeQuestStep.VISITE_AREA)
                UpdateVisiteArea();
        }

        #region Update Method
        public void UpdateContactTarget(int targetValue)
        {
            IsDone = CurrentValue <= StepData.StepTargetValue;
        }
        public void UpdateKillTarget(int targetValue)
        {
            CurrentValue += targetValue;
            IsDone = CurrentValue >= StepData.StepTargetValue;
        }
        public void UpdateVisiteArea()
        {
            IsDone = true;
        }
        #endregion
    }
}
