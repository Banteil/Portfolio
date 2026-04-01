using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T));
                if (_instance == null) { 
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<T>(); 
                    singletonObject.name = typeof(T).ToString() + " (Singleton)"; 
                }
                DontDestroyOnLoad(_instance.gameObject);
            } 
            return _instance;
        }
    }
}
