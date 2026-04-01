using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectObject : MonoBehaviour
{
    Vector3 savePos;
    Vector3 dir;
    float time;
    int directionCheck;

    private void OnEnable()
    {
        savePos = transform.position;
        dir = (savePos - transform.parent.position).normalized;
        time = 0f;
        directionCheck = 1;
    }

    void Update()
    {
        if (time >= 0.3f)
        {
            directionCheck *= -1;
            time = 0f;
        }

        transform.localPosition += dir * directionCheck * Time.deltaTime;
        time += Time.deltaTime;
    }

    private void OnDisable()
    {
        transform.position = savePos;
    }
}
