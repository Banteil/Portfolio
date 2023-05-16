using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class GroundEffecter : MonoBehaviour
    {
        [SerializeField]
        protected List<Transform> _effectPivotList = new List<Transform>();
        float distance = 3f;

        public void CheckGround(int index, float scale = 1f, List<GameObject> additionalEffects = null)
        {
            if (index < 0 || _effectPivotList.Count <= index) return;

            RaycastHit hit;
            int layerMask = 1 << LayerMask.NameToLayer("Ground");
            var pivot = _effectPivotList[index];
            if (Physics.Raycast(pivot.position - (pivot.forward * (distance * 0.5f)), pivot.forward, out hit, distance, layerMask))
            {
                ActiveEffect(hit, pivot, scale, additionalEffects);
            }
            else if (Physics.SphereCast(pivot.position + (pivot.forward * distance), distance * 0.5f, Vector3.down, out hit, distance, layerMask))
            {
                ActiveEffect(hit, pivot, scale, additionalEffects);
            }
        }

        void ActiveEffect(RaycastHit hit, Transform pivot, float scale, List<GameObject> additionalEffects)
        {
            var componet = hit.collider.GetComponent<MaterialType>();
            if (componet == null)
                return;

            var hitDir = Vector3.Reflect(pivot.forward, hit.normal);
            EffectsManager.Get().SetEffect((int)componet.TypeOfMaterial, hit.point, hitDir, null, 5f, scale);
            if (additionalEffects == null || (additionalEffects != null && additionalEffects.Count <= 0)) return;
            for (int i = 0; i < additionalEffects.Count; i++)
            {
                var effectObj = additionalEffects[i];
                Instantiate(effectObj, hit.point, Quaternion.identity);
            }
        }
    }
}
