using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character3D : Character
{
    protected override void PreInitialization()
    {
        base.PreInitialization();
        _rigidbody = GetComponentInChildren<Rigidbody>();
    }

    protected override void CharacterMovement()
    {
        _rigidbody.velocity = CurrentMovement + _impact;
    }
}
