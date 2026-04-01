using UnityEngine;

public class ManagerSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T _instance;
    public static T Instance => _instance;

    private void Awake()
    {
        var temp = GameObject.FindObjectOfType(typeof(T)) as T;
        if (temp)
            Destroy(this.gameObject);
        _instance = this.gameObject.GetComponent<T>();
    }
}