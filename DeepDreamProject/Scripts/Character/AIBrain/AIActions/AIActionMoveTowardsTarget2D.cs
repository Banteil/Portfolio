using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIActionMoveTowardsTarget2D : AIAction
{
    /// the minimum distance from the target this Character can reach.
    [Tooltip("the minimum distance from the target this Character can reach.")]
    public float MinimumDistance = 1f;

    protected MovementAbility _movementAbility;
    protected int _numberOfJumps = 0;

    /// <summary>
    /// On init we grab our CharacterMovement ability
    /// </summary>
    public override void Initialization()
    {
        _movementAbility = _brain.Owner?.GetAbility<MovementAbility>();
        _movementAbility.ScriptDrivenInput = true;
    }

    /// <summary>
    /// On PerformAction we move
    /// </summary>
    public override void PerformAction()
    {
        Move();
    }

    /// <summary>
    /// Moves the character towards the target if needed
    /// </summary>
    protected virtual void Move()
    {
        if (_brain.Target == null)
        {
            return;
        }

        if (this.transform.position.x < _brain.Target.position.x)
        {
            _movementAbility.HorizontalMovement = 1f;
        }
        else
        {
            _movementAbility.HorizontalMovement = -1f;
        }

        if (this.transform.position.y < _brain.Target.position.y)
        {
            _movementAbility.VerticalMovement = 1f;
        }
        else
        {
            _movementAbility.VerticalMovement = -1f;
        }

        if (Mathf.Abs(this.transform.position.x - _brain.Target.position.x) < MinimumDistance)
        {
            _movementAbility.HorizontalMovement = 0f;
        }

        if (Mathf.Abs(this.transform.position.y - _brain.Target.position.y) < MinimumDistance)
        {
            _movementAbility.VerticalMovement = 0f;
        }
    }

    /// <summary>
    /// On exit state we stop our movement
    /// </summary>
    public override void OnExitState()
    {
        base.OnExitState();

        _movementAbility.HorizontalMovement = 0f;
        _movementAbility.VerticalMovement = 0f;
    }
}
