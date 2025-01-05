using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class PrefabData
    {
        public string Name;
        [SerializeField]
        public GameObject PrefabObject;
        [TextArea]
        public string Description;
    }

    [CreateAssetMenu(fileName = "PrefabTable", menuName = "Table/PrefabTable", order = 1)]
    public class PrefabTable : ScriptableObject
    {
        public List<PrefabData> PrefabObjects;
    }
}
