using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementAbility))]
public class DashAbility : CharacterAbility
{
    public float DashSpeedScale = 1.5f;
    public float IdleThreshold = 0.05f;
    public ParticleSystem[] DashParticles;

    protected ButtonStates _dashButtonInput;
    protected ButtonStates _dashionButtonState;
    protected bool _particlePlaying;

    protected override void InternalHandleInput()
    {
        if (!InputManager.HasInstance) return;
        _dashButtonInput = InputManager.Instance.DashInput.ButtonState;
        base.InternalHandleInput();
    }

    protected override void HandleInput()
    {
        base.HandleInput();
        _dashionButtonState = _dashButtonInput;
    }

    public override void ProcessAbility()
    {
        Dashing();
        Feedback();
    }

    protected virtual void Dashing()
    {
        if (_dashionButtonState.Equals(ButtonStates.ButtonPressed))
        {
            _character.CurrentMovement *= DashSpeedScale;
        }
    }

    public override void Feedback()
    {
        if (_dashionButtonState.Equals(ButtonStates.ButtonPressed) && _character.VelocityMagnitude > IdleThreshold)
        {
            foreach (ParticleSystem system in DashParticles)
            {
                if (!_particlePlaying && (system != null))
                {
                    system.Play();
                }
                _particlePlaying = true;
            }
        }
        else
        {
            foreach (ParticleSystem system in DashParticles)
            {
                if (_particlePlaying && (system != null))
                {
                    system.Stop();
                    _particlePlaying = false;
                }
            }
        }
    }

    public override void UpdateAnimator()
    {
        
    }
}
