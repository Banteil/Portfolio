using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class AnimStateEvent : StateMachineBehaviour
    {
        [SerializeField] private AnimStateEventSO _stateEvent;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            _stateEvent?.Raise(TypeAnimState.ENTER);
        }
        //public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    base.OnStateUpdate(animator, stateInfo, layerIndex);

        //    _stateEvent?.Raise(TypeAnimState.UPDATE);
        //}
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            _stateEvent?.Raise(TypeAnimState.EXIT);
        }
    } 
}