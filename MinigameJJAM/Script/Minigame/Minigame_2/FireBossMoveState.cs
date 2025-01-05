using UnityEngine;

namespace starinc.io
{
    public class FireBossMoveState : StateMachineBehaviour
    {
        private FireBossController _boss;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _boss ??= animator.GetComponent<FireBossController>();
            _boss.State = "Move";
            _boss.InitializeMoveState();
            Manager.Sound.PlaySFX("m2sfx_fire_move");
        }

        //public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
            
        //}

        //public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
           
        //}
    }
}
