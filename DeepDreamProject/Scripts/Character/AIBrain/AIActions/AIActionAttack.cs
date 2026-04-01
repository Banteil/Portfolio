using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIActionAttack : AIAction
{
    protected AttackAbility _attackAbility;

    /// <summary>
    /// On init we grab our CharacterMovement ability
    /// </summary>
    public override void Initialization()
    {
        _attackAbility = _brain.Owner?.GetAbility<AttackAbility>();
        if (_attackAbility != null ) _attackAbility.ScriptDrivenInput = true;
    }

    /// <summary>
    /// On PerformAction we move
    /// </summary>
    public override void PerformAction()
    {
        Attack();
    }

    /// <summary>
    /// Moves the character towards the target if needed
    /// </summary>
    protected virtual void Attack()
    {
        if (_brain.Target == null)
        {
            return;
        }

        _attackAbility.ActiveAttack(true);
    }

    /// <summary>
    /// On exit state we stop our movement
    /// </summary>
    public override void OnExitState()
    {        
        base.OnExitState();
        _attackAbility.ActiveAttack(false);
    }
}
