using UnityEngine;

namespace Zeus
{
    public class EffectAttackInfo : AttackInfo
    {
        [Header("Effect Attack Info")]
        public HumanBodyBones InstantiatePoint = HumanBodyBones.LeftHand;
        public ProjectileAttackObject AttackEffect;
    }

    public class EffectAttackControl : AttackControlState
    {
        public EffectAttackInfo[] AttackInfos;
        public override AttackInfo[] AttackInfoArray { get { return AttackInfos; } }

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _mFighter = animator.GetComponent<IAttackListener>();
            _isAttacking = true;
            if (_mFighter != null)
                _mFighter.OnEnableAttack();
            
            _currentIndex = 0;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.normalizedTime % 1 >= AttackInfos[_currentIndex].StartDamage && !_isActive)
            {                
                _isActive = true;
                ActiveEffect(animator, AttackInfos[_currentIndex]);
            }
            else if (stateInfo.normalizedTime % 1 > AttackInfos[_currentIndex].EndDamage && _isActive)
            {
                _isActive = false;
                if (_currentIndex < AttackInfos.Length - 1) _currentIndex++;
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
                _isActive = false;

            if (_isAttacking)
            {
                _isAttacking = false;
                if (_mFighter != null)
                    _mFighter.OnDisableAttack();
            }
            if (_mFighter != null && ResetAttackTrigger)
                _mFighter.ResetAttackTriggers();
        }

        void ActiveEffect(Animator animator, EffectAttackInfo attackInfo)
        {
            var projectile = Instantiate(attackInfo.AttackEffect.gameObject).GetComponent<ProjectileAttackObject>();
            projectile.CombatManager = animator.GetComponent<CombatManager>();
            if (projectile.CombatManager != null)
                projectile.Target = projectile.CombatManager.GetComponent<ControlAI>().CurrentTarget.Transform;

            if (animator.isHuman)
                projectile.transform.position = animator.GetBoneTransform(attackInfo.InstantiatePoint).position;
            else
            {
                projectile.transform.position = animator.transform.position;
                if (projectile.CombatManager != null)
                {
                    foreach (var member in projectile.CombatManager.Members)
                    {
                        if (member.HitCollider.HitColliderName.Equals(attackInfo.InstantiatePoint.ToString()))
                        {
                            projectile.transform.position = member.HitCollider.transform.position;
                            break;
                        }
                    }
                }
            }
            var dir = animator.transform.forward;
            dir.y = 0f;
            projectile.transform.forward = dir;

        }
    }
}