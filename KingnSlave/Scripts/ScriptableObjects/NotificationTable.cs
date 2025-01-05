using System.Collections.Generic;
using UnityEngine;

namespace starinc.io.kingnslave
{
    [CreateAssetMenu(fileName = "NotificationTable", menuName = "Table/Notification Table", order = 1)]
    public class NotificationTable : ScriptableObject
    {
        public List<NotificationData> NotificationDatas = new List<NotificationData>();
    }
}
