using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTest : MonoBehaviour
{
    public LayerMask CheckLayerMask;
    // Start is called before the first frame update


    // Update is called once per frame
    void FixedUpdate()
    {
        var startPosition = transform.position;
        startPosition.y += 1.5f;
        RaycastHit hit;
        if (Physics.Raycast(startPosition, -transform.up, out hit, Mathf.Infinity, CheckLayerMask.value, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(startPosition, -transform.up, Color.green, 2f);

            Debug.DrawLine(hit.point, hit.point + hit.normal, Color.red, 1f);
        }
    }
}
 