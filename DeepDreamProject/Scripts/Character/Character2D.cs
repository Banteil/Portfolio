using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character2D : Character
{
    protected override void PreInitialization()
    {
        base.PreInitialization();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _rigidbody2D = GetComponentInChildren<Rigidbody2D>();
    }

    protected override void CharacterMovement()
    {
        if (_rigidbody2D.bodyType.Equals(RigidbodyType2D.Static)) return;

        if (CurrentMovement.x < 0) BindModel.localRotation = Quaternion.Euler(0f, 180f, 0f);
        else if (CurrentMovement.x > 0) BindModel.localRotation = Quaternion.identity;
        _rigidbody2D.velocity = CurrentMovement + _impact;
    }

    public void ReturnMaterial()
    {
        _renderer.material = DataManager.Instance.BasicMaterial;
    }
}
