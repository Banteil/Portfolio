using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    ParticleSystem pS;
    ParticleSystemRenderer pSR;
    [SerializeField]
    SpriteRenderer sR;
    [SerializeField]
    Color color;

    private void Awake()
    {
        pS = GetComponent<ParticleSystem>();
        pSR = GetComponent<ParticleSystemRenderer>();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        var main = pS.main;
        main.startColor = color;
        pSR.material.mainTexture = sR.sprite.texture;
    }
}
