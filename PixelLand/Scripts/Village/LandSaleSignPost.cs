using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandSaleSignPost : InteractionObject
{
    PersonalSpace pS;

    private void Awake()
    {
        pS = transform.parent.GetComponent<PersonalSpace>();
        sightAct = transform.GetChild(0).GetComponent<SightAct>();
        outline = GetComponent<SpriteOutline>();

        objectName = "땅 매매 표지판";
        actDescription = "땅 구입";        
    }

    public override void Interaction(CharacterBasic character)
    {
        UIManager.Instance.GetUI("LogInfoUI").GetComponent<LogInfo>().DisplayLogInfo(pS.GetSpaceName() + "를 구매하였습니다!");
        selfSwitch = true;
        pS.OwnerID = character.ObjectID;
        UIManager.Instance.GetUI("ResidentInformation").GetComponent<ResidentInformation>().SetResidentNumber(pS.MyVillage.GetVillageNumber());
        UIManager.Instance.GetUI("ResidentInformation").GetComponent<ResidentInformation>().SetSpaceInfoText(pS.GetSpaceName());
        gameObject.SetActive(false);
    }

    public override void ObjectDetecting(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.CompareTag("Player"))
                cH.InteractOn(this);
        }
    }

    public override void ObjectDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.CompareTag("Player"))
                cH.InteractOn(this);
        }
    }

    public override void OutOfDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.CompareTag("Player"))
            {
                cH.InteractOff(this);
                outline.enabled = false;
            }
        }
    }
}
