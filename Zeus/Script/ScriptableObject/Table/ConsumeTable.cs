using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public struct ConsumeEffectInfo
    {
        public ConsumeFunction Function;
        public int Value;
    }

    [System.Serializable]
    public class ConsumeTableData
    {
        public int ID;
        public int NameID;
        public List<ConsumeEffectInfo> Effects;
        public string Icon;
        public int VfxID;
        public int SfxID;
        public string ObjectPath;
        public float Cooldown;
    }

    [CreateAssetMenu(fileName = "ConsumeTable", menuName = "Zeus/Table/ConsumeTable")]
    public class ConsumeTable : ScriptableObject
    {
        public ConsumeTableData[] ConsumeTableDatas;

        private Dictionary<int, ConsumeTableData> _consumeTable;

        private void OnEnable()
        {
            Initialized();
        }

        private void Initialized()
        {
            if (_consumeTable != null)
                return;

            _consumeTable = new Dictionary<int, ConsumeTableData>();

            foreach (var item in ConsumeTableDatas)
            {
                _consumeTable.Upsert(item.ID, item);
            }
        }
        internal ConsumeTableData GetData(int id)
        {
            if (_consumeTable.ContainsKey(id))
                return _consumeTable[id];

            return null;
        }
    }
}