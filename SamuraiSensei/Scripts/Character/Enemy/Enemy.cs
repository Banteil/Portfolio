using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType { MELEE, RANGED }

public class Enemy : Character
{
    protected const float meleeAttackRange = 5f;

    protected Rigidbody2D rB;

    [SerializeField]
    protected EnemyInfo info;
    public EnemyInfo Info
    {
        get { return info; }
        set
        {
            info = value;
            hp = info.maxHP;
            attackDelayTime = info.attackSpeed;
        }
    }

    [SerializeField]
    EnemyHpUI hpUI;
    [SerializeField]
    bool isBoss;

    void Awake()
    {
        aT = GetComponent<Animator>();
        sR = GetComponent<SpriteRenderer>();
        rB = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        switch (state)
        {
            case CharacterState.IDLE:
                Idle();
                break;
            case CharacterState.MOVE:
                Move();
                break;
            case CharacterState.ATTACK:
                Attack();
                break;
        }
    }

    protected override void Idle()
    {
        TargetSearch();
        if (target == null) return;

        state = CharacterState.MOVE;
    }

    protected override void TargetSearch()
    {
        target = null;

        List<int> checkOrder = new List<int>();
        checkOrder.Add(currentLine);
        switch (currentLine)
        {
            case 0:
                checkOrder.Add(1);
                checkOrder.Add(2);
                break;
            case 1:
                checkOrder.Add(0);
                checkOrder.Add(2);
                break;
            case 2:
                checkOrder.Add(1);
                checkOrder.Add(0);
                break;
        }

        for (int i = 0; i < checkOrder.Count; i++)
        {
            if (BattleManager.Instance.Playerbles[checkOrder[i]].State.Equals(CharacterState.DIE)) continue;
            target = BattleManager.Instance.Playerbles[checkOrder[i]];
            break;
        }
    }

    protected void Move()
    {
        if (target.State.Equals(CharacterState.DIE))
            TargetSearch();

        float dis = Vector2.Distance(transform.position, target.transform.position);
        float attackRange;

        if (info.attackType.Equals(AttackType.MELEE))
            attackRange = meleeAttackRange;
        else
            attackRange = info.attackRange;

        if (dis <= attackRange)
        {
            state = CharacterState.ATTACK;
            return;
        }

        transform.position -= Vector3.up * info.speed * Time.deltaTime;
    }

    public override void Hit(float damage, ElementalProperties attackElement, float rigidTime, float knockBackPower)
    {
        if (state.Equals(CharacterState.DIE)) return;

        float result = BattleManager.Instance.DamageElementCalculation(info.propertie, attackElement, damage);
        hp -= result;
        DisplayDamage((int)result);
        if (hp <= 0)
        {
            state = CharacterState.DIE;
            Die();
        }
        else
        {
            state = CharacterState.HIT;
            if(!isFlash) StartCoroutine(Flash());
            rB.AddForce(Vector2.up * knockBackPower);
            StartCoroutine(RecoveryRigid(rigidTime));
        }
    }

    protected override IEnumerator RecoveryRigid(float time)
    {
        yield return new WaitForSeconds(time);
        rB.velocity = Vector3.zero;
        state = CharacterState.IDLE;
    }

    protected override void Attack()
    {
        if (target.State.Equals(CharacterState.DIE))            
        {
            TargetSearch();
            attackDelayTime = info.attackSpeed;
        }

        attackDelayTime += Time.deltaTime;
        if (attackDelayTime >= info.attackSpeed)
        {
            if (target != null)
            {
                if (info.attackType.Equals(AttackType.MELEE))
                {
                    target.Hit(info.damage, info.propertie, 0, 0);
                }
                else
                {
                    //aT.SetTrigger("Attack");
                    GameObject obj = Instantiate(BattleResourceManager.Instance.BulletObject, BattleManager.Instance.ObjectTr, false);
                    obj.transform.position = transform.position;
                    Bullet bullet = obj.GetComponent<Bullet>();
                    bullet.Damage = info.damage;
                    bullet.Propertie = info.propertie;
                    bullet.Target = target.transform;
                }
            }
            attackDelayTime = 0f;
        }
    }

    protected override void Die()
    {
        BattleManager.Instance.Enemies[currentLine].Remove(this);
        BattleManager.Instance.StageClearCheck();
        Destroy(gameObject);
    }

    public void DisplayHpBar()
    {
        GameObject obj;
        obj = isBoss ? Instantiate(BattleResourceManager.Instance.BossHpBarObject, BattleUIManager.Instance.ObjectCanvasTr, false) : 
            Instantiate(BattleResourceManager.Instance.EnemyHpBarObject, BattleUIManager.Instance.ObjectCanvasTr, false);
        obj.transform.position = transform.position;
        hpUI = obj.GetComponent<EnemyHpUI>();
        hpUI.Target = transform;
        hpUI.SetBossInfo(info.id);        
    }
}
