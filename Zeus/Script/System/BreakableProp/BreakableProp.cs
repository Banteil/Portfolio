using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class BreakableProp : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private BrokenProp _brokenProp;
        [SerializeField] private Transform _dustVFX;
        [SerializeField] private Transform _fragVFX;
        [SerializeField] private UnityEvent _onDestroy;

        private bool _isBroken;

        private void Awake()
        {
            _brokenProp.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isBroken) return;
            if (!_layerMask.ContainsLayer(other.gameObject.layer)) return;

            var hitPoint = other.ClosestPointOnBounds(transform.position);
            var rot = gameObject.transform.rotation;
            var pos = gameObject.transform.position;

            _isBroken = true;

            if (_brokenProp != null)
            {
                _brokenProp.transform.parent = null;
                _brokenProp.gameObject.SetActive(true);
                _brokenProp.Break(true, hitPoint, 200f);
                _onDestroy?.Invoke();
            }

            if (_dustVFX != null)
            {
                // Spawn dustVFX
                var dust = Instantiate(_dustVFX, pos, rot);
                if (dust != null)
                { 
                    EffectsManager.Get().SetEffect(dust.gameObject, pos);
                    EffectsManager.Get().AddParticle(dust.gameObject);
                }
            }
            if (_fragVFX != null)
            {
                // Spawn fragsVFX
                var frag = Instantiate(_fragVFX, pos, rot);
                if (frag != null)
                {
                    EffectsManager.Get().SetEffect(frag.gameObject, pos);
                    EffectsManager.Get().AddParticle(frag.gameObject);
                }
            }
            Destroy(gameObject);

            //Rigidbody rigid = this.transform.GetComponent<Rigidbody>();
            //rigid.AddExplosionForce(1000, hitPoint, 2);
        }
        //private void OnTriggerStay(Collider other)
        //{
        //    if (_isBroken) return;
        //    if (!_layerMask.ContainsLayer(other.gameObject.layer)) return;

        //    var hitPoint = other.ClosestPointOnBounds(transform.position);
        //    //var hitVector = hitPoint - transform.position;

        //    //if (hitVector.magnitude < resistance)
        //    //{
        //    BreakIt();
        //    Rigidbody rigid = this.transform.GetComponent<Rigidbody>();
        //    rigid.AddExplosionForce(1000, hitPoint, 2);
        //    //}
        //}
        public void BreakIt()
        {

            // get transform gameobject

            Quaternion rot = gameObject.transform.rotation;
            Vector3 pos = gameObject.transform.position;


            if (_isBroken != true)
            {
                // Spwawn Broekn Prefab
                if (_brokenProp != null)
                {
                    _brokenProp.transform.parent = null;
                    _brokenProp.gameObject.SetActive(true);
                    _brokenProp.Break();
                }

                // prevent to sapwn again
                _isBroken = true;

                // check if a VFX prefabs is setup
                if (_dustVFX != null)
                {
                    // Spawn dustVFX
                    Instantiate(_dustVFX, pos, rot);
                }
                if (_fragVFX != null)
                {
                    // Spawn fragsVFX
                    Instantiate(_fragVFX, pos, rot);
                }
                Destroy(gameObject);
            }
        }
    }
}
