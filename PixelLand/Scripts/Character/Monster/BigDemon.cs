using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BigDemon : MonsterBasic
{
    protected override void Awake()
    {
        base.Awake();

        aT = transform.GetChild(1).GetComponent<Animator>();
        sR = transform.GetChild(1).GetComponent<SpriteRenderer>();
        hitBox = transform.GetChild(1).GetComponent<CharacterHitBox>();
        attackBox = transform.GetChild(2).GetComponent<MonsterAttackBox>();
        afterImage = transform.GetChild(3).GetComponent<AfterImage>();
    }

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < CharacterManager.Instance.MonsterInfo.Count; i++)
        {
            if (characterID.Equals(CharacterManager.Instance.MonsterInfo[i].CharacterID))
            {
                Info = CharacterManager.Instance.MonsterInfo[i];
                break;
            }
        }
        aiPath.maxSpeed = Info.Speed;
    }

    Coroutine process = null;

    protected override void Update()
    {
        base.Update();

        if (state.Equals(CharacterState.DIE)) return;

        desnination.target = target;
        
        if (target != null)
        {
            Vector2 dir = (aiPath.destination - transform.position).normalized;
            if (dir.x > 0) h = 1;
            else if (dir.x < 0) h = -1;
            if (dir.y > 0) v = 1;
            else if (dir.y < 0) v = -1;
            LookDirection();
        }

        //switch (state)
        //{
        //    case CharacterState.IDLE:
        //        Idle();
        //        break;
        //    case CharacterState.TRACKING:
        //        Tracking();
        //        break;
        //    case CharacterState.ATTACK:
        //        Attack();
        //        break;
        //    case CharacterState.HIT:
        //        aiPath.canMove = false;
        //        break;
        //}
    }

    protected override void LookDirection()
    {
        if ((int)h > 0)
            transform.eulerAngles = Vector3.zero;
        else if ((int)h < 0)
            transform.eulerAngles = new Vector3(0, 180f);
    }

    protected override void Idle()
    {
        sight = 6f;
        aiPath.canMove = false;
        rB.velocity = Vector2.zero;
    }

    protected override void Tracking()
    {
        sight = 8f;
        aiPath.canMove = true;

        if (target.gameObject != null)
        {
            float dis = (target.position - transform.position).magnitude;
            if (dis < Info.AttackRange)
                state = CharacterState.ATTACK;
        }
        else
            state = CharacterState.IDLE;
    }

    protected override void Attack()
    {
        if(target.gameObject == null)
        {
            state = CharacterState.IDLE;
            return;
        }

        aiPath.canMove = false;
        if (process == null)
            process = StartCoroutine(AttackProcess());
    }

    IEnumerator AttackProcess()
    {
        Vector2 dir = (desnination.target.position - transform.position).normalized;
        rB.velocity = Vector2.zero;
        aT.speed = 0;
        
        yield return new WaitForSeconds(0.3f);

        afterImage.gameObject.SetActive(true);
        attackBox.AttackBoxCollider.enabled = true;
        aT.speed = 1;    
        rB.velocity = dir * (Info.Speed * 2);

        yield return new WaitForSeconds(0.5f);

        afterImage.gameObject.SetActive(false);
        aiPath.canMove = true;
        attackBox.AttackBoxCollider.enabled = false;
        rB.velocity = Vector2.zero;
        state = CharacterState.TRACKING;
        process = null;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        aiPath.canMove = false;
        if (enemyHpUI != null)
            Destroy(enemyHpUI.gameObject);
        aT.speed = 1;
        aT.SetTrigger("IsDie");
        afterImage.gameObject.SetActive(false);
        bodyCollider.enabled = false;
        attackBox.enabled = false;
        rB.velocity = Vector2.zero;

        ItemManager.Instance.DropOfItem(transform.position, _dropInfo, 5);
        CharacterManager.Instance.MonsterList.Remove(this);
    }

    public override void ObjectDetection(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.transform.CompareTag("Player") || cH.CB.transform.CompareTag("NPC"))
            {
                if (target == null)
                    target = cH.CB.transform;

                state = CharacterState.TRACKING;
                DisplayEnemyHp();
            }
        }
    }

    public override void ObjectDetecting(GameObject detectionObj)
    {
        if (detectionObj.CompareTag("Character"))
        {
            CharacterHitBox cH = detectionObj.GetComponent<CharacterHitBox>();
            if (cH.CB.transform.CompareTag("Player") || cH.CB.transform.CompareTag("NPC"))
            {
                if (target == null)
                    target = cH.CB.transform;
            }
        }
    }

    public override void OutOfDetection(GameObject detectionObj)
    {

    }
}
