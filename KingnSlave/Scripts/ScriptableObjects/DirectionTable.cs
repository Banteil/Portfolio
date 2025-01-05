using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    [Serializable]
    public class DirectionData
    {
        public string Name;
        [SerializeField]
        public BaseDirection Direction;
        [TextArea]
        public string Description;
    }

    [CreateAssetMenu(fileName = "DirectionTable", menuName = "Table/DirectionTable", order = 1)]
    public class DirectionTable : ScriptableObject
    {
        public List<DirectionData> LoadingDirectionDatas;
    }
}
