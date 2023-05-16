using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Zeus
{
    public class ZeusGuardEffect : MonoBehaviour
    {
        public ParticleSystem GuardEffect;
        public GameObject CollisionEffect;
        public float LifeTime = 2f;

        private BoxCollider _boxColl;

        private void Start()
        {
            _boxColl = GetComponent<BoxCollider>();
        }

        public void Active()
        {
            if (_boxColl == null) { return; }
            _boxColl.enabled = true;
        }

        public void DeActive()
        {
            if (_boxColl == null) { return; }
            _boxColl.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {

        }
    }
}