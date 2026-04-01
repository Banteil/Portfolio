using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunDust : MonoBehaviour
{
    Vector3 direction;
    public Vector3 Direction { set { direction = value; if (direction.x > 0) GetComponent<SpriteRenderer>().flipX = true; } }
    float gravity = 0;

    private void Update()
    {
        gravity += 0.1f * Time.deltaTime;
        transform.position += direction * Time.deltaTime;
        transform.position += Vector3.up * gravity;
    }

    public void DestrySelf() => Destroy(gameObject);
}
