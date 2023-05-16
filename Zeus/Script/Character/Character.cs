using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    #region ExcutionData
    public enum TypeExcutionDirection
    {
        FRONT,
        BACK,
    }

    [Serializable]
    public class ExcutionData
    {
        public TypeWeapon WeaponType;
        public TypeExcutionDirection DirectionType;
        public Transform OwnerPoint;
        public string BehaviourPath;
        public string AnimatorPath;
    }

    #endregion

    public enum TypeCharacter
    {
        PLAYERBLE,
        AI
    }

    [Flags]
    public enum TypeCharacterState
    {
        NONE = 0,
        GUARD = 1,
        PARRY_READY = 2,
        PARRY = 4,
        DO_NOT_MOVE = 8,
        DAMAGE_INCREASE = 16,
        GROGGY = DO_NOT_MOVE | DAMAGE_INCREASE,
    }

    [Serializable]
    public class OnActionHandle : UnityEvent<Collider>
    {
    }

    public class Character : HealthController, ICharacter
    {
        public enum DeathBy
        {
            Animation,
            AnimationWithRagdoll,
            Ragdoll,
            Dissolve
        }

        public enum AfterDie
        {
            None,
            RemoveComponents,
            DestroySelf,
            InactiveSelf
        }

        [EditorToolbar("Start")]
        [ReadOnly]
        [SerializeField]
        protected string _guid;
        internal TypeCharacterState CharacterState { get; private set; }
        internal int ZoneID { get; set; }

        [ReadOnly][SerializeField] protected TypeCharacter _typeCharacter;
        public TypeCharacter TypeCharacter => _typeCharacter;

        [EditorToolbar("Blocker")]
        public float BlockerAddRadius = 0f;
        public CapsuleCollider BlockerCollider { get; private set; }

        [EditorToolbar("Health")]
        public DeathBy DeathByType = DeathBy.Animation;
        public AfterDie AfterDieType = AfterDie.None;
        public Animator Animator { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public CapsuleCollider CapsuleCollider { get; set; }
        public bool ReactionPrevention { get; set; }

        OnActiveRagdoll _onActiveRagdoll = new();

        public OnActiveRagdoll OnActiveRagdollEvent
        {
            get => _onActiveRagdoll;
            protected set => _onActiveRagdoll = value;
        }

        public delegate void DamageValueEvent(Damage damage);

        public DamageValueEvent OnGuardSuccess;
        public DamageValueEvent OnParrySuccess;

        private AnimationStateTable _animationStateTable;
        protected AnimatorParameter _triggerResetStateHash;
        protected AnimatorParameter _recoilIDHash;
        protected AnimatorParameter _triggerRecoilHash;

        public List<ExcutionData> Excutions;

        public string GUID => _guid;

        public virtual bool GhostMode
        {
            set
            {
                if (value) Rigidbody.velocity = Vector3.zero;
                Rigidbody.isKinematic = value;
                CapsuleCollider.isTrigger = value;
            }
        }

        public virtual bool IsBlocking => CharacterState.HasFlag(TypeCharacterState.GUARD);

        public virtual Transform CharacterTransform => transform;

        public virtual GameObject CharacterGameObject => gameObject;

        public virtual AnimationStateTable AnimationStateTable => _animationStateTable;

        public virtual bool IsStop
        {
            get => IsStateActive("Stop");
            set
            {
                if (value)
                {
                    ResetAttackTriggers();
                    Animator.SetFloat("InputMagnitude", 0f);
                    if (_triggerResetStateHash.isValid) Animator.SetTrigger(_triggerResetStateHash);
                    AddState("Stop", float.PositiveInfinity, true);
                }
                else
                {
                    DeleteState("Stop");
                }
            }
        }

        private List<TimeLimitState> _timeLimitStates = new();

        public float BlockerRadius
        {
            get
            {
                if (BlockerCollider == null) return 0f;
                return BlockerCollider.radius;
            }
        }

        [EditorToolbar("Ability")]
        public Transform BindAbility;
        private List<CharacterAbility> _abilities = new();

        protected override void AwakeInit()
        {
            _guid = Guid.NewGuid().ToString();
            Animator = GetComponent<Animator>();
            if (Animator != null)
            {
                _animationStateTable = new AnimationStateTable(Animator);
            }

            Rigidbody = GetComponent<Rigidbody>();
            CapsuleCollider = GetComponent<CapsuleCollider>();
            BlockerInit();
            _abilities = BindAbility
                ? BindAbility.GetComponentsInChildren<CharacterAbility>().ZToList()
                : GetComponentsInChildren<CharacterAbility>().ZToList();
        }

        protected override void Init()
        {
            base.Init();
            CharacterObjectManager.Get().AddCharacterList(this);
            if (_typeCharacter == TypeCharacter.AI)
                OnDeadEvent.AddListener(CharacterObjectManager.Get().RemoveCharacterList);
        }

        void BlockerInit()
        {
            var blockerObj = new GameObject("CharacterBlocker")
            {
                transform =
                {
                    parent = transform,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity,
                    localScale = Vector3.one
                },
                layer = LayerMask.NameToLayer("Blocker")
            };

            BlockerCollider = blockerObj.AddComponent<CapsuleCollider>();
            BlockerCollider.direction = CapsuleCollider.direction;
            BlockerCollider.radius = CapsuleCollider.radius + BlockerAddRadius;
            BlockerCollider.height = CapsuleCollider.height;
            BlockerCollider.center = CapsuleCollider.center;

            var blockerRigidbody = blockerObj.AddComponent<Rigidbody>();
            blockerRigidbody.isKinematic = true;
            blockerRigidbody.useGravity = false;
            blockerRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            Physics.IgnoreCollision(CapsuleCollider, BlockerCollider, true);
        }

        protected virtual void Update()
        {
            EarlyProcessAbilities();
            ProcessAbilities();
            LateProcessAbilities();
            CheckTimeLimitState();
        }

        protected virtual void EarlyProcessAbilities()
        {
            foreach (var ability in _abilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                {
                    ability.EarlyProcessAbility();
                }
            }
        }

        protected virtual void ProcessAbilities()
        {
            foreach (var ability in _abilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                {
                    ability.ProcessAbility();
                }
            }
        }

        protected virtual void LateProcessAbilities()
        {
            foreach (var ability in _abilities)
            {
                if (ability.enabled && ability.AbilityInitialized)
                {
                    ability.LateProcessAbility();
                }
            }
        }

        public void AbilitySendMessage(AnimationEvent animationEvent)
        {
            if (animationEvent.objectReferenceParameter != null)
            {
                var ability = GetAbility(animationEvent.objectReferenceParameter.name);
                if (ability == null)
                {
                    Debug.LogError($"{animationEvent.objectReferenceParameter.name} 어빌리티가 존재하지 않습니다.");
                    return;
                }

                ability.SendMessage(animationEvent.stringParameter);
            }
            else
            {
                if (BindAbility != null)
                    BindAbility.SendMessage(animationEvent.stringParameter);
                else
                    SendMessage(animationEvent.stringParameter);
            }
        }

        public T GetAbility<T>() where T : CharacterAbility
        {
            foreach (var ability in _abilities)
            {
                if (ability is T characterAbility)
                {
                    return characterAbility;
                }
            }

            return null;
        }

        public CharacterAbility GetAbility(string abilityName)
        {
            foreach (var ability in _abilities)
            {
                if (ability.GetType().Name.Equals(abilityName))
                {
                    return ability;
                }
            }

            return null;
        }

        private void CheckTimeLimitState()
        {
            for (int i = 0; i < _timeLimitStates.Count; i++)
            {
                var state = _timeLimitStates[i];
                state.Time += GameTimeManager.Instance.DeltaTime;
                if (state.Time >= state.Duration)
                {
                    _timeLimitStates.RemoveAt(i);
                    i--;
                }
            }
        }

        public void AddState(string stateName, float duration, bool fixedDuration = false)
        {
            if (IsStateActive(stateName))
            {
                var state = _timeLimitStates.Find(state => { return state.State.Equals(stateName); });
                if (state.FixedDuration) duration = state.Duration;
                state.ResetInfo(duration);
            }
            else
            {
                _timeLimitStates.Add(new TimeLimitState(stateName, duration, fixedDuration));
            }
        }

        public void DeleteState(string stateName)
        {
            var index = _timeLimitStates.FindIndex((state) => state.State.Equals(stateName));
            if (index.Equals(-1))
            {
                Debug.LogWarning($"{gameObject.name}에겐 {stateName} 상태가 없습니다.");
                return;
            }

            _timeLimitStates.RemoveAt(index);
        }

        private bool IsStateActive(string stateName)
        {
            return _timeLimitStates.Exists((state) => state.State.Equals(stateName));
        }

        public override void TakeDamage(Damage damage)
        {
            if (IsInvincibility) return;

            base.TakeDamage(damage);
            TriggerDamageReaction(damage);
            if (damage.DamageValue <= 0)
                return;

            if (EffectsManager.Get() != null)
            {
                EffectsManager.Get().SetBloodFx(damage.HitPosition,
                    (damage.HitPosition - damage.AttackPosition).normalized);
            }
        }

        public virtual void TriggerDamageReaction(Damage damage)
        {
            if (Animator != null && Animator.enabled && !damage.ActiveRagdoll && _currentHealth > 0)
            {
                // trigger hitReaction animation
                if (damage.HitReaction)
                {
                    int reactionID = damage.AttackerHitProperties.ReactionID;
                    int hitDirection = (int)transform.HitAngle(damage.Sender.position);
                    Animator.Play(_animationStateTable.HitStateHash[reactionID][hitDirection],
                        _animationStateTable.FullBodyLayer, 0f);
                    if (_triggerResetStateHash.isValid) SetTrigger(_triggerResetStateHash);
                    HitReactProcess(damage.AttackerHitProperties.StopDuration);
                }
                else
                {
                    if (_recoilIDHash.isValid)
                        Animator.SetInteger(_recoilIDHash, damage.AttackerHitProperties.RecoilID);
                    if (_triggerRecoilHash.isValid) Animator.SetTrigger(_triggerRecoilHash);
                    if (_triggerResetStateHash.isValid) SetTrigger(_triggerResetStateHash);
                }
            }

            if (damage.ActiveRagdoll)
                OnActiveRagdollEvent.Invoke(damage);

            CharacterObjectManager.Get().AddHitBackList(_guid, damage);
        }

        protected virtual void HitReactProcess(float duration)
        {
            ResetAttackTriggers();
            Animator.SetFloat("InputMagnitude", 0f);
            AddState("Stop", duration);
        }

        public virtual void OnDeath() => IsDead = true;

        public virtual void ParrySuccess(Damage damage)
        {
        }

        public virtual void GuardSuccess(Damage damage)
        {
        }

        IEnumerator SetTriggerProcess(int trigger)
        {
            Animator.SetTrigger(trigger);
            yield return null;
            Animator.ResetTrigger(trigger);
        }

        public void SetTrigger(int trigger)
        {
            StartCoroutine(SetTriggerProcess(trigger));
        }

        public virtual void OnRecoil(int recoilID)
        {
        }

        public virtual void ResetAttackTriggers()
        {
            Animator.ResetTrigger("Attack");
        }

        internal void AddActionState(TypeCharacterState type)
        {
            CharacterState |= type;
        }

        internal void RemoveActionState(TypeCharacterState type)
        {
            CharacterState &= ~type;
        }

        private void OnDestroy()
        {
            if (TypeCharacter == TypeCharacter.PLAYERBLE)
            {
                return;
            }

            if (CharacterObjectManager.Get() == null) return;
            CharacterObjectManager.Get().RemoveCharacterList(gameObject);

            if (TypeCharacter != TypeCharacter.AI) return;
            var iZeus = GetComponent<IControlAIZeus>();
            CharacterObjectManager.Get().RemoveGroupMember(iZeus);
        }
    }

    public class TimeLimitState
    {
        public string State { get; }
        public float Duration { get; private set; }
        public float Time { get; set; }
        public bool FixedDuration { get; private set; }

        public TimeLimitState(string stateName, float duration, bool fixedDuration)
        {
            State = stateName;
            Duration = duration;
            FixedDuration = fixedDuration;
            Time = 0f;
        }

        public void ResetInfo(float duration)
        {
            Duration = duration;
            Time = 0f;
        }
    }
}