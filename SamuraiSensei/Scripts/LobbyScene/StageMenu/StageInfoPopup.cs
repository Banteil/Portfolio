using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StageInfoPopup : MonoBehaviour
{
    int worldIndex;
    int stageIndex;
    StageInfo info = new StageInfo();

    [SerializeField]
    Transform popupTr;
    public Transform PopupTr { get { return popupTr; } }
    [SerializeField]
    TextMeshProUGUI stageNumberText;
    [SerializeField]
    TextMeshProUGUI stageCombatPowerText;
    [SerializeField]
    TextMeshProUGUI goldNumberText;
    [SerializeField]
    TextMeshProUGUI gemNumberText;
    [SerializeField]
    GameObject gemXMark;
    [SerializeField]
    TextMeshProUGUI staminaCostText;
    [SerializeField]
    StageCharacterBox[] characterBoxes;
    public StageCharacterBox[] CharacterBoxes { get { return characterBoxes; } }

    StageCharacterBox saveBox;
    public StageCharacterBox SaveBox { get { return saveBox; } set { saveBox = value; } }

    private void OnEnable()
    {
        for (int i = 0; i < characterBoxes.Length; i++)
        {
            characterBoxes[i].CharacterID = GameManager.Instance.SelectCharacter[i];
        }
    }

    public void SetIndexInfo(int world, int stage)
    {
        worldIndex = world;
        stageIndex = stage;
        stageNumberText.text = (worldIndex + 1) + "-" + (stageIndex + 1);

        for (int i = 0; i < TableDatabase.Instance.StageTable.infoList.Count; i++)
        {
            int tempWorld = TableDatabase.Instance.StageTable.infoList[i].world;
            int tempStage = TableDatabase.Instance.StageTable.infoList[i].stage;
            if (tempWorld.Equals(worldIndex) && tempStage.Equals(stageIndex))
            {
                info = TableDatabase.Instance.StageTable.infoList[i];                
                break;
            }
        }

        for (int i = 0; i < TableDatabase.Instance.UserStageTable.infoList.Count; i++)
        {
            if(info.id.Equals(TableDatabase.Instance.UserStageTable.infoList[i].stageID))
            {
                if (TableDatabase.Instance.UserStageTable.infoList[i].isClear)
                    gemXMark.SetActive(true);
                else
                    gemXMark.SetActive(false);
                break;
            }
        }

        goldNumberText.text = info.rewardGold.ToString();
        gemNumberText.text = info.rewardGem.ToString();
        staminaCostText.text = info.staminaCost.ToString();
    }

    public void StageStart()
    {
        GameManager.Instance.Stamina -= info.staminaCost;
        GameManager.Instance.SelectStage = TableDatabase.Instance.StageTable.infoList[stageIndex];

        for (int i = 0; i < TableDatabase.Instance.UserStageTable.infoList.Count; i++)
        {
            if (TableDatabase.Instance.UserStageTable.infoList[i].stageID.Equals(GameManager.Instance.SelectStage.id)) break;

            if(i.Equals(TableDatabase.Instance.UserStageTable.infoList.Count - 1))
            {
                UserStageInfo stageInfo = new UserStageInfo();
                stageInfo.id = TableDatabase.Instance.UserStageTable.infoList.Count;
                stageInfo.userID = "tester";
                stageInfo.stageID = GameManager.Instance.SelectStage.id;
                stageInfo.isClear = false;
                TableDatabase.Instance.UserStageTable.infoList.Add(stageInfo);
            }
        }

        GameManager.Instance.CallLoadScene(SceneNumber.battle);
    }

    private void OnDisable()
    {
        gemXMark.SetActive(false);
    }
}
