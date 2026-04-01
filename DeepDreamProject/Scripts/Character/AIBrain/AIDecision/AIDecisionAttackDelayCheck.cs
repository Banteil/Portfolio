using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDecisionAttackDelayCheck : AIDecision
{
    protected AttackAbility _attackAbility;

    public override void Initialization()
    {
        _attackAbility = _brain.Owner.GetAbility<AttackAbility>();
    }

    public override bool Decide()
    {
        return CheckDelay();
    }

    /// <summary>
    /// Owner의 공격 딜레이가 종료된 시점을 true로 체크함
    /// </summary>
    /// <returns></returns>
    protected virtual bool CheckDelay()
    {
        if (_brain.Target == null || _attackAbility == null)
        {
            return false;
        }

        return _attackAbility.State.Equals(AttackState.ENDATTACK);
    }
}
