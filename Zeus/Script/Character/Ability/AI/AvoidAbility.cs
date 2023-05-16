using UnityEngine;

namespace Zeus
{
    public class AvoidAbility : AIOnlyAbility
    {
        public float AvoidChance = 0f;

        [Range(0f, 100f)]
        [SerializeField]
        private AnimatorParameter _avoidTriggerHash;

        protected override void Initialize()
        {
            base.Initialize();
            if (_owner.Animator != null)
            {
                _avoidTriggerHash = new AnimatorParameter(_owner.Animator, "Avoid");
            }
            _zeusAI.CallTakeDamageAction += TryAvoid;
        }

        protected virtual bool TryAvoid()
        {
            if (!(Random.Range(0f, 100f) <= AvoidChance)) return false;
            Avoid();
            return true;
        }

        public virtual void Avoid()
        {
            var rotDir = (_zeusAI.CurrentTarget.Transform.position - transform.position).normalized;
            rotDir.y = 0;
            transform.rotation = Quaternion.LookRotation(rotDir);
            _zeusAI.Animator.SetTrigger("ResetState");
            _zeusAI.Animator.CrossFadeInFixedTime(_avoidTriggerHash, 0.01f);
        }
    }
}
