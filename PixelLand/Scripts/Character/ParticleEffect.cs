using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    [SerializeField]
    ParticleSystem[] pS;

    private void Update()
    {
        bool isAlive = false;
        for (int i = 0; i < pS.Length; i++)
        {
            isAlive = pS[i].IsAlive();
        }
        if(!isAlive)
            Destroy(gameObject);
    }
}
