using UnityEngine;

namespace Zeus
{
    public class TriggerEnterZone : ZoneEventData
    {
        protected override void TriggerEnterEvent(Collider other)
        {
            if (other.transform.CompareTag("Player"))
                CheckCondition();
        }
    }
}
