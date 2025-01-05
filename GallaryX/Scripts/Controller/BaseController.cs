using UnityEngine;

namespace starinc.io.gallaryx
{
    public class BaseController : MonoBehaviour
    {
        private void Awake() => OnAwake();

        protected virtual void OnAwake() { }

        private void Start() => OnStart();

        protected virtual void OnStart() { }

        private void Update() => OnUpdate();

        protected virtual void OnUpdate()
        {
            InputStateFunction();
            InputActionFunction();
        }

        protected virtual void InputStateFunction() { }

        protected virtual void InputActionFunction() { }
    }
}
