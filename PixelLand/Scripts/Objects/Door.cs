using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractionObject
{
    public bool needKey;

    private void Start()
    {
        sR = GetComponent<SpriteRenderer>();
        aT = GetComponent<Animator>();
        sightAct = transform.GetChild(0).GetComponent<SightAct>();
        outline = GetComponent<SpriteOutline>();

        objectName = "문";
        actDescription = "문 열기";
    }

    public override void Interaction(CharacterBasic character)
    {
        if(selfSwitch) return;

        if (needKey)
        {
            for (int i = 0; i < character.Items.Belongings.Count; i++)
            {
                if (character.Items.Belongings[i].Info.ID.Equals("Key")) //열쇠가 있으면
                {
                    character.Items.Belongings[i] = null;
                    Open();
                }                    
            }            
        }
        else
        {
            Open();
        }
    }

    void Open()
    {
        selfSwitch = true;
        aT.SetTrigger("DoorOpen");
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
    public override void ObjectDetecting(GameObject detectionObj)
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
