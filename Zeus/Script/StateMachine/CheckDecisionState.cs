using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class CheckDecisionState : StateMachineBehaviour
    {
        public List<StateDecision> Decisions;

        IFSMBehaviourController _fsmController;
        ZeusAIController _zeusAI;
        bool _check;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _fsmController ??= animator.GetComponent<IFSMBehaviourController>();
            _zeusAI ??= animator.GetComponent<ZeusAIController>();

            if (_fsmController == null || _zeusAI == null) _check = true;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_check) return;
            if (_zeusAI.CurrentTarget == null || _zeusAI.CurrentTarget.IsDead)
            {
                animator.SetTrigger("ResetState");
                _check = true;
                return;
            }

            if (PossibleUsePattern())
            {
                animator.SetTrigger("CheckDecision");
                _check = true;
            }
        }

        protected bool PossibleUsePattern()
        {
            foreach (var decision in Decisions)
            {
                if (!decision.Decide(_fsmController))
                {
                    return false;
                }
            }
            return true;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _check = false;
        }
    }
}
