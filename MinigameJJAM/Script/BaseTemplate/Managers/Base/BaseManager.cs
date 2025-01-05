using UnityEngine;

namespace starinc.io
{
    public class BaseManager : MonoBehaviour
    {
        private void Awake()
        {
            var result = Manager.Set(this);
            if (!result)
                Destroy(gameObject);
            else
            {
                Util.DontDestroyObject(gameObject);
                transform.position = Vector3.zero;
                OnAwake();
            }
        }

        protected virtual void OnAwake() { }

        private void Start() => OnStart();

        protected virtual void OnStart() { }
    }
}
