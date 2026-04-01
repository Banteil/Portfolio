using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image HPGauge;

    public void UpdateBar(float currentHp, float maxHp)
    {
        HPGauge.fillAmount = currentHp / maxHp;
    }
}
