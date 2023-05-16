using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class ProjectileModelController : ThirdPersonController
    {
        public float ProjectileSpeed = 5f;
        private void FixedUpdate()
        {
            ControlLocomotionType();
        }
        public override void ControlLocomotionType()
        {
            Debug.Log("테스트");
            Vector3 dir = _rotateTarget.forward;
            MoveCharacter(dir);
            ControlRotationType();
        }

        public override void MoveCharacter(Vector3 direction)
        {

            InputSmooth = Vector3.Lerp(InputSmooth, _input, (IsStrafing ? StrafeSpeed.MovementSmooth : FreeSpeed.MovementSmooth)
                * (UseRootMotion ? GameTimeManager.Instance.DeltaTime : GameTimeManager.Instance.FixedDeltaTime));


            direction.y = 0;
            //magnitude : 벡터의 길이
            direction = direction.normalized * Mathf.Clamp(direction.magnitude, 0, 1f);
            Vector3 targetPosition = Rigidbody.position + direction *
                (ProjectileSpeed * SpeedMultiplier) * (UseRootMotion ? GameTimeManager.Instance.DeltaTime : GameTimeManager.Instance.FixedDeltaTime);
            Vector3 targetVelocity = (targetPosition - transform.position) / (UseRootMotion ? GameTimeManager.Instance.DeltaTime : GameTimeManager.Instance.FixedDeltaTime);
            Rigidbody.velocity = targetVelocity;
        }

        public override void ControlRotationType()
        {
            if (_rotateTarget == null)
            {
                _rotateTarget = Camera.main.transform;
            }
            Vector3 dir = _rotateTarget.forward;
            RotateToDirection(dir);
        }
    }
}