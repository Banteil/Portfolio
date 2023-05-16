using UnityEngine;

namespace Zeus
{
    public class ActiveDissolve : StateMachineBehaviour
    {
        public enum DissolveState { Dissolve, Materialize }
        public DissolveState State;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var dissolver = animator.GetComponent<Dissolver>();
            if (dissolver != null)
            {
                dissolver.MaterializeDissolve(State);
            }
        }
    }
}
