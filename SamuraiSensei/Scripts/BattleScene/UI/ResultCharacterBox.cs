using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultCharacterBox : MonoBehaviour
{
    [SerializeField]
    Image backgroundImage, characterStandingImage;
    [SerializeField]
    Image expGauge;
    public Image EXPGauge { get { return expGauge; } }

    [SerializeField]
    TextMeshProUGUI levelText;
    public TextMeshProUGUI LevelText { get { return levelText; } }
    [SerializeField]
    TextMeshProUGUI expValueText;
    public TextMeshProUGUI EXPValueText {  get { return expValueText; } }

    int firstLevel, resultLevel;
    public int FirstLevel { set { firstLevel = value; } }
    public int ResultLevel { set { resultLevel = value; } }

    float firstEXP, resultEXP;
    public float FirstEXP { set { firstEXP = value; } }
    public float ResultEXP { set { resultEXP = value; } }

    int characterID;
    public int CharacterID
    {
        get { return characterID; }
        set
        {
            characterID = value;
            backgroundImage.sprite = ResourceManager.Instance.CharacterBoxBackgroundSprite[characterID];
            characterStandingImage.sprite = ResourceManager.Instance.CharacterBoxStandingSprite[characterID];
        }
    }

    public void EXPIncreaseDirection()
    {
        StartCoroutine(IncreaseProcess());
    }

    IEnumerator IncreaseProcess()
    {
        float currentEXP = firstEXP;
        int currentLevel = firstLevel;
        while (true)
        {
            if (currentLevel.Equals(resultLevel) && currentEXP >= resultEXP)
            {
                expGauge.fillAmount = resultEXP / ResourceManager.Instance.GetMaxExp(resultLevel);
                expValueText.text = resultEXP.ToString("0") + " / " + ResourceManager.Instance.GetMaxExp(resultLevel).ToString("0");
                break;
            }

            currentEXP += 20f * Time.deltaTime;
            expGauge.fillAmount = currentEXP / ResourceManager.Instance.GetMaxExp(currentLevel);
            expValueText.text = currentEXP.ToString("0") + " / " + ResourceManager.Instance.GetMaxExp(currentLevel).ToString("0");

            if(currentEXP >= ResourceManager.Instance.GetMaxExp(currentLevel))
            {
                currentLevel++;
                currentEXP = 0;
            }
            yield return null;
        }
    }
}
