using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
{    
    int worldIndex;
    [SerializeField]
    int stageIndex;

    [SerializeField]
    Image buttonImage;
    public Image ButtonImage { get { return buttonImage; } }
    [SerializeField]
    TextMeshProUGUI stageNumberText;
    [SerializeField]
    bool isOpen;
    public bool IsOpen 
    { 
        set
        { 
            isOpen = value;
            if(isOpen)
                buttonImage.color = Color.white;
            else
                buttonImage.color = Color.gray;
        } 
    }
    public void SetIndexInfo(int world, int stage)
    {
        worldIndex = world;
        stageIndex = stage;
        stageNumberText.text = (worldIndex + 1) + "-" + (stageIndex + 1);
    }

    public void OpenStagePreparationUI()
    {
        if (!isOpen) return;

        LobbyStage.Instance.StageInfoPopup.SetIndexInfo(worldIndex, stageIndex);
        LobbyStage.Instance.StageInfoPopup.gameObject.SetActive(true);
    }
}
