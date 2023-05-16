using UnityEngine;

namespace Zeus
{
    public class ZoneObject : MonoBehaviour
    {
        public string Name;
        public ZoneObjectsInfo ObjectInfo = new ZoneObjectsInfo();

        internal virtual ZoneInfo ZoneInfo { get; set; }

        private void Start()
        {
            ObjectInfo.Name = Name;
            ObjectInfo.SpawnPosition = transform.position;
            ObjectInfo.SpawnRotation = transform.rotation;
        }
    }
}
