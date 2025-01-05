using UnityEngine;

namespace starinc.io
{
    public class CatRestorationChecker : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.ResetTrigger("Restoration");
            animator.SetBool("IsMessUp", false);
        }
    }
}