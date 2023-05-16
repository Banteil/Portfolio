using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zeus
{
    [ClassHeader("ZEUS AI CONTROLLER", IconName = "AI-icon")]
    public class ZeusAIController : ControlAICombat, IControlAIZeus
    {
        #region Variables

        [EditorToolbar("Start")]
        [SerializeField]
        protected string _groupID;

        [EditorToolbar("Combat Settings", order = 9)]
        [Header("AI Pattern List")]
        [SerializeField]
        protected List<AIPattern> _patterns;

        [HideInInspector] public AttackSignUI AttackSign;
        public Func<bool> CallTakeDamageAction;

        private int _moveSetID;
        private int _attackSetID;
        private IFSMBehaviourController _fsmController;

        private static readonly int MoveSetIDHash = Animator.StringToHash("MoveSetID");
        private static readonly int AttackSetIDHash = Animator.StringToHash("AttackSetID");

        #endregion

        #region PropertyVariables

        public CombatManager CombatManager { get; set; }

        public string GroupID
        {
            get => _groupID;
            set => _groupID = value;
        }

        public List<AIPattern> AIPatterns => _patterns;

        public int CurrentPatternIndex { get; set; } = 0;

        public virtual bool CanUsePattern => PossibleUsePattern(_patterns[CurrentPatternIndex]);

        public virtual int MoveSetID
        {
            get => _moveSetID;
            set
            {
                if (value == _moveSetID && Animator.GetInteger(MoveSetIDHash) == value) return;
                _moveSetID = value;
                Animator.SetInteger(MoveSetIDHash, _moveSetID);
            }
        }

        public virtual int AttackSetID
        {
            get => _attackSetID;
            set
            {
                if (value == _attackSetID && Animator.GetInteger(AttackSetIDHash) == value) return;
                _attackSetID = value;
                Animator.SetInteger(AttackSetIDHash, _attackSetID);
            }
        }

        public int RotateDirection { get; set; }

        #endregion

        protected override void AwakeInit()
        {
            base.AwakeInit();
            CombatManager = GetComponent<CombatManager>();
            _fsmController = GetComponent<IFSMBehaviourController>();
        }

        protected override void Init()
        {
            base.Init();
            _typeCharacter = TypeCharacter.AI;
            RotateDirection = 0;
            for (int i = 0; i < _patterns.Count; i++)
            {
                _patterns[i].Index = i;
            }
        }

        public override void CreateSecondaryComponents()
        {
            base.CreateSecondaryComponents();
            if (CombatManager == null) gameObject.AddComponent<CombatManager>();
        }


        public virtual void SetMeleeHitTags(List<string> tags)
        {
            if (CombatManager) CombatManager.HitProperties.HitDamageTags = tags;
        }

        protected override void AnimatorLocomotion()
        {
            Animator.SetFloat("InputHorizontal", CanMove && IsStrafing && !IsSprinting ? _direction : 0f, .2f,
                GameTimeManager.Instance.DeltaTime);
            Animator.SetFloat("InputVertical", CanMove ? _speed : 0f, .2f, GameTimeManager.Instance.DeltaTime);

            IsStrafingAnim = IsStrafing;
            IsGroundedAnim = IsGrounded;
            VerticalVelocityAnim = _verticalVelocity;
        }

        public override void OnEnableAttack()
        {
            AttackCount--;
        }

        public override void OnDisableAttack()
        {
            if (!CanAttack)
            {
                ResetAttackTriggers();
                if (_patterns.Count <= 0)
                    InitAttackTime();
                else
                    PatternChange();
            }
            else if (!PossibleUsePattern(_patterns[CurrentPatternIndex]))
            {
                ResetAttackTriggers();
                AttackCount = 0;
                PatternChange();
            }

            LockRotation = false;
        }

        private void PatternChange()
        {
            CurrentPatternIndex = _patterns[CurrentPatternIndex].NextIndex;
            //다음 패턴 인덱스가 없다면(-1) 사용 가능한 패턴 탐색 후 인덱스 부여
            if (CurrentPatternIndex.Equals(-1))
                SetUsePatternIndex();
            InitAttackTime(_patterns[CurrentPatternIndex]);
        }

        private bool PossibleUsePattern(AIPattern pattern)
        {
            foreach (var decision in pattern.Decisions)
            {
                if (!decision.Decide(_fsmController))
                {
                    return false;
                }
            }

            return true;
        }

        public void SetUsePatternIndex()
        {
            CurrentPatternIndex = 0;
            var usePossiblePatterns = _patterns.FindAll((x) => PossibleUsePattern(x)).ToList();
            if (usePossiblePatterns.Count.Equals(1))
                CurrentPatternIndex = usePossiblePatterns[0].Index;
            else if (usePossiblePatterns.Count > 1)
            {
                //우선 순위로 패턴 체크
                usePossiblePatterns = usePossiblePatterns.OrderBy(x => x.Priority).ToList();
                var maxPriority = usePossiblePatterns[^1].Priority;
                for (int i = 0; i < usePossiblePatterns.Count; i++)
                {
                    if (usePossiblePatterns[i].Priority >= maxPriority) continue;
                    usePossiblePatterns.RemoveAt(i);
                    i--;
                }

                //우선 순위에서도 걸러지지 않으면 랜덤 인덱스 부여
                CurrentPatternIndex = usePossiblePatterns.Count.Equals(1)
                    ? usePossiblePatterns[0].Index
                    : usePossiblePatterns[UnityEngine.Random.Range(0, usePossiblePatterns.Count)].Index;
                //Debug.Log($"{_patterns[_currentPatternIndex].PatternName} 선택!");
            }
        }

        public override void InitAttackTime(AIPattern pattern = null)
        {
            base.InitAttackTime(pattern);
            AttackSetID = pattern?.AttackID ?? AttackSetID;
            _attackDistance = (pattern?.AttackRange ?? _attackDistance) + BlockerRadius;
            _minDistanceOfTheTarget = (pattern != null ? (pattern.AttackRange * 0.8f) : _minDistanceOfTheTarget) + BlockerRadius;
        }

        public override void TakeDamage(Damage damage)
        {
            if (CallTakeDamageAction != null)
                if (CallTakeDamageAction.Invoke()) return;

            if (AnimatorSpeedManager.Get() != null)
            {
                AnimatorSpeedManager.Get().SetAnimatorSpeed(Animator, 0.02f, 0.1f);
            }

            base.TakeDamage(damage);
        }

        protected override void HitReactProcess(float duration)
        {
            if (_patterns.Count > 0) InitAttackTime(_patterns[CurrentPatternIndex]);
            base.HitReactProcess(duration);
        }

        protected override void StrafeMovement()
        {
            StrafeLimitSpeed(MaxSpeed);
            if (StopMove)
            {
                _strafeMagnitude = 0f;
            }

            var rotDir = IsAttacking
                ? (CurrentTarget.Transform.position - transform.position)
                : TargetDirection.normalized;
            rotDir.y = 0;
            if (rotDir.magnitude > 0.1f)
            {
                if (!LockRotation)
                {
                    Rotate(rotDir);
                }
            }
            else
            {
                if (_temporaryDirectionTime > 0)
                {
                    Rotate(_temporaryDirection);
                }
            }

            Animator.SetFloat("InputMagnitude", _strafeMagnitude, InputMagnitudeSmooth,
                GameTimeManager.Instance.DeltaTime);
        }

        protected override void Rotate(Vector3 targetDirection)
        {
            targetDirection.y = 0f;
            if (targetDirection.magnitude > 0.1f)
            {
                if (RotateDirection.Equals(0))
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(targetDirection, Vector3.up),
                        RotationSpeed * GameTimeManager.Instance.DeltaTime * Animator.speed);
                else
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation,
                        Quaternion.LookRotation(transform.right * RotateDirection, Vector3.up),
                        RotationSpeed * GameTimeManager.Instance.DeltaTime * Animator.speed);
                    float diff = transform.rotation.eulerAngles.y -
                                 Quaternion.LookRotation(targetDirection, Vector3.up).eulerAngles.y;
                    float dergee = 100f;
                    if (Mathf.Abs(diff) <= dergee)
                        RotateDirection = 0;
                }
            }
        }

        public void StartFSMCoroutine(IEnumerator coroutine) => StartCoroutine(coroutine);
    }
}