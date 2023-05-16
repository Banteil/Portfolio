using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Zeus
{
    [Serializable]
    public class ExcutionTableData
    {
        public int ID;
        public int ExcutionAnimationNum;
        public string ExcutionTargetAnimation;
        public string CutScene;
        public float DistanceBetweenCharacters;
    }


    [CreateAssetMenu(fileName = "WeaponTable", menuName = "Zeus/Table/ExcutionTable")]
    public class ExcutionTable : ScriptableObject
    {
        public ExcutionTableData[] excutionTableData;
        
        internal ExcutionTableData GetData(int id)
        {
            return Array.Find(excutionTableData, (x) => x.ID == id);
        }
    }
}