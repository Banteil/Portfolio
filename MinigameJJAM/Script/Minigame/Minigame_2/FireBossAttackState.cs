using UnityEngine;

namespace starinc.io
{
    public class FireBossAttackState : StateMachineBehaviour
    {
        private FireBossController _boss;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _boss ??= animator.GetComponent<FireBossController>();
            _boss.State = "Attack";
            _boss.AttackProcess();
        }

        //public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
            
        //}

        //public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
           
        //}
    }
}
