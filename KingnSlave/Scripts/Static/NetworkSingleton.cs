using Fusion;
using UnityEngine;

namespace starinc.io
{
    public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if ((UnityEngine.Object)_instance == (UnityEngine.Object)null)
                {
                    _instance = UnityEngine.Object.FindObjectOfType<T>();
                    if (UnityEngine.Object.FindObjectsOfType<T>().Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong  - there should never be more than 1 singleton! Reopening the scene might fix it.");
                        return _instance;
                    }
                    if ((UnityEngine.Object)_instance == (UnityEngine.Object)null)
                    {
                        GameObject obj = new GameObject(typeof(T).ToString());
                        _instance = obj.AddComponent<T>();
                        obj.name = typeof(T).ToString();
                        obj.transform.position = Vector3.zero;
                    }
                    return _instance;
                }
                return _instance;
            }
        }

        public static bool HasInstance
        {
            get
            {
                return _instance != null;
            }
        }

        private void Awake()
        {
            if ((UnityEngine.Object)_instance != (UnityEngine.Object)null && (UnityEngine.Object)_instance != (UnityEngine.Object)(this as T))
            {
                UnityEngine.Object.Destroy(base.gameObject);
                return;
            }
            else
            {
                _instance = Instance;
                OnAwake();
            }
        }

        protected virtual void OnAwake() { }
    }
}
