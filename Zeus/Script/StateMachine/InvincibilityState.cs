using UnityEngine;

namespace Zeus
{
    public class InvincibilityState : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var health = animator.GetComponent<HealthController>();
            if (health != null)
                health.IsInvincibility = true;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var health = animator.GetComponent<HealthController>();
            if (health != null)
                health.IsInvincibility = false;
        }
    }
}
