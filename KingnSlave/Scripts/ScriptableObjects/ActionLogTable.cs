using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    [CreateAssetMenu(fileName = "ActionLogTable", menuName = "Table/ActionLog Table", order = 1)]
    public class ActionLogTable : ScriptableObject
    {
        public List<ActionLogData> ActionLogDatas = new List<ActionLogData>();
    }
}
