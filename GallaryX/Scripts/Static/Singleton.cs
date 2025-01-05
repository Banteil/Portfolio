using UnityEngine;

namespace starinc.io.gallaryx
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
        [SerializeField]
        private bool _isDonDestroyObject = true;
        private static T _instance;

		public static T Instance
		{
			get
			{
				if ((Object)_instance == (Object)null)
				{
					_instance = Object.FindFirstObjectByType<T>();
					if (Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).Length > 1)
					{
						Debug.LogError("[Singleton] Something went really wrong  - there should never be more than 1 singleton! Reopening the scene might fix it.");
						return _instance;
					}
					if ((Object)_instance == (Object)null)
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
			if ((Object)_instance != (Object)null && (Object)_instance != (Object)(this as T))
			{
				Object.Destroy(base.gameObject);
				return;
			}
			else
			{
				_instance = Instance;
                if (_isDonDestroyObject)
                    Util.DontDestroyObject(base.gameObject);
                OnAwake();
			}
		}

		protected virtual void OnAwake() { }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}
