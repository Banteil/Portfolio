using UnityEngine;

public class ObjectSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = GetInstance();
            return _instance;
        }
    }

    private static T GetInstance()
    {
        var type = typeof(T);
        var temp = GameObject.FindObjectOfType(type) as T;
        if (!temp)
        {
            GameObject obj = new GameObject(type.Name);
            temp = obj.AddComponent<T>();
        }
        return temp;
    }

    private void OnDestroy()
    {
        if (_instance != null)
            Destroy(_instance.gameObject);
    }
}