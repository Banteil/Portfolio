using UnityEngine;

namespace Zeus
{
    public class EffectController
    {
        internal int ObejectID { get; set; }
        internal bool Area { get; set; }
        internal bool Inifinity { get; private set; }
        internal int TableID { get; set; }
        internal float RemainTime { get; private set; }

        private GameObject effectObject;

        internal int StartEffect(int tableID, Vector3 poistion, Vector3 lookDirection, Transform panrent, float duration, float scale)
        {
            TableID = tableID;
            var ob = TableManager.GetEffectGameObject(TableID);
            if (ob == null)
            {
                Debug.LogError($"Not Found Effect : TableID {tableID}");
                return 0;
            }
            var rot = Quaternion.FromToRotation(Vector3.forward, lookDirection);
            ob.transform.SetPositionAndRotation(poistion, rot); 
            return StartEffect(ob, poistion, panrent, duration, scale);
        }

        internal int StartEffect(GameObject ob, Vector3 poistion, Transform panrent, float duration, float scale)
        {
            if (ob == null) 
            {
                Debug.LogError("Object is Null");
                return -1;
            }

            Inifinity = duration == 0;
            RemainTime = duration;
            effectObject = ob;
            effectObject.transform.localScale = ob.transform.localScale * scale;
            effectObject.transform.SetPositionAndRotation(poistion, ob.transform.rotation);
            if (panrent != null)
                effectObject.transform.SetParent(panrent, false);
            effectObject.SetActive(true);
            var particleSystems = effectObject.GetComponentsInChildren<ParticleSystem>();
            foreach (var item in particleSystems)
            {
                item.Play();
            }
            ObejectID = ob.GetInstanceID();
            return ObejectID;
        }

        internal void Destroy()
        {
            if (effectObject != null)
            {
                var particleSystems = effectObject.GetComponentsInChildren<ParticleSystem>();
                foreach (var item in particleSystems)
                {
                    item.Stop();
                }

                GameObject.Destroy(effectObject, 1f);
                effectObject = null;
            }

            RemainTime = 0;
        }

        internal void Update(bool timeCalculate = true)
        {
            if (Inifinity)
                return;

            if (RemainTime == 0)
                return;

            if (!timeCalculate)
                return;

            RemainTime -= GameTimeManager.Instance.DeltaTime;
            if (RemainTime <= 0)
            {
                Destroy();
            }
        }

        internal bool UpdateRemainTime(float duration)
        {
            if (RemainTime <= 0) 
                return false;

            RemainTime = duration;
            return true;    
        }
    }
}
