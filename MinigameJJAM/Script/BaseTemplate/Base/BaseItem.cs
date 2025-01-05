using System;
using UnityEngine;

namespace starinc.io
{
    public class BaseItem : MonoBehaviour
    {
        #region Cache
        protected Rigidbody2D _rigidbody;
        protected Collider2D _collider;
        #endregion

        #region Callback
        public event Action<BaseItem> OnReturnToPool;
        #endregion

        private void Awake()
        {
            BindInitialization();
        }

        protected virtual void OnEnable()
        {
            Reset();
        }

        protected virtual void OnDisable() { }

        protected virtual void BindInitialization()
        {
            _rigidbody = Util.GetOrAddComponent<Rigidbody2D>(gameObject);
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
        }

        protected virtual void Reset() { }
        
        protected virtual void Interact(CreatureController character) { }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.CompareTag("Player"))
            {
                var character = collision.GetComponent<CreatureController>();
                Interact(character);
                OnReturnToPool?.Invoke(this);
            }
        }
    }
}
