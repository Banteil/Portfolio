using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDestroy : MonoBehaviour
{
    public float Time;

    void Start()
    {
        Invoke("DestroyObject", Time);
    }
    
    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
