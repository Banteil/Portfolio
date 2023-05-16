using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public enum TypeSkillFire
    {
        NONE,
        PROJECTILE,
        WORLD,
        LOCAL,
    }

    public enum TypeSkillTarget
    {
        NONE,
        AUTO, //CURRENT TARGET우선. 없으면 타겟시스템에 의해 받아서 처리.
    }

    [System.Serializable]
    public class SequenceSkillData : AssetTableData
    {
        public TypeSkillFire FireType;
        public TypeSkillTarget TargetType;
        public string Icon;
        public float DelayActiveTime;
        public float LifeTime;
        public int Damage;
        public int SoundTableID;
    }

    [System.Serializable]
    public class SkillTableData : SequenceSkillData
    {
        public SequenceSkillData[] SequenceSkillDatas;
    }

    [CreateAssetMenu(fileName = "SkillTable", menuName = "Zeus/Table/SkillTable")]
    public class SkillTable : ScriptableObject
    {
        public  List<SkillTableData> TableDatas;

        internal SkillTableData GetData(int id)
        {
            return TableDatas.Find(_ => _.ID == id);
        }
    }
}
