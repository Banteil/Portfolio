using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playerble : Character
{
    [SerializeField]
    int id;

    [SerializeField]
    protected MyPlayerbleInfo info;
    public MyPlayerbleInfo Info
    {
        get { return info; }
        set
        {
            info = value;
            hp = info.info.maxHP;
            attackDelayTime = info.info.attackSpeed;
        }
    }

    //´É·ÂÄ¡ Get,Set
    protected float plusBuffAttackSpeed = 0f;
    public float PlusBuffAttackSpeed { set { plusBuffAttackSpeed = value; } }
    protected float multiplyBuffAttackSpeed = 1f;
    public float MultiplyBuffAttackSpeed { set { multiplyBuffAttackSpeed = value; } }
    public float AttackSpeed
    {
        get
        {
            float value = (info.info.attackSpeed + plusBuffAttackSpeed) * multiplyBuffAttackSpeed;
            return value;
        }
    }

    protected float plusBuffDamage = 0f;
    public float PlusBuffDamage { set { plusBuffDamage = value; } }
    protected float multiplyBuffDamage = 1f;
    public float MultiplyBuffDamage { set { multiplyBuffDamage = value; } }
    public float Damage
    {
        get
        {
            float value = (info.info.damage + plusBuffDamage) * multiplyBuffDamage;
            return value;
        }
    }

    void Awake()
    {
        aT = GetComponent<Animator>();
        sR = GetComponent<SpriteRenderer>();
        for (int i = 0; i < TableDatabase.Instance.UserPlayerbleTable.infoList.Count; i++)
        {
            if(TableDatabase.Instance.UserPlayerbleTable.infoList[i].characterID.Equals(id))
            {
                for (int j = 0; j < TableDatabase.Instance.PlayerbleTable.infoList.Count; j++)
                {
                    if(TableDatabase.Instance.PlayerbleTable.infoList[j].id.Equals(id))
                    {
                        MyPlayerbleInfo myInfo = new MyPlayerbleInfo();
                        myInfo.info = TableDatabase.Instance.PlayerbleTable.infoList[j];
                        myInfo.exp = TableDatabase.Instance.UserPlayerbleTable.infoList[i].exp;
                        myInfo.level = TableDatabase.Instance.UserPlayerbleTable.infoList[i].level;
                        myInfo.currentGrade = TableDatabase.Instance.UserPlayerbleTable.infoList[i].currentGrade;
                        Info = myInfo;
                        break;
                    }                    
                }                
                
                break;
            }
        }
        
    }

    void Update()
    {
        switch (state)
        {
            case CharacterState.IDLE:
                Idle();
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
        else
            state = CharacterState.ATTACK;
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
            float saveDis = 0f;
            int saveIndex = 0;
            bool checkSuccess = false;
            for (int j = 0; j < BattleManager.Instance.Enemies[checkOrder[i]].Count; j++)
            {
                float dis = Vector2.Distance(transform.position, BattleManager.Instance.Enemies[checkOrder[i]][j].transform.position);
                if (dis <= info.info.attackRange)
                {
                    if(j.Equals(0) || dis < saveDis)
                    {
                        saveDis = dis;
                        saveIndex = j;
                        checkSuccess = true;
                    }
                }                
            }

            if (checkSuccess)
            {
                target = BattleManager.Instance.Enemies[checkOrder[i]][saveIndex];
                break;
            }
        }
    }

    public override void Hit(float damage, ElementalProperties attackElement, float rigidTime, float knockBackPower)
    {
        if (state.Equals(CharacterState.DIE)) return;

        float result = BattleManager.Instance.DamageElementCalculation(info.info.propertie, attackElement, damage);
        hp -= result;
        DisplayDamage((int)result);
        BattleUIManager.Instance.PlayerHpInfoUpdate(this);
        if (hp <= 0)
        {
            state = CharacterState.DIE;
            Die();
        }
        else
        {
            state = CharacterState.HIT;
            if (!isFlash) StartCoroutine(Flash());
            StartCoroutine(RecoveryRigid(rigidTime));
        }
    }


    protected override void Attack()
    {
        if (target == null)
        {
            attackDelayTime = AttackSpeed;
            state = CharacterState.IDLE;
        }
        else
            TargetSearch();

        attackDelayTime += Time.deltaTime;
        if(attackDelayTime >= AttackSpeed)
        {
            if (target != null)
            {
                aT.SetTrigger("Attack");
                GameObject obj = Instantiate(BattleResourceManager.Instance.BulletObject, BattleManager.Instance.ObjectTr, false);
                Vector3 pos = transform.position;
                pos.y += sR.sprite.rect.height * 0.01f;
                obj.transform.position = pos;
                Bullet bullet = obj.GetComponent<Bullet>();
                bullet.SR.sprite = BattleResourceManager.Instance.BulletSprites[id];
                bullet.Damage = Damage;
                bullet.Propertie = info.info.propertie;
                bullet.Target = target.transform;
            }
            attackDelayTime = 0f;
        }
    }

    protected override void Die()
    {
        BattleManager.Instance.DeleteSkillDeck(id);
        BattleManager.Instance.StageFailCheck();
    }
}
