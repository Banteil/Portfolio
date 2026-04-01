using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterHpUI : MonoBehaviour
{
    [SerializeField]
    Image icon;
    public Image Icon { get { return icon; } }

    [SerializeField]
    Image hpGauge;
    public Image HPGauge { get { return hpGauge; } }

    [SerializeField]
    TextMeshProUGUI hpText;
    public TextMeshProUGUI HPText { get { return hpText; } }

}
