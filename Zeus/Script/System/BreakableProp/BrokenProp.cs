using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Zeus
{
    public class BrokenProp : MonoBehaviour
    {
        [SerializeField] private float _duration = 3f;
        [SerializeField] private List<Rigidbody> _fragments;

        private void Awake()
        {
            if (_fragments.Count == 0) _fragments = GetComponentsInChildren<Rigidbody>().ToList();
        }

        public void Break(bool explosion = false, Vector3 explosionPoint = default, float explosionForce = 100f)
        {
            StartCoroutine(Destroy(explosion, explosionPoint, explosionForce));
        }

        private IEnumerator Destroy(bool explosion, Vector3 explosionPoint, float explosionForce)
        {
            if (explosion)
            {
                for (int i = 0; i < _fragments.Count; i++)
                {
                    var fragment = _fragments[i];
                    fragment.AddExplosionForce(explosionForce, explosionPoint, 10f);
                }
            }

            yield return new WaitForSeconds(_duration);
            for (int i = 0; i < _fragments.Count; i++)
            {
                var fragment = _fragments[i];
                fragment.transform.DOScale(0, 0.5f).onComplete = () => { _fragments.Remove(fragment); };
            }

            yield return new WaitWhile(() => _fragments.Count > 0);
            Destroy(this.gameObject);
        }
    }
}