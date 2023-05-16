using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public enum TypeMapCheck
    {
        LEVEL1,
        LEVEL2,
        LEVEL3,
        LEVEL4,
    }

    [RequireComponent(typeof(SphereCollider))]
    public class MapCheck : MonoBehaviour
    {
        public UnityAction<TypeMapCheck, Collider, bool> CallCollisionChecker;
        public TypeMapCheck CheckType;

        IEnumerator Start()
        {
            GetComponent<Collider>().enabled = false;
            yield return new WaitUntil(() => WorldMapLoadManager.Get() != null);

            WorldMapLoadManager.Get().AddCollisionEvent(this);
            GetComponent<Collider>().enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<MapDataObject>(out _))
                return;

            //Debug.Log("OnTriggerEnter === " + other.name + $"LEVEL = {CheckType}");
            CallCollisionChecker?.Invoke(CheckType, other, true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<MapDataObject>(out _))
                return;

            //Debug.Log("OnTriggerExit === " + other.name + $"LEVEL = {CheckType}");
            CallCollisionChecker?.Invoke(CheckType, other, false);
        }
    }
}