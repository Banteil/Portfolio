using System;
using UnityEngine;

namespace starinc.io.gallaryx
{
    public class BaseBrain
    {
        #region Cache
        protected CharacterController _character;
        #endregion

        public InputEvent BrainInputEvent = new InputEvent();

        public Action StoppingDistanceCallback;

        public bool IsRunning
        {
            get
            {
                if (!_character.HasAbility<MoveAbility>()) return false;
                var moveAbility = _character.GetAbility<MoveAbility>();                
                return moveAbility.IsRunning;
            }
        }

        public virtual bool IsJumping
        {
            get
            {
                if (!_character.HasAbility<JumpAbility>()) return false;
                var jumpAbility = _character.GetAbility<JumpAbility>();
                return !jumpAbility.IsGround;
            }
        }

        public virtual Vector3 TargetRotationValue { get; set; }

        public virtual Vector3 GoalLocation { get; set; }

        public virtual Vector3 GetMoveDirection { get; }

        public virtual bool ArrivalAtDestination { get; }

        public virtual void OnEnable(CharacterController characterController)
        {
            _character = characterController;
            SetInputEvent();
        }

        public virtual void OnUpdate() { }

        protected virtual void SetInputEvent() { }

        public virtual void OnDisable() { }
    }
}
