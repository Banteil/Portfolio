using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Portal : InteractionObject
{
    [SerializeField]
    int transferPlace;
    [SerializeField]
    Vector2 transferPos;

    void Start()
    {
        sR = GetComponent<SpriteRenderer>();
        aT = GetComponent<Animator>();
        sightAct = transform.GetChild(0).GetComponent<SightAct>();
        outline = GetComponent<SpriteOutline>();

        objectName = "Æ÷Å»";
        actDescription = "ÀÌµ¿";
    }

    public override void Interaction(CharacterBasic character)
    {
        if (transferPlace <= 1) return;
        else if (character.GetComponent<PlayerController>() == null) return;

        character.HitBox.InteractOff(this);
        GameManager.Instance.Player.InPortal(transform.position);
        GameManager.Instance.MoveOtherScene(transferPlace, transferPos);
    }

    public override void ObjectDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.gameObject.CompareTag("Player"))
                cH.InteractOn(this);
        }
    }

    public override void ObjectDetecting(GameObject detectionObj) { }

    public override void OutOfDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.gameObject.CompareTag("Player"))
            {
                cH.InteractOff(this);
                outline.enabled = false;
            }
        }
    }
}
