using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    Camera _minimapCamera;

    private void Awake()
    {
        _minimapCamera = GetComponent<Camera>();
    }

    public void SetPosition(Vector3 pos)
    {
        pos.z = -10f;
        transform.position = pos;
    }
}
