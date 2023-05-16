using UnityEngine;

namespace Zeus
{
    public class ProjectileAttackAnimationControl : StateMachineBehaviour
    {
        //������������ ���� �ð�
        public float SpawnTime = 0.05f;
        public TypeGrapHand EquipPosition;
        private IAttackListener _fighter;
        private bool _isAttacking;
        private bool _isAttacked;


        public AttackInfo AttackInfo;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //������ �ִ� �� ���� ��������
            _fighter = animator.GetComponent<IAttackListener>();
            _isAttacking = true;
            _isAttacked = false;
            if (_fighter != null)
            {
                //����
                _fighter.OnEnableAttack();
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //�������� ��� ���� �ð���
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

        //���� �ݶ��̴� Ȱ��ȭ/��Ȱ��ȭ
        protected void SpawnProjectile(Animator animator)
        {
            var combatManager = animator.GetComponent<CombatManager>();
            if (combatManager)
            {
                combatManager.ShootProjectile(AttackInfo);
                //���� �ݶ��̴� Ȱ��ȭ ��Ȱ��ȭ
            }
        }
    }
}


