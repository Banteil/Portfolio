using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{    
    void Update()
    {
        transform.position = Camera.main.transform.position;
    }
}
