using UnityEngine;

namespace Zeus
{
    public abstract class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        internal static bool ApplicationIsQuitting = false;

        private static object _lock = new object();

        public static T Instance
        {
            get
            {
                if (!Application.isPlaying || ApplicationIsQuitting)
                {
                    return null;
                }
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (_instance == null)
                        {
                            var singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = typeof(T).ToString() + "Singleton";

                            DontDestroyOnLoad(_instance);
                        }
                    }
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (!Application.isPlaying)
                return;

            if (_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T));
                _instance.gameObject.name = typeof(T).ToString() + "Singleton";
                DontDestroyOnLoad(_instance);

                _OnAwake();
            }
            else
            {
                if (!IsSameObject())
                {
                    Destroy(gameObject);
                }
            }
        }

        private bool IsSameObject()
        {
            if (gameObject == null)
                return true;

            return _instance.gameObject.GetHashCode() == gameObject.GetHashCode();
        }

        public void OnDestroy()
        {
            _OnDestroy();
        }

        protected virtual void _OnAwake() { }
        protected virtual void _OnDestroy() { }

        public static bool HasInstance
        {
            get
            {
                return _instance != null && !ApplicationIsQuitting;
            }
        }

        private void OnApplicationQuit()
        {
//#if !UNITY_EDITOR
            ApplicationIsQuitting = true;
//#endif
            _instance = null;
        }
    }
}