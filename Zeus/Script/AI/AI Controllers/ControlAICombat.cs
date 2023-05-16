using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public partial class ControlAICombat : ControlAI, IControlAICombat
    {
        #region Variables

        [EditorToolbar("Combat Settings", order = 9)]
        [Header("Attack Settings")]
        [SerializeField]
        protected float _minAttackTime = 0.5f;

        [SerializeField] protected float _maxAttackTime = 2f;
        [SerializeField] protected float _attackDistance = 1f;

        [Header("Blocking Settings")]
        [Tooltip(
            "The AI has a chance to enter the defence animation while in combat, easier to predict when to attack")]
        [Range(0f, 100f)]
        [SerializeField]
        protected float _combatBlockingChance = 50;

        [Tooltip(
            "The AI has a random chance to block the damage at the time he receive it, so you won't be able to predict")]
        [Range(0f, 100f)]
        [SerializeField]
        protected float _onDamageBlockingChance = 25;

        [SerializeField] protected float _minStayBlockingTime = 4;
        [SerializeField] protected float _maxStayBlockingTime = 6;
        [SerializeField] protected float _minTimeToTryBlock = 4;
        [SerializeField] protected float _maxTimeToTryBlock = 6;

        [vHelpBox("Damage type that can block")]
        [SerializeField]
        protected List<string> _ignoreDefenseDamageTypes = new List<string>() { "Unarmed", "Melee" };

        [Header("Combat Movement")]
        [SerializeField]
        protected float _minDistanceOfTheTarget = 2;

        [SerializeField] protected float _combatDistance = 4f;
        [SerializeField] protected bool _strafeCombatMovement = true;

        [zHideInInspector("_strafeCombatMovement")]
        [SerializeField,
         Tooltip(
             "This control random Strafe Combate Movement side, if True the side is ever -1 or 1 else side can be set to zero (0)")]
        protected bool _alwaysStrafe = false;

        [zHideInInspector("_strafeCombatMovement")]
        [SerializeField]
        protected float _minTimeToChangeStrafeSide = 1f, _maxTimeToChangeStrafeSide = 4f;

        [EditorToolbar("Debug", order = 100)]
        [vHelpBox("Debug Combat")]
        [SerializeField, ReadOnly(false)]
        protected bool _isInCombat;

        [SerializeField, ReadOnly(false)] protected bool _isBlocking;
        protected float _attackTime;
        protected float _blockingTime;
        protected float _tryBlockTime;
        protected float _timeToChangeStrafeSide;

        protected AnimatorParameter _isBlockingHash;

        #endregion

        protected override void Init()
        {
            base.Init();
            if (Animator)
            {
                _isBlockingHash = new AnimatorParameter(Animator, "IsBlocking");
            }

            StrafeCombatSide = 1;
        }

        protected override void HandleTarget()
        {
            base.HandleTarget();
            UpdateStrafeCombateMovementSide();
        }

        protected override void UpdateAnimator()
        {
            base.UpdateAnimator();
            UpdateCombatAnimator();
        }

        protected virtual void UpdateCombatAnimator()
        {
            if (!IsMainRuntimeAnimation) return;
            if (_isBlocking && Time.time > _blockingTime || CustomAction)
            {
                _tryBlockTime = Random.Range(_minTimeToTryBlock, _maxTimeToTryBlock) + Time.time;
                _isBlocking = false;
            }

            if (_isBlockingHash.isValid) Animator.SetBool(_isBlockingHash, _isBlocking);
        }

        public override void FindTarget(bool checkForObstacles = true)
        {
            if (_currentTarget.Transform && TargetDistance <= CombatRange && _hasPositionOfTheTarget || IsAttacking)
            {
                if (_updateFindTargetTime > Time.time) return;
                _updateFindTargetTime = Time.time + GetUpdateTimeFromQuality(_findTargetUpdateQuality);
                return;
            }

            base.FindTarget(checkForObstacles);
        }

        protected virtual void UpdateStrafeCombateMovementSide()
        {
            if (!StrafeCombatMovement) return;
            if (_timeToChangeStrafeSide <= 0)
            {
                var randomValue = Random.Range(0, 100);
                if (_alwaysStrafe)
                {
                    if (randomValue > 50)
                    {
                        StrafeCombatSide = 1;
                    }
                    else StrafeCombatSide = -1;
                }
                else
                {
                    if (randomValue >= 70)
                    {
                        StrafeCombatSide = 1;
                    }
                    else if (randomValue <= 30) StrafeCombatSide = -1;
                    else StrafeCombatSide = 0;
                }

                _timeToChangeStrafeSide = Random.Range(_minTimeToChangeStrafeSide, _maxTimeToChangeStrafeSide);
            }
            else
                _timeToChangeStrafeSide -= GameTimeManager.Instance.DeltaTime;
        }

        public virtual float CombatRange => _combatDistance;

        public virtual int StrafeCombatSide { get; set; }

        public virtual bool StrafeCombatMovement => _strafeCombatMovement;

        public virtual bool IsInCombat
        {
            get => _isInCombat;
            set
            {
                if (!value) Debug.Log("IsInCombat False");
                _isInCombat = value;
            }
        }

        public virtual float MinDistanceOfTheTarget => _minDistanceOfTheTarget;

        public virtual bool CanMove => !StopMove && !LockMovement && !CustomAction && !IsStop;

        public virtual float AttackDistance => _attackDistance;

        public virtual int AttackCount { get; set; }

        public virtual bool CanAttack => (_attackTime < Time.time) && AttackCount > 0 && !CustomAction && _targetInLineOfSight && !IsStop && TargetDistance <= AttackDistance;

        public bool IsAttacking => AnimatorStateInfos.HasTag("Attack");

        public virtual void Attack(bool forceCanAttack = false)
        {
            if (CanAttack || forceCanAttack)
            {
                Animator.SetTrigger("Attack");
            }
        }

        System.Random random = new System.Random();

        public virtual float BetterRandomThenUnity(float minimum, float maximum)
        {
            return (float)(random.NextDouble() * (maximum - minimum) + minimum);
        }

        public virtual void InitAttackTime(AIPattern pattern = null)
        {
            _tryBlockTime = BetterRandomThenUnity(_minTimeToTryBlock, _maxTimeToTryBlock) + Time.time;
            _attackTime = BetterRandomThenUnity(_minAttackTime, _maxAttackTime) + Time.time;
            AttackCount = pattern?.MaxAttackCount ?? 1;
        }

        public virtual void ResetAttackTime()
        {
            AttackCount = 0;
            _attackTime = Random.Range(_minAttackTime, _maxAttackTime) + Time.time;
        }

        public virtual bool CanBlockInCombat =>
            _combatBlockingChance > 0 && Time.time > _tryBlockTime && Time.time > _blockingTime &&
            !CustomAction;

        public virtual void ResetBlockTime()
        {
            _blockingTime = 0;
        }

        public virtual void Blocking()
        {
            if (_isBlocking || !CanBlockInCombat) return;
            if (!CheckChanceToBlock(_combatBlockingChance)) return;
            _isBlocking = true;
            _blockingTime = Random.Range(_minStayBlockingTime, _maxStayBlockingTime) + Time.time;
        }

        protected virtual void ImmediateBlocking()
        {
            if (!CheckChanceToBlock(_onDamageBlockingChance)) return;
            _blockingTime = Random.Range(_minStayBlockingTime, _maxStayBlockingTime) + Time.time;
            _isBlocking = true;
        }

        protected virtual bool CheckChanceToBlock(float chance)
        {
            return Random.Range(0f, 100f) <= chance;
        }

        public override void ResetAttackTriggers()
        {
            Animator.ResetTrigger("Attack");
        }

        public virtual void OnEnableAttack()
        {
            AttackCount--;
            LockRotation = true;
        }

        public virtual void OnDisableAttack()
        {
            if (AttackCount <= 0) InitAttackTime();
            LockRotation = false;
        }

        public override void OnRecoil(int recoilID)
        {
            if (Animator == null || !Animator.enabled) return;
            Animator.SetInteger("RecoilID", recoilID);
            Animator.SetTrigger("TriggerRecoil");
            Animator.SetTrigger("ResetState");
            ResetAttackTriggers();
        }

        protected virtual void TryBlockAttack(Damage damage)
        {
            var canBlock = !_ignoreDefenseDamageTypes.Contains(damage.DamageType) && !damage.IgnoreDefense;
            if (string.IsNullOrEmpty(damage.DamageType) && canBlock)
            {
                ImmediateBlocking();
            }

            damage.HitReaction = !_isBlocking || !canBlock;
        }

        public override void TakeDamage(Damage damage)
        {
            TryBlockAttack(damage);
            base.TakeDamage(damage);
        }
    }
}