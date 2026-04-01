using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TopUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI gemText;
    [SerializeField]
    TextMeshProUGUI staminaText;
    [SerializeField]
    TextMeshProUGUI staminaTimeText;
    [SerializeField]
    TextMeshProUGUI goldText;

    void Update()
    {
        gemText.text = GameManager.Instance.Gem.ToString();
        staminaText.text = GameManager.Instance.Stamina + "/" + GameManager.Instance.MaxStamina;
        if (GameManager.Instance.Stamina >= GameManager.Instance.MaxStamina)
            staminaTimeText.enabled = false;
        else
        {
            staminaTimeText.enabled = true;
            staminaTimeText.text = GameManager.Instance.MinuteCal.ToString("00") + ":" + GameManager.Instance.SecondCal.ToString("00");
        }
        goldText.text = GameManager.Instance.Gold.ToString();
    }
    
    public void OpenSettingUI()
    {
        Debug.Log("설정");
    }

    public void OpenEventUI()
    {
        Debug.Log("이벤트");
    }

    public void OpenMailUI()
    {
        Debug.Log("메일");
    }
}
