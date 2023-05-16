using UnityEngine;

namespace Zeus
{
    public class FixedDirectionRotation : StateMachineBehaviour
    {
        public enum RotateDirection { Left, Right }
        public RotateDirection Direction;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var controlAI = animator.GetComponent<IControlAIZeus>();
            if (controlAI != null)
                controlAI.RotateDirection = Direction.Equals(RotateDirection.Left) ? -1 : 1;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var controlAI = animator.GetComponent<IControlAIZeus>();
            if (controlAI != null)
                controlAI.RotateDirection = 0;
        }
    }
}
