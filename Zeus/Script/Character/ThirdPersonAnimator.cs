using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class ThirdPersonAnimator : ThirdPersonMotor
    {
        #region Variable
        public const float WalkSpeed = 0.5f;
        public const float RunningSpeed = 1.5f;
        public const float SprintSpeed = 2f;

        public float MaxAimAngle = 30f;
        #endregion

        #region 

        public UnityAction onAttackStateChange;

        #endregion

        protected override void Init()
        {
            base.Init();
            RegisterAnimatorStateInfos();
            _onDead.AddListener((GameObject o) => DeadAnimation());
            onResetHealth.AddListener(() => Reseraction());
            _triggerResetStateHash = new AnimatorParameter(Animator, "ResetState");
        }

        protected void RegisterAnimatorStateInfos()
        {
            //애니메이터 정보를 저장
            AnimatorStateInfos = new AnimatorStateInfos(GetComponent<Animator>());
            AnimatorStateInfos.RegisterListener();
        }

        protected virtual void OnEnable()
        {
            if (AnimatorStateInfos.Animator != null)
            {
                AnimatorStateInfos.RegisterListener();
            }
        }

        protected virtual void OnDisable()
        {
            AnimatorStateInfos.RemoveListener();
        }

        public void UpdateAnimator()
        {
            if (Animator == null || !Animator.enabled)
            {
                return;
            }
            AnimatorLayerControl();
            UpdateAnimatorParameters();
            ActionsControl();
        }

        public void AnimatorLayerControl()
        {
            _baseLayerInfo = Animator.GetCurrentAnimatorStateInfo(BaseLayer);
            _upperBodyInfo = Animator.GetCurrentAnimatorStateInfo(UpperBodyLayer);
            _fullBodyInfo = Animator.GetCurrentAnimatorStateInfo(FullBodyLayer);
        }

        //현재 애니메이션의 태그 존재 확인용
        public void ActionsControl()
        {
            _customAction = IsAnimatorTag("CustomAction");
            _lockAnimMovement = IsAnimatorTag("LockMovement");
            _lockRotation = IsAnimatorTag("LockRotation");
            IsAttack = IsAnimatorTag("Attack");
            var currentCantAttack = IsAnimatorTag("IsCantAttack");
            if (currentCantAttack == false && IsCantAttack)
            {
                IsCantAttack = currentCantAttack;
                onAttackStateChange.Invoke();
            }
            IsCantAttack = currentCantAttack;
            //해당 변수 활성화시 루트모션으로 이동가능
            _animRootMotion = IsAnimatorTag("RootMotion");
            NotMoveToRootMotion = IsAnimatorTag("NotMoveToRootMotion");
            //해당 변수 활성화 시 루트모션 도중 회전 가능
            _customActionRotateAble = IsAnimatorTag("CustomActionRotateAble");
            LookForwardInRoll = IsAnimatorTag("LookForwardInRoll");
            _cantInputToAnimator = IsAnimatorTag("CantInput");
            //이동을 제외한 키 인풋 막을 때 사용(Ex 무기 교체시 이동은 가능하지만 공격불가)
            _cantInputExceptionMove = IsAnimatorTag("CantInputExceptionMove");
            _useHandIK = IsAnimatorTag("UseHandIK");
            _ignoreHandIK = IsAnimatorTag("IgnoreHandIK");
            IsParrySuccess = IsAnimatorTag("IsParrySuccess");
        }

        public void SetAnimatorMoveSpeed(MovementSpeed speed)
        {
            //InverseTransformDirection : 월드 기준으로 정의된 것을 로컬로 바꿈
            //캐릭터가 바라보는 방향 기준으로 값을 정하기 위해;
            Vector3 relativeInput = transform.InverseTransformDirection(MoveDirection);
            _verticalSpeed = relativeInput.z;
            _horizontalSpeed = relativeInput.x;

            var newInput = new Vector2(_verticalSpeed, _horizontalSpeed);
            var mag = newInput.magnitude;
            if (_input != Vector3.zero)
            {
                _sprintWeight = Mathf.Lerp(_sprintWeight, IsSprinting ? 1f : 0f, (IsStrafing ? StrafeSpeed.MovementSmooth : FreeSpeed.MovementSmooth) * Time.fixedDeltaTime);
            }
            else
            {
                _sprintWeight = Mathf.Lerp(_sprintWeight, 0f, (IsStrafing ? StrafeSpeed.MovementSmooth : FreeSpeed.MovementSmooth) * Time.fixedDeltaTime);
            }
            _inputMagnitude = Mathf.Clamp(Mathf.Lerp(mag, mag + 0.5f, _sprintWeight), 0, IsSprinting ? SprintSpeed : RunningSpeed);
        }

        public void ResetInputAnimatorParameters()
        {
            //애니메이션 피라미터를 초기화 함
            //추후 추가되는 피라미터에 따라 추가 작업 필요
            //ex
            Animator.SetFloat(AnimatorParameters.GroundDistance, 0f);
            Animator.SetBool(AnimatorParameters.IsGrounded, true);
            Animator.SetBool(AnimatorParameters.IsAim, false);
            Animator.SetBool(AnimatorParameters.IsDead, false);
            Animator.SetBool(AnimatorParameters.IsBlock, false);
            Animator.SetBool(AnimatorParameters.WeaponEquip, false);

            Animator.ResetTrigger(AnimatorParameters.LightAttack);
            Animator.ResetTrigger(AnimatorParameters.HeavyAttack);
            Animator.ResetTrigger(AnimatorParameters.WeaponSwap);
            Animator.ResetTrigger(AnimatorParameters.TriggerReaction);
            Animator.ResetTrigger(AnimatorParameters.Skill);
            Animator.ResetTrigger(AnimatorParameters.Parry);
            Animator.ResetTrigger(AnimatorParameters.BlockTrigger);
            Animator.ResetTrigger(AnimatorParameters.ExcutionTrigger);
            Animator.ResetTrigger(AnimatorParameters.AttackReset);
            Animator.ResetTrigger(AnimatorParameters.CounterAttack);
            Animator.ResetTrigger(AnimatorParameters.UsePotion);
            Animator.ResetTrigger(AnimatorParameters.PickUp);

            Animator.SetTrigger("ResetTrigger");
            Animator.SetFloat("InputHorizontal", 0);
            Animator.SetFloat("InputVertical", 0);
            Animator.SetFloat("InputMagnitude", 0);
        }


        protected void DeadAnimation()
        {
            Animator.SetBool(AnimatorParameters.IsDead, IsDead);
            PlayerUIManager.Get().OnCheckPointStart();
        }

        protected virtual void Reseraction()
        {
            Animator.SetBool(AnimatorParameters.IsDead, IsDead);
            ResetInputAnimatorParameters();
            CantInput = false;
        }
        #region Animations Method

        //현재 애니메이션 태그 확인
        public bool IsAnimatorTag(string tag)
        {
            if (Animator == null)
            {
                return false;
            }

            if (AnimatorStateInfos != null)
            {
                if (AnimatorStateInfos.HasTag(tag))
                {
                    return true;
                }
            }

            if (_baseLayerInfo.IsTag(tag))
            {
                return true;
            }

            return false;
        }


        #endregion


        public virtual void UpdateAnimatorParameters()
        {
            if (IsStrafing && !_cantInputToAnimator)
            {
                Animator.SetFloat(AnimatorParameters.InputHorizontal, _horizontalSpeed * (IsSprinting ? RunningSpeed : WalkSpeed), StrafeSpeed.AnimationSmooth, Time.fixedDeltaTime);
                Animator.SetFloat(AnimatorParameters.InputVertical, _verticalSpeed * (IsSprinting ? RunningSpeed : WalkSpeed), StrafeSpeed.AnimationSmooth, Time.fixedDeltaTime);
                Animator.SetFloat(AnimatorParameters.AimVertical, _verticalSpeed * (IsSprinting ? RunningSpeed : WalkSpeed), StrafeSpeed.AnimationSmooth, Time.fixedDeltaTime);
                //Animator.SetFloat(AnimatorParameters.RotationValue, Vector3.Angle(new Vector3(0f, transform.eulerAngles.y, 0f), new Vector3(0f, _lastCharacterAngle.y, 0f)));
                //Animator.SetFloat(AnimatorParameters.RotationMagnitude, _rotationMagnitude, LeanSmooth, Time.fixedDeltaTime);
            }
            else
            {
                Animator.SetFloat(AnimatorParameters.RotationMagnitude, _rotationMagnitude, LeanSmooth, Time.fixedDeltaTime);
                Animator.SetBool(AnimatorParameters.IsSprinting, IsSprinting);
                Animator.SetFloat(AnimatorParameters.RotationValue, Vector3.Angle(new Vector3(transform.forward.x, 0f, transform.forward.z), new Vector3(MoveDirection.x, 0f, MoveDirection.z)));
            }
            Animator.SetBool(AnimatorParameters.IsGrounded, IsGrounded);
            Animator.SetFloat(AnimatorParameters.GroundDistance, _groundDistance);
            Animator.SetFloat(AnimatorParameters.InputMagnitude, Mathf.LerpUnclamped(_inputMagnitude, 0f, _stopMoveWeight), IsStrafing ? StrafeSpeed.AnimationSmooth : FreeSpeed.AnimationSmooth, Time.fixedDeltaTime);
        }

        public override void ResetAttackTriggers()
        {
            Animator.ResetTrigger(AnimatorParameters.LightAttack);
            Animator.ResetTrigger(AnimatorParameters.HeavyAttack);
        }
    }

    public static partial class AnimatorParameters
    {
        //애니메이션 피라미터 관리
        public static int InputHorizontal = Animator.StringToHash("InputHorizontal");
        public static int InputVertical = Animator.StringToHash("InputVertical");
        public static int InputMagnitude = Animator.StringToHash("InputMagnitude");
        public static int AimVertical = Animator.StringToHash("AimVertical");
        public static int RotationMagnitude = Animator.StringToHash("RotationMagnitude");
        public static int IdleRandom = Animator.StringToHash("IdleRandom");
        public static int IdleRandomTrigger = Animator.StringToHash("IdleRandomTrigger");
        public static int RotationValue = Animator.StringToHash("RotationValue");
        public static int Evasion = Animator.StringToHash("Evasion");
        public static int EvasionInputHorizontal = Animator.StringToHash("InputEvasionHorizontal");
        public static int EvasionInputVertical = Animator.StringToHash("InputEvasionVertical");
        public static int IsSprinting = Animator.StringToHash("IsSprinting");
        public static int LightAttack = Animator.StringToHash("LightAttack");
        public static int HeavyAttack = Animator.StringToHash("HeavyAttack");
        public static int AttackReset = Animator.StringToHash("AttackReset");
        public static int CounterAttack = Animator.StringToHash("CounterAttack");
        public static int ExcutionTrigger = Animator.StringToHash("ExcutionTrigger");
        public static int Parry = Animator.StringToHash("Parry");
        public static int WeaponType = Animator.StringToHash("WeaponType");
        public static int WeaponSwap = Animator.StringToHash("WeaponSwap");
        public static int WeaponEquip = Animator.StringToHash("WeaponEquip");
        public static int Skill = Animator.StringToHash("Skill");
        public static int SkillAniNum = Animator.StringToHash("SkillAniNum");
        public static int SkillNum = Animator.StringToHash("SkillNum");
        public static int IsGrounded = Animator.StringToHash("IsGrounded");
        public static int GroundDistance = Animator.StringToHash("GroundDistance");
        public static int IsAim = Animator.StringToHash("IsAim");
        public static int IsDead = Animator.StringToHash("IsDead");
        public static int IsHit = Animator.StringToHash("Hit");
        public static int TriggerReaction = Animator.StringToHash("TriggerReaction");
        public static int IsBlock = Animator.StringToHash("Block");
        public static int BlockTrigger = Animator.StringToHash("BlockTrigger");
        public static int UsePotion = Animator.StringToHash("UsePotion");
        public static int PickUp = Animator.StringToHash("PickUp");
    }
}