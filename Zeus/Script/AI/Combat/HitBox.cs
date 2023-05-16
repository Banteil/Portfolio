using UnityEngine;
using System;
using UnityEngine.Events;

namespace Zeus
{
    [ClassHeader("HitBox", OpenClose = false)]
    public class HitBox : zMonoBehaviour
    {
        internal UnityAction<HitBox, Collider> CallHit;
        public string[] IgnoreTag;
        // [HideInInspector]
        public Collider Trigger
        {
            get
            {
                _trigger = gameObject.GetComponent<Collider>();

                if (!_trigger) _trigger = gameObject.AddComponent<BoxCollider>();
                return _trigger;
            }
        }

        public HitBoxType TriggerType = HitBoxType.Damage;
        protected Collider _trigger;
        public bool EnableOnStart;

        protected void OnDrawGizmos()
        {
            Color color = (TriggerType & HitBoxType.Damage) != 0 && (TriggerType & HitBoxType.Recoil) == 0 ? Color.green :
                           (TriggerType & HitBoxType.Damage) != 0 && (TriggerType & HitBoxType.Recoil) != 0 ? Color.yellow :
                           (TriggerType & HitBoxType.Recoil) != 0 && (TriggerType & HitBoxType.Damage) == 0 ? Color.red : Color.black;
            color.a = 0.6f;
            Gizmos.color = color;
            if (!Application.isPlaying && Trigger && !Trigger.enabled) Trigger.enabled = true;
            if (Trigger && Trigger.enabled)
            {
                if (Trigger as BoxCollider)
                {
                    BoxCollider box = Trigger as BoxCollider;

                    //var sizeX = transform.lossyScale.x * box.size.x;
                    //var sizeY = transform.lossyScale.y * box.size.y;
                    //var sizeZ = transform.lossyScale.z * box.size.z;
                    //Matrix4x4 rotationMatrix = Matrix4x4.TRS(box.bounds.center, transform.rotation, new Vector3(sizeX, sizeY, sizeZ));
                    //Gizmos.matrix = rotationMatrix;
                    //Gizmos.DrawCube(Vector3.zero, Vector3.one);

                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                    Gizmos.DrawCube(box.center, Vector3.Scale(Vector3.one, box.size));
                }
            }
        }

        protected void Start()
        {
            if (Trigger && !EnableOnStart)
            {
                Trigger.isTrigger = true;
                Trigger.enabled = false;
            }
            transform.gameObject.layer = LayerMask.NameToLayer("Attack");
        }

        protected void OnTriggerEnter(Collider other)
        {
            foreach (var item in IgnoreTag)
            {
                if (other.transform.root.gameObject.CompareTag(item))
                    return;
            }

            CallHit?.Invoke(this, other);
        }
    }

    [Flags]
    public enum HitBoxType
    {
        Damage = 1, Recoil = 2,
    }
}