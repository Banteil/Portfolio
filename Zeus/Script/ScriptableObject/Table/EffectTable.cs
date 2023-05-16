using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public enum TypeHitMaterial
    {
        NONE = 0,
        METAL = 4501, //EffectTable ID
        WOOD = 4502,
        STONE = 4503,
        WATER = 4504,
    }

    [System.Serializable]
    public class AssetTableData
    {
        public int ID;
        public string AssetName;
    }

    [CreateAssetMenu(fileName = "EffectTable", menuName = "Zeus/Table/EffectTable")]
    public class EffectTable : ScriptableObject
    {
        public List<AssetTableData> TableDatas;

        internal AssetTableData GetData(int id)
        {
            return TableDatas.Find(_ => _.ID == id);
        }
    }
}
