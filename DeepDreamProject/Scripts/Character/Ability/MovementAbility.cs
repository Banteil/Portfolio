using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbility : CharacterAbility
{
    public float MovementSpeed = 5f;

    protected float _horizontalInput;
    protected float _verticalInput;
    protected float _horizontalMovement;
    public float HorizontalMovement { set { _horizontalMovement = value; } }
    protected float _verticalMovement;
    public float VerticalMovement { set { _verticalMovement = value; } }

    protected override void InternalHandleInput()
    {
        if (!InputManager.HasInstance || ScriptDrivenInput) return;
        _horizontalInput = InputManager.Instance.MovementDirection.x;
        _verticalInput = InputManager.Instance.MovementDirection.y;
        base.InternalHandleInput();
    }

    protected override void HandleInput()
    {
        _horizontalMovement = _horizontalInput;
        _verticalMovement = _verticalInput;
    }

    public override void ProcessAbility()
    {
        SetMovement();
    }

    protected virtual void SetMovement()
    {
        Vector3 _movementVector = Vector3.zero;
        Vector2 _currentInput = Vector2.zero;
        _currentInput.x = _horizontalMovement;
        _currentInput.y = _verticalMovement;
        Vector2 _normalizedInput = _currentInput.normalized;
        float _movementSpeed = _characterStatExist ? _character.CharacterStat.MoveSpeed : MovementSpeed;

        _movementVector = _normalizedInput * _movementSpeed;
        _character.CurrentMovement = _movementVector;        
    }

    protected virtual Vector3 GetCharacterVelocity()
    {
        Vector3 velocity = Vector3.zero;
        if (_character.DimensionTypes.Equals(DimensionTypes.DIMENSION2D))
            velocity = _character.Rigidbody2D.velocity;
        else
            velocity = _character.Rigidbody.velocity;

        return velocity;
    }

    public override void UpdateAnimator()
    {
        Vector3 velocity = GetCharacterVelocity();
        float _movementSpeed = _characterStatExist ? _character.CharacterStat.MoveSpeed : MovementSpeed;
        _character.Animator.SetFloat("Velocity", velocity.magnitude / _movementSpeed);
    }
}
