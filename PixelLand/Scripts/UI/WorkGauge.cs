using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkGauge : CharacterUI
{
    Image gaugeImage;
    public Image GaugeImage { get { return gaugeImage; } }

    private void Awake()
    {
        gaugeImage = GetComponent<Image>();
    }

    void Update()
    {
        if (target == null) return;

        transform.position = target.transform.position - ((Vector3.up * (target.SR.sprite.rect.height / 16f) / 4f));
    }
}
