using UnityEngine;

namespace Zeus
{
    public abstract class BaseObject<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static MonoBehaviour instance;
        // Use this for initialization
        private void Awake()
        {
            instance = this;

            _OnAwake();
        }

        public static T Get()
        {
            return instance as T;
        }

        protected virtual void _OnAwake() { }
    }
}