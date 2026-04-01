using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public abstract class MonsterBasic : CharacterBasic
{
    protected CircleCollider2D bodyCollider;
    protected MonsterAttackBox attackBox;

    protected AIPath aiPath;
    public AIPath AI { get { return aiPath; } }
    protected AIDestinationSetter desnination;

    [SerializeField]
    protected MyItemDropInfo _dropInfo;
    public MyItemDropInfo DropInfo { get { return _dropInfo; } }

    abstract protected void Idle();
    abstract protected void Tracking();
    abstract protected void Attack();
    abstract protected void Die();

    public override void HitHandling(float damage, int knockbackPower, Vector3 attackerPos)
    {
        if (state != CharacterState.DIE)
        {
            Vector3 hitbackDir = (transform.position - attackerPos).normalized;
            rB.velocity = Vector2.zero;
            rB.AddForce(hitbackDir * knockbackPower);
        }
        else
            Die();
    }

    protected override void Awake()
    {
        base.Awake();

        aiPath = GetComponent<AIPath>();
        desnination = GetComponent<AIDestinationSetter>();
        bodyCollider = GetComponent<CircleCollider2D>();
    }
    public Vector2 GetTargetVector()
    {
        return target.position - transform.position;
    }

    public float GetTargetDistance()
    {
        return GetTargetVector().magnitude;
    }


    public Vector2 GetTargetDirection()
    {
        return GetTargetVector().normalized;
    }

    public override Transform Detection()
    {
        return desnination.target = target = base.Detection();
    }

    public void OnOffAfterEffect(bool enabled)
    {
        afterImage.gameObject.SetActive(enabled);
        attackBox.AttackBoxCollider.enabled = enabled;
    }
}
