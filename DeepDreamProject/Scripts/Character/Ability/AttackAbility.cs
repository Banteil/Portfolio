using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackState { NONE, STARTATTACK, ATTACKING, ENDATTACK }

public class AttackAbility : CharacterAbility
{
    public AttackState State;
    public float AttackSpeed = 1;

    public MMFeedbacks StartAttackFeedback;
    public MMFeedbacks EndAttackFeedback;

    protected float _attackDelay;
    protected float _timer = 0f;

    protected ButtonStates _attackButtonInput;
    protected ButtonStates _attackButtonState;

    protected ItemHandleAbility _itemHandleAbility;
    protected bool UseItem { get { return _itemHandleAbility != null; } }

    protected override void Initialization()
    {
        _itemHandleAbility = _character.GetAbility<ItemHandleAbility>();
        base.Initialization();
    }

    protected override void InternalHandleInput()
    {
        if (!InputManager.HasInstance || ScriptDrivenInput) return;
        _attackButtonInput = InputManager.Instance.FireInput.ButtonState;
        base.InternalHandleInput();
    }

    protected override void HandleInput()
    {
        _attackButtonState = _attackButtonInput;
    }

    public virtual void ActiveAttack(bool active)
    {
        _attackButtonState = active ? ButtonStates.ButtonPressed : ButtonStates.Off;
    }

    public override void ProcessAbility()
    {
        if (UseItem)
            if (_itemHandleAbility.HandleItem == null) return;

        switch (State)
        {
            case AttackState.NONE:
                CheckInput();
                break;
            case AttackState.STARTATTACK:
                StartAttack();
                break;
            case AttackState.ATTACKING:
                Attacking();
                break;
            case AttackState.ENDATTACK:
                EndAttack();
                break;
        }
    }

    protected virtual void CheckInput()
    {
        if (_attackButtonState.Equals(ButtonStates.ButtonPressed))
        {
            if (State.Equals(AttackState.NONE))
            {
                State = AttackState.STARTATTACK;
            }
        }
    }

    protected virtual void StartAttack()
    {
        _character.State = CharacterStates.ACT;
        _attackDelay = AttackDelayCalculation(_character.CharacterStat.AgilityStat.Value, UseItem ? _itemHandleAbility.HandleItem.Data.AttackSpeed : AttackSpeed);        
        State = AttackState.ATTACKING;

        if (UseItem)
        {
            _itemHandleAbility.HandleItem?.Animator.SetFloat("AttackSpeed", 1 / _attackDelay);
            _itemHandleAbility.HandleItem?.Animator.SetTrigger("Attack");
        }
        else
        {
            _character.Animator.SetFloat("AttackSpeed", 1 / _attackDelay);
            _character.Animator.SetTrigger("Attack");
        }

        StartAttackFeedback?.PlayFeedbacks(transform.position);
    }

    protected virtual void Attacking()
    {
        _timer += Time.deltaTime;
        if (_timer >= _attackDelay)
        {
            _timer = 0f;
            State = AttackState.ENDATTACK;
        }
    }

    protected virtual void EndAttack()
    {
        _character.State = CharacterStates.IDLE;
        State = AttackState.NONE;
        EndAttackFeedback?.PlayFeedbacks(transform.position);
    }

    protected virtual float AttackDelayCalculation(float agility, float attackSpeed)
    {
        float delay = 2.5f / (agility * attackSpeed);
        return delay;
    }
}
