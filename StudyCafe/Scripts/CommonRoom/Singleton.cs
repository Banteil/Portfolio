using Photon.Pun;
using UnityEngine;
public class Singleton<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    static T instance = null;

    public static T Instance
    {
        get
        {
            instance = FindObjectOfType(typeof(T)) as T;

            if (instance == null)
            {
                Debug.Log("싱글톤 인스턴스 없음");
                //instance = new GameObject().AddComponent<T>();
                //instance.name = typeof(T).ToString();
                return null;
            }
            else
                return instance;
        }
    }
}