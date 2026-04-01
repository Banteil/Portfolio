using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationMovementAbility : MovementAbility
{
    public float Acceleration = 0.1f;
    protected Vector3 _movementVector;

    protected override void SetMovement()
    {
        Vector2 currentInput = Vector2.zero;
        currentInput.x = _horizontalMovement;
        currentInput.y = _verticalMovement;
        Vector2 normalizedInput = currentInput.normalized;

        _movementVector = (Vector3)(normalizedInput * Acceleration);
        _character.CurrentMovement += _movementVector;

        float movementSpeed = _characterStatExist ? _character.CharacterStat.MoveSpeed : MovementSpeed;
        if (_character.CurrentMovement.magnitude > movementSpeed)
        {
            _character.CurrentMovement = _character.CurrentMovement.normalized * movementSpeed;
        }
    }
}
