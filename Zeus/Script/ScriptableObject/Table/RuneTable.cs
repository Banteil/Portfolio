using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public class RuneTableData
    {
        public int ID;
        public int SkillID;
        public string Icon;
    }

    [CreateAssetMenu(fileName = "RuneTable", menuName = "Zeus/Table/RuneTable")]
    public class RuneTable : ScriptableObject
    {
        public RuneTableData[] RuneTableDatas;

        private Dictionary<int, RuneTableData> _runeTable;

        private void OnEnable()
        {
            Initialized();
        }

        private void Initialized()
        {
            if (_runeTable != null)
                return;

            _runeTable = new Dictionary<int, RuneTableData>();

            foreach (var item in RuneTableDatas)
            {
                _runeTable.Upsert(item.ID, item);
            }
        }
        internal RuneTableData GetData(int id)
        {
            if (_runeTable.ContainsKey(id))
                return _runeTable[id];

            return null;
        }
        //internal RuneTableData GetData(int runeID)
        //{
        //    var data = RuneTableDatas.Where(x => x.ID == runeID).FirstOrDefault();
        //    return data;
        //}
    }
}