using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zeus
{
    public abstract class DataSO : ScriptableObject
    {
        [SerializeField] protected int _id = default;
        [SerializeField] protected string _name = default;
        [SerializeField] protected string _description = default;
    }

    [CreateAssetMenu(menuName = "Zeus/Table/Quest Table/Quest Table", order = int.MinValue)]
    public class QuestTable : ScriptableObject
    {
        [SerializeField] private QuestSO[] _questDatas;

        private Dictionary<int, QuestSO> _questTable;

        public List<int> QuestIDs => _questTable.Keys.ToList();
        public List<QuestSO> Quests => _questTable.Values.ToList();

        private void OnEnable()
        {
            Initialized();
        }

        private void Initialized()
        {
            if (_questTable != null)
                return;

            _questTable = new Dictionary<int, QuestSO>();

            foreach (var item in _questDatas)
            {
                _questTable.Upsert(item.ID, item);
            }
        }

        internal QuestSO GetData(int id)
        {
            if (_questTable.ContainsKey(id))
                return _questTable[id];

            return null;
        }
        internal QuestSO GetNextQuestData(int id)
        {
            var quest = GetData(id);
            if (quest == null) return null;
            return GetData(quest.NextQuestID);
        }
        internal List<QuestSO> GetDependenceQuestData(int id)
        {
            return _questTable.Values.Where(x => x.DependenceQuestID == id).ToList();
        }
    }
}
