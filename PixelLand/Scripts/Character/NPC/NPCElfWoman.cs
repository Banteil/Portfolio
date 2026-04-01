using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCElfWoman : NPCBasic
{
    static int maxSwitchNum = 1;

    private void Start()
    {
        rB = transform.GetComponent<Rigidbody2D>();
        sR = transform.GetChild(1).GetComponent<SpriteRenderer>();
        aT = transform.GetChild(1).GetComponent<Animator>();

        hitBox = transform.GetChild(1).GetComponent<CharacterHitBox>();
        handPos = hitBox.transform.GetChild(0);
        wC = handPos.GetChild(0).GetComponent<WeaponController>();

        for (int i = 0; i < CharacterManager.Instance.NPCInfo.Count; i++)
        {
            if (characterID.Equals(CharacterManager.Instance.NPCInfo[i].CharacterID))
            {
                Info = CharacterManager.Instance.NPCInfo[i];
                break;
            }
        }
        actDescription = "대화";
        rB.bodyType = RigidbodyType2D.Kinematic;

        if (interUI == null)
        {
            GameObject obj = Instantiate(ResourceManager.Instance.InterUI, MapManager.Instance.MapCanvas, false);
            interUI = obj.GetComponent<InteractionUI>();
            interUI.Target = this;
            Debug.Log("인터렉션 UI 생성! : " + gameObject.name);
        }
    }

    void Update()
    {
        switch (state)
        {
            case CharacterState.IDLE:
                Idle();
                break;
            case CharacterState.DIE:
                Die();
                break;
            default:
                break;
        }

    }

    void Idle()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        h = dir.x;
        v = dir.y;
        LookDirection();

        if (enemyHpUI != null)
        {
            Destroy(enemyHpUI.gameObject);
            enemyHpUI = null;
        }
    }

    void Die()
    {
        StopAllCoroutines();
        if (enemyHpUI != null)
            Destroy(enemyHpUI.gameObject);
        CharacterManager.Instance.NPCList.Remove(this);
        Destroy(gameObject);
    }

    public override void Interaction(CharacterBasic character)
    {
        if (GameManager.Instance.IsDirecting) return;

        character.HitBox.InteractOff(this);
        interUI.gameObject.SetActive(false);
        TalkNPC(objectName, switchNum);
        switchNum++;
        if (switchNum > maxSwitchNum) switchNum = maxSwitchNum;
    }


    public override void ObjectDetection(GameObject detectionObj)
    {
        if (GameManager.Instance.IsDirecting) return;

        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox hB = detectionObj.GetComponent<CharacterHitBox>();
            if (hB.CB.gameObject.CompareTag("Player"))
            {
                hB.InteractOn(this);
                interUI.gameObject.SetActive(true);
                target = hB.CB.transform;
            }
        }
    }

    public override void ObjectDetecting(GameObject detectionObj)
    {
        if (GameManager.Instance.IsDirecting || !state.Equals(CharacterState.IDLE)) return;

        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox hB = detectionObj.GetComponent<CharacterHitBox>();
            if (hB.CB.gameObject.CompareTag("Player"))
            {
                hB.InteractOn(this);
                interUI.gameObject.SetActive(true);
                target = hB.CB.transform;
            }
        }
    }

    public override void OutOfDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox hB = detectionObj.GetComponent<CharacterHitBox>();
            if (hB.CB.gameObject.CompareTag("Player"))
            {                
                hB.InteractOff(this);
                interUI.gameObject.SetActive(false);
                target = null;
                state = CharacterState.IDLE;
            }
        }
    }
}
