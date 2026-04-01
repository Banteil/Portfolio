using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpUI : CharacterUI
{
    Image hpGauge;
    public Image HPGauge { get { return hpGauge; } }

    private void Awake()
    {
        hpGauge = transform.GetChild(0).GetComponent<Image>();
    }

    private void Update()
    {
        if(target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.transform.position + (Vector3.up * (target.SR.sprite.bounds.size.y));
        hpGauge.fillAmount = target.HP / target.Info.MaxHP;
    }
}
