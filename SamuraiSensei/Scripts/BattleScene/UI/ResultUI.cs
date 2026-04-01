using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum RewardType { GOLD, GEM, ITEM }

public class ResultUI : MonoBehaviour
{
    [SerializeField]
    Image background;

    [SerializeField]
    ResultCharacterBox[] characterBoxes;
    [SerializeField]
    RewardItemBox[] rewardBoxes;

    int restartCost, nextCost;
    [SerializeField]
    TextMeshProUGUI restartStaminaText;
    [SerializeField]
    Button nextStageButton;
    [SerializeField]
    TextMeshProUGUI nextStageStaminaText;

    [SerializeField]
    TopUI topUI;

    public bool isDefeat;

    private void OnEnable()
    {
        topUI.gameObject.SetActive(true);
        //캐릭터 창 세팅
        for (int i = 0; i < characterBoxes.Length; i++)
        {
            characterBoxes[i].CharacterID = GameManager.Instance.SelectCharacter[i];
            for (int j = 0; j < TableDatabase.Instance.UserPlayerbleTable.infoList.Count; j++)
            {
                if(TableDatabase.Instance.UserPlayerbleTable.infoList[j].characterID.Equals(GameManager.Instance.SelectCharacter[i]))
                {
                    //첫 UI 세팅
                    characterBoxes[i].LevelText.text = "Lv " + TableDatabase.Instance.UserPlayerbleTable.infoList[j].level;
                    characterBoxes[i].EXPValueText.text = TableDatabase.Instance.UserPlayerbleTable.infoList[j].exp + " / " + ResourceManager.Instance.GetMaxExp(TableDatabase.Instance.UserPlayerbleTable.infoList[j].level);
                    characterBoxes[i].EXPGauge.fillAmount = TableDatabase.Instance.UserPlayerbleTable.infoList[j].exp / ResourceManager.Instance.GetMaxExp(TableDatabase.Instance.UserPlayerbleTable.infoList[j].level);
                    characterBoxes[i].FirstLevel = TableDatabase.Instance.UserPlayerbleTable.infoList[j].level;                    
                    characterBoxes[i].FirstEXP = TableDatabase.Instance.UserPlayerbleTable.infoList[j].exp;
                    break;
                }
            }
        }

        //스테이지 별 정보 세팅
        restartCost = TableDatabase.Instance.StageTable.infoList[GameManager.Instance.SelectStage.stage].staminaCost;
        restartStaminaText.text = restartCost.ToString();

        if(TableDatabase.Instance.StageTable.infoList.Count <= GameManager.Instance.SelectStage.stage + 1 || isDefeat)
        {
            nextCost = 0;
            nextStageButton.interactable = false;
            nextStageStaminaText.text = "X";
        }
        else
        {
            nextCost = TableDatabase.Instance.StageTable.infoList[GameManager.Instance.SelectStage.stage + 1].staminaCost;
            nextStageStaminaText.text = nextCost.ToString();
        }

        //졌을 때 처리
        if (isDefeat)
        {
            background.sprite = Resources.Load<Sprite>("Sprites/UI/Battle/Result/Result_Defeat");
            nextStageButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/UI/Battle/Result/Defeat_NextStage");
        }
        else
        {
            //보상 세팅
            rewardBoxes[0].SettingItemInfo(RewardType.GOLD, GameManager.Instance.SelectStage.rewardGold);
            GameManager.Instance.Gold += GameManager.Instance.SelectStage.rewardGold;

            for (int i = 0; i < TableDatabase.Instance.UserStageTable.infoList.Count; i++)
            {
                if (TableDatabase.Instance.UserStageTable.infoList[i].stageID.Equals(GameManager.Instance.SelectStage.id))
                {
                    if (!TableDatabase.Instance.UserStageTable.infoList[i].isClear)
                    {
                        rewardBoxes[1].SettingItemInfo(RewardType.GEM, GameManager.Instance.SelectStage.rewardGem);
                        GameManager.Instance.Gem += GameManager.Instance.SelectStage.rewardGem;
                    }
                    break;
                }
            }
            //아이템 추가되면 구현
            //rewardBoxes[2].SettingItemInfo(RewardType.ITEM, GameManager.Instance.SelectStage.rewardItemID);


            //참가 캐릭터 경험치 추가
            for (int i = 0; i < GameManager.Instance.SelectCharacter.Length; i++)
            {
                for (int j = 0; j < TableDatabase.Instance.UserPlayerbleTable.infoList.Count; j++)
                {
                    if (TableDatabase.Instance.UserPlayerbleTable.infoList[j].characterID.Equals(GameManager.Instance.SelectCharacter[i]))
                    {
                        //실제 정보 세팅
                        TableDatabase.Instance.UserPlayerbleTable.infoList[j].exp += GameManager.Instance.SelectStage.exp;
                        if (TableDatabase.Instance.UserPlayerbleTable.infoList[j].exp >= ResourceManager.Instance.GetMaxExp(TableDatabase.Instance.UserPlayerbleTable.infoList[j].level))
                        {
                            float leftover = TableDatabase.Instance.UserPlayerbleTable.infoList[j].exp - ResourceManager.Instance.GetMaxExp(TableDatabase.Instance.UserPlayerbleTable.infoList[j].level);
                            TableDatabase.Instance.UserPlayerbleTable.infoList[j].level++;
                            TableDatabase.Instance.UserPlayerbleTable.infoList[j].exp = leftover;
                        }
                        characterBoxes[i].ResultLevel = TableDatabase.Instance.UserPlayerbleTable.infoList[j].level;
                        characterBoxes[i].ResultEXP = TableDatabase.Instance.UserPlayerbleTable.infoList[j].exp;
                        break;
                    }
                }
                characterBoxes[i].EXPIncreaseDirection();
            }
            
            for (int i = 0; i < TableDatabase.Instance.UserStageTable.infoList.Count; i++)
            {
                if(TableDatabase.Instance.UserStageTable.infoList[i].stageID.Equals(GameManager.Instance.SelectStage.id))
                {
                    TableDatabase.Instance.UserStageTable.infoList[i].isClear = true;
                    break;
                }
            }           
        }
    }

    public void Restart()
    {
        if (GameManager.Instance.Stamina < restartCost) return;
        GameManager.Instance.Stamina -= restartCost;
        GameManager.Instance.CallLoadScene(SceneNumber.battle);
    }

    public void NextStage()
    {
        if (GameManager.Instance.Stamina < nextCost) return;
        GameManager.Instance.Stamina -= nextCost;
        GameManager.Instance.SelectStage = TableDatabase.Instance.StageTable.infoList[GameManager.Instance.SelectStage.stage + 1];
        GameManager.Instance.CallLoadScene(SceneNumber.battle);
    }

    public void ReturnLobby()
    {
        GameManager.Instance.CallLoadScene(SceneNumber.lobby);
    }
}
