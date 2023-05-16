using System.Collections;
using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public abstract class QuestStepSO : ScriptableObject
    {
        public int ID;
        public int NameID;
        public int DescriptionID;
    }
}