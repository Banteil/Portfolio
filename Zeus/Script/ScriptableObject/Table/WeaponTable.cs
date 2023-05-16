using System;
using UnityEngine;

namespace Zeus
{
    public enum TypeWeapon { NONE = 0, ONEHAND = 1, ONEHANDANDSHEILD = 2, TWOHAND = 3, BOW = 4, ARROW = 5, }

    [System.Serializable]
    public class WeaponTableData
    {
        public int ID;
        public TypeWeapon WeaponCategory;
        public string LeftWeapon;
        public string RightWeapon;
        public string Icon;
        public int StringTableID;
        public string LeftHandMeshEffectPath;
        public string RightHandMeshEffectPath;
        public int Damage;
        public WeaponSkill[] SkillDatas;
        public int ParrySkillID;
        public int[] HitSoundIDs;
        public string GuardEffect;
        public string GuardHitEffect;
        public int GuardSoundID;
        public int GuardRandomSoundID;
        public int ParrySoundID;
        public int ExcutionID;
        public int[] MaterializeSound;
    }

    [System.Serializable]
    public class WeaponSkill
    {
        public int SkillID;
        public int SkillAnimationID;
    }

    [CreateAssetMenu(fileName = "WeaponTable", menuName = "Zeus/Table/WeaponTable")]
    public class WeaponTable : ScriptableObject
    {
        public WeaponTableData[] weaponTableDatas;

        internal WeaponTableData GetData(int id)
        {
            return Array.Find(weaponTableDatas,(x) => x.ID == id);
        }
    }
}
