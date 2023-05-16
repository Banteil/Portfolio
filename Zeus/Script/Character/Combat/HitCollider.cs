using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [ClassHeader("Hit Collider", OpenClose = false)]
    public class HitCollider : zMonoBehaviour
    {
        [ReadOnly(false)] public string HitColliderName;
        public List<Collider> HitDeterminationColliders = new List<Collider>();
        public Character Owner;
        public TypeHitMaterial HitMaterial;

        public void ActiveHitCollider(bool active)
        {
            foreach(var collider in HitDeterminationColliders)
            {
                collider.enabled = active;
            }
        }
    }
}
