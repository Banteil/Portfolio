using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEffect : MonoBehaviour
{
    SpriteRenderer sR;
    int direction = -1;

    // Start is called before the first frame update
    void Start()
    {
        sR = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        sR.color += new Color(0, 0, 0, Time.deltaTime * direction);
        if (sR.color.a <= 0 || sR.color.a >= 1) direction *= -1;
    }
}
