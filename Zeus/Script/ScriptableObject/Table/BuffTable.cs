using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public class BuffTableData
    {
        public int ID;
        public TypeBuff BuffType;
        public TypeDeBuff DebuffType;
        public float Power;
        public float Duration;
        public string Icon;
    }

    [CreateAssetMenu(fileName = "BuffTable", menuName = "Zeus/Table/BuffTable")]
    public class BuffTable : ScriptableObject
    {
        public List<BuffTableData> TableDatas;

        internal BuffTableData GetData(int tableID)
        {
            return TableDatas.Find(_=>_.ID == tableID);
        }
    }
}