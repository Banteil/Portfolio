using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SpearSkeleton : MonsterBasic
{
    public float attackDelay = 0.6f;

    public List<GameObject> monsters = new List<GameObject>();

    void Start()
    {
        rB = GetComponent<Rigidbody2D>();
        aT = transform.GetChild(1).GetComponent<Animator>();
        sR = transform.GetChild(1).GetComponent<SpriteRenderer>();
        hitBox = transform.GetChild(1).GetComponent<CharacterHitBox>();

        bodyCollider = GetComponent<CircleCollider2D>();
        attackBox = transform.GetChild(2).GetComponent<MonsterAttackBox>();
        desnination = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();

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

    void Update()
    {
        desnination.target = target;
        if (target != null)
        {
            Vector2 dir = (aiPath.destination - transform.position).normalized;
            h = dir.x;
            v = dir.y;
        }

        switch (state)
        {
            case CharacterState.IDLE:
                Idle();
                break;
            case CharacterState.TRACKING:
                Tracking();
                break;
            case CharacterState.ATTACK:
                Attack();
                break;
            case CharacterState.HIT:
                aiPath.canMove = false;
                break;
        }
    }

    protected override void Idle()
    {
        sight = 6f;
        aiPath.canMove = false;
        rB.velocity = Vector2.zero;
        aT.SetBool("Run", false);
    }

    protected override void Tracking()
    {
        sight = 8f;
        aiPath.canMove = true;
        aT.SetBool("Run", true);

        if (target != null)
        {
            LookDirection();

            float dis = (target.position - transform.position).magnitude;
            //float yDis = Mathf.Abs(target.position.y - (transform.position.y-0.5f));
            if (dis < Info.AttackRange - 0.5f)
                state = CharacterState.ATTACK;
        }
        else
            state = CharacterState.IDLE;
    }

    protected override void LookDirection()
    {
        if (Mathf.Abs(target.position.x - transform.position.x) < GetComponent<CircleCollider2D>().bounds.size.x / 2)
        {
            if (target.position.y > transform.position.y)
            {
                attackBox.transform.localScale = new Vector3(1, 1, 1);
                attackBox.transform.eulerAngles = new Vector3(0, 0, 90);
            }
            else
            {
                attackBox.transform.localScale = new Vector3(1, 1, 1);
                attackBox.transform.eulerAngles = new Vector3(0, 0, -90);
            }
        }
        else
        {
            if (target.transform.position.x > transform.position.x)
            {
                sR.flipX = false;
                attackBox.transform.localScale = new Vector3(1, 1, 1);
                attackBox.transform.eulerAngles = new Vector3(0, 0, 0);
            }
            else
            {
                sR.flipX = true;
                attackBox.transform.localScale = new Vector3(-1, 1, 1);
                attackBox.transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
        //if (target.transform.position.x < transform.position.x)
        //{
        //    sR.flipX = true;
        //    attackBox.transform.localScale = new Vector3(-1, 1, 1);
        //}
        //else
        //{
        //    sR.flipX = false;
        //    attackBox.transform.localScale = new Vector3(1, 1, 1);
        //}
    }

    protected override void Attack()
    {
        aiPath.canMove = false;
        if (process == null)
            process = StartCoroutine(AttackProcess());
    }

    IEnumerator AttackProcess()
    {
        rB.isKinematic = true;
        rB.velocity = Vector2.zero;
        aT.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        attackBox.AttackBoxCollider.enabled = true;

        yield return new WaitForSeconds(0.2f);

        attackBox.AttackBoxCollider.enabled = false;

        yield return new WaitForSeconds(0.2f);

        rB.isKinematic = false;
        aiPath.canMove = true;
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
        bodyCollider.enabled = false;
        attackBox.enabled = false;
        rB.velocity = Vector2.zero;

        ItemManager.Instance.DropOfItem(transform.position, _dropInfo, 5);
        CharacterManager.Instance.MonsterList.Remove(this);

        if (DungeonManager.Instance != null)
            DungeonManager.Instance.MonsterCountCheck();
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
                rB.WakeUp();
                for (int i = 0; i < monsters.Count; i++)
                {
                    if (monsters[i] == null) continue;
                    monsters[i].GetComponent<SpearSkeleton>().target = target;
                    monsters[i].GetComponent<SpearSkeleton>().state = CharacterState.TRACKING;
                    monsters[i].GetComponent<SpearSkeleton>().DisplayEnemyHp();
                    monsters[i].GetComponent<SpearSkeleton>().rB.WakeUp();
                }

            }
            else if (cH.CB.transform.CompareTag("Monster"))
            {
                if (cH.CB.transform.gameObject != gameObject)
                    monsters.Add(detectionObj.transform.parent.gameObject);
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
        if (detectionObj.CompareTag("Character"))
        {
            if (detectionObj.transform.CompareTag("Monster"))
            {
                if (detectionObj != this.gameObject)
                {
                    monsters.Remove(detectionObj.transform.parent.gameObject);
                }
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            rB.velocity = Vector2.zero;
        }
    }

}
