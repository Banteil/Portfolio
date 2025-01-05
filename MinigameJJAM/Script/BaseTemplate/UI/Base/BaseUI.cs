using UnityEngine;

namespace starinc.io
{
    public class BaseUI : BindBehavior
    {
        public Canvas UICanvas { get; private set; }
        public RectTransform RectTr { get { return (RectTransform)transform; } }        

        private void Awake()
        {
            UICanvas = gameObject.GetComponent<Canvas>();
            BindInitialization();
            OnAwake();
        }

        protected virtual void OnAwake() { }

        private void Start() => OnStart();

        protected virtual void OnStart() { }

        
        protected virtual void BindInitialization() { }

        protected virtual void OnDestroy() { }
    }
}
