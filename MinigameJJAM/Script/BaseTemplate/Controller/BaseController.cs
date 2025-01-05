using System;
using UnityEngine;

namespace starinc.io
{
    public class BaseController : MonoBehaviour
    {
        #region Cache
        public Animator ControllerAnimator { get; private set; }
        public Rigidbody2D Rigid2D { get; private set; }
        public SpriteRenderer Renderer { get; private set; }
        public Collider2D Collider { get; private set; }
        #endregion

        #region Callback
        public event Action OnDisableController;
        #endregion

        private void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            RequiredConfigurationCaching();
        }

        protected virtual void RequiredConfigurationCaching()
        {
            ControllerAnimator = GetComponent<Animator>();
            Renderer = GetComponent<SpriteRenderer>();
            Rigid2D = GetComponent<Rigidbody2D>();
            Collider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            OnStart();
        }

        protected virtual void OnStart() { }

        protected virtual void DisableController()
        {            
            OnDisableController?.Invoke();
        }

        public virtual void InputAction(Vector2 input) { }
    }
}
