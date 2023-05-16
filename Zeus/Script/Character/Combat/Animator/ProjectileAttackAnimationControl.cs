using UnityEngine;

namespace Zeus
{
    public class ProjectileAttackAnimationControl : StateMachineBehaviour
    {
        //데미지판정이 들어가는 시간
        public float SpawnTime = 0.05f;
        public TypeGrapHand EquipPosition;
        private IAttackListener _fighter;
        private bool _isAttacking;
        private bool _isAttacked;


        public AttackInfo AttackInfo;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //데미지 주는 쪽 정보 가져오기
            _fighter = animator.GetComponent<IAttackListener>();
            _isAttacking = true;
            _isAttacked = false;
            if (_fighter != null)
            {
                //공격
                _fighter.OnEnableAttack();
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //데미지가 들어 가는 시간대
            if (stateInfo.normalizedTime % 1 >= SpawnTime! && !_isAttacked)
            {
                _isAttacked = true;
                SpawnProjectile(animator);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_isAttacking)
            {
                _isAttacking = false;
                if (_fighter != null)
                {
                    _fighter.OnDisableAttack();
                }
            }
        }

        //무기 콜라이더 활성화/비활성화
        protected void SpawnProjectile(Animator animator)
        {
            var combatManager = animator.GetComponent<CombatManager>();
            if (combatManager)
            {
                combatManager.ShootProjectile(AttackInfo);
                //무기 콜라이더 활성화 비활성화
            }
        }
    }
}


