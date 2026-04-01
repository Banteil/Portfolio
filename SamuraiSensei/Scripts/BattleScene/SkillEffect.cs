using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    [SerializeField]
    SkillType type;
    [SerializeField]
    ElementalProperties propertie;

    Skill skillInfo;
    public Skill SkillInfo 
    { 
        set 
        { 
            skillInfo = value;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<SkillEffect>() != null)
                    transform.GetChild(i).GetComponent<SkillEffect>().skillInfo = skillInfo;
            }
        } 
    }

    [SerializeField]
    bool isChild;

    public void UseSkill()
    {
        if (isChild) return;
        switch(type)
        {
            case SkillType.RECOVERY:
                for (int i = 0; i < BattleManager.Instance.Playerbles.Length; i++)
                {
                    BattleManager.Instance.Recovery(BattleManager.Instance.Playerbles[i], skillInfo);
                }
                break;
            case SkillType.BUFF:
                for (int i = 0; i < BattleManager.Instance.Playerbles.Length; i++)
                {
                    BattleManager.Instance.Buff(BattleManager.Instance.Playerbles[i], skillInfo);
                }                
                break;
            case SkillType.FUNCTIONAL:
                BattleManager.Instance.Functional(skillInfo);
                break;
        }
    }

    public void EndEffect()
    {
        if (isChild) return;
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(type.Equals(SkillType.ATTACK) || type.Equals(SkillType.DEBUFF))
        {
            if (collision.CompareTag("Enemy"))
            {
                float damage = BattleManager.Instance.GetSkillValue(skillInfo.formula, skillInfo);
                float rigidTime = BattleManager.Instance.GetSkillValue(skillInfo.rigidTime, skillInfo);
                float knockBackPower = BattleManager.Instance.GetSkillValue(skillInfo.knockBackPower, skillInfo);
                collision.GetComponent<Enemy>().Hit(damage, propertie, rigidTime, knockBackPower);
            }
        }
    }
}
