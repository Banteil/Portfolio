using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PauseUI : MonoBehaviour
{
    int restartCost;
    [SerializeField]
    TextMeshProUGUI restartStaminaText;

    [SerializeField]
    TopUI topUI;

    private void OnEnable()
    {
        topUI.gameObject.SetActive(true);
        restartCost = TableDatabase.Instance.StageTable.infoList[GameManager.Instance.SelectStage.stage].staminaCost;
        restartStaminaText.text = restartCost.ToString();
        Time.timeScale = 0f;
    }

    public void RestartStage()
    {
        gameObject.SetActive(false);
        if (GameManager.Instance.Stamina < restartCost) return;
        GameManager.Instance.Stamina -= restartCost;
        GameManager.Instance.CallLoadScene(SceneNumber.battle);
    }

    public void ReturnLobby()
    {
        gameObject.SetActive(false);
        GameManager.Instance.CallLoadScene(SceneNumber.lobby);
    }

    private void OnDisable()
    {
        topUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
