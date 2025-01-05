using UnityEngine;

namespace starinc.io
{
    public class CatMessUpChecker : StateMachineBehaviour
    {
        private bool _endState = false;
        private CatGame _catGame;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _endState = false;
            _catGame ??= FindAnyObjectByType<CatGame>();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_endState) return;
            if (stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(layerIndex))
            {
                animator.SetBool("IsMessUp", true);
                _catGame.OnMessUp();
                _endState = true;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(!_endState)
            {
                animator.SetBool("IsMessUp", true);
                _catGame.OnMessUp();
                _endState = true;
            }
        }
    }
}