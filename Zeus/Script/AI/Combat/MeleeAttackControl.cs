using UnityEngine;

namespace Zeus
{
    public class MeleeAttackControl : AttackControlState
    {
        public AttackInfo[] AttackInfos = new AttackInfo[1];

        public override AttackInfo[] AttackInfoArray { get { return AttackInfos; } }

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _mFighter = animator.GetComponent<IAttackListener>();
            _isAttacking = true;
            if (_mFighter != null)
                _mFighter.OnEnableAttack();
            _currentIndex = 0;
            if(DisplaySignBeforeAttack) ActiveAttackSign(animator, AttackInfos[0]);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (AttackInfos.Length <= 0) return;

            if (stateInfo.normalizedTime % 1 >= AttackInfos[_currentIndex].StartDamage && stateInfo.normalizedTime % 1 <= AttackInfos[_currentIndex].EndDamage && !_isActive)
            {
                _isActive = true;
                ActiveDamage(animator, true, AttackInfos[_currentIndex]);
                if(AttackInfos[_currentIndex].DisplaySignDuringAttack) ActiveAttackSign(animator, AttackInfos[_currentIndex]);
            }
            else if (stateInfo.normalizedTime % 1 > AttackInfos[_currentIndex].EndDamage && _isActive)
            {
                _isActive = false;
                ActiveDamage(animator, false, AttackInfos[_currentIndex]);
                if(_currentIndex < AttackInfos.Length - 1)
                    _currentIndex++;
            }

            if (stateInfo.normalizedTime % 1 > AttackInfos[AttackInfos.Length - 1].EndDamage && _isAttacking)
            {
                _isAttacking = false;
                if (_mFighter != null)
                    _mFighter.OnDisableAttack();
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_isActive)
            {
                _isActive = false;
                ActiveDamage(animator, false, AttackInfos[AttackInfos.Length - 1]);
            }

            if (_isAttacking)
            {
                _isAttacking = false;
                if (_mFighter != null)
                    _mFighter.OnDisableAttack();
            }

            if (_mFighter != null && ResetAttackTrigger)
                _mFighter.ResetAttackTriggers();
        }

        void ActiveDamage(Animator animator, bool colliderActive, AttackInfo attackInfo)
        {
            var combatManager = animator.GetComponent<CombatManager>();
            if (combatManager)
            {
                combatManager.SetActiveAttack(attackInfo, colliderActive);
            }
        }
    }
}