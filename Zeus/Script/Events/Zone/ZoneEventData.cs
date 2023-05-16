using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    [RequireComponent(typeof(ZoneData))]
    public class ZoneEventData : MonoBehaviour
    {
        public UnityEvent CompleteEvent;
        protected ZoneData _zoneData;

        public virtual void CheckCondition() { CompleteEvent?.Invoke(); }
        public virtual void CheckCondition(Character character) { CompleteEvent?.Invoke(); }

        private void Awake()
        {
            _zoneData = GetComponent<ZoneData>();
        }

        public virtual void Initialize() { }

        protected virtual void TriggerEnterEvent(Collider other) { }


        protected void OnTriggerEnter(Collider other)
        {
            TriggerEnterEvent(other);
        }
    }
}
