using UnityEngine;

namespace Zeus
{
    public class RandomAttackBehaviour : StateMachineBehaviour
    {
        public int AttackCount;

        //OnStateMachineEnter is called when entering a statemachine via its Entry Node
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetInteger("RandomAttack", Random.Range(0, AttackCount));
        }
    }
}