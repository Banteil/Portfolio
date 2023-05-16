using System.Collections;
using UnityEngine;

namespace Zeus
{
    [ClassHeader("Third Person Controller", IconName = "AI-icon")]
    public class ThirdPersonController : ThirdPersonAnimator
    {
        internal CombatInput CombatInput;
        protected LockOn _lockOn;
        public RigController RigController;

        protected PlayerGaugeTypeUI _hpUI;
        protected PlayerCountTypeUI _staminaUI;

        //ȿ����
        private const int _hitEffectSoundID = 111;
        //���̽�
        private const int _hitSoundID = 105;

        public GameObject DefaultModel;
        [Header("Evation")]
        public GameObject EvasionModel;
        private const int _evantionSoundNum = 57;
        private int _evasionMaxCount = 3;
        private int _evasionCount;
        public float EvasionDelay = 3f;
        private float _currentEvasionDelayTime;
        //ȸ�������� �̺�Ʈ
        public event VoidEvent onEvasionEvent;

        [Header("Aim")]
        public float AimSpeed = 1f;
        [SerializeField]
        private float _aimVertical;
        public float AimPoint = 20f;

        [Header("Guard And Parry")]
        [HideInInspector]
        public float ParryDuring = 0.2f;
       

        //Delegate
        //ThirdPersonInput���� ����
        public event OnFixedEvent OnFixedUpdate;
        public delegate void OnFixedEvent();

        public delegate void VoidEvent();
        //���� Ȱ ���¸� ĵ���� ��Ȳ�� ���
        public event VoidEvent onCancelAction;

        //�и� ���¸� ĵ���Ҷ� ���
        


        protected override void Init()
        {
            base.Init();
            _lockOn = GetComponent<LockOn>();
            CombatInput = GetComponent<CombatInput>();
            EffectsManager.Get().AddParticle(EvasionModel);
            StartCoroutine(LoadData());
            OnFixedUpdate += EvasionCountRecovery;

            _typeCharacter = TypeCharacter.PLAYERBLE;
            _evasionCount = _evasionMaxCount;
        }

        private IEnumerator LoadData()
        {
            yield return new WaitUntil(() => PlayerUIManager.Get() != null);

            _hpUI = PlayerUIManager.Get().GetUI<PlayerGaugeTypeUI>(TypePlayerUI.HP);
            _staminaUI = PlayerUIManager.Get().GetUI<PlayerCountTypeUI>(TypePlayerUI.STAMINA);
            _staminaUI.SetValue(_staminaUI.Symbol.Count);
            onChangeHealth.AddListener(HPChange);
            HPChange(CurrentHealth);
            SetEvasionCount(0);
        }

        protected override void Reseraction()
        {
            base.Reseraction();
            _evasionCount = _evasionMaxCount;
            SetEvasionCount(0);
        }

        public void UpdateCharacterState()
        {
            if (OnFixedUpdate == null) { return; }
            OnFixedUpdate.Invoke();
        }

        #region Move And Rotate

        public void ControlAnimatorRootMotion()
        {
            if (!enabled)
            {
                return;
            }

            if (_isEvasion)
            {
                EvasionBehavior();
                return;
            }
            //������ �� �������� ĳ���� ȸ��
            if (LookForwardInRoll)
            {
                RotateToDirection(_rotateTarget.forward, 10f);
                return;
            }

            if (NotMoveToRootMotion)
            {
                MoveCharacter(MoveDirection);
                return;
            }

            if (_customAction || _lockAnimMovement)
            {
                StopCharacterWithLerp();
                Rigidbody.position = Animator.rootPosition;
                Rigidbody.rotation = Animator.rootRotation;
            }
            if (_animRootMotion)
            {
                MoveCharacter(MoveDirection);
                SetAnimatorMoveSpeed(FreeSpeed);
                Rigidbody.position = Animator.rootPosition;
                Rigidbody.rotation = Animator.rootRotation;
            }
        }

        //���ڸ�� Ÿ�Կ� ���� _moveSpeed ��ġ ����
        public virtual void ControlLocomotionType()
        {
            if (_lockAnimMovement || _lockMovement || _customAction || CantInput)
            {
                return;
            }
            if (!_lockSetMoveSpeed)
            {
                //ĳ������ �̵� ���°� Strafe ���°� �ƴҶ�
                if (!IsStrafing)
                {
                    SetControllerMoveSpeed(FreeSpeed);
                    SetAnimatorMoveSpeed(FreeSpeed);
                }
                else if (IsStrafing)
                {
                    if (_isEvasion) { return; }
                    IsStrafing = true;
                    SetControllerMoveSpeed(StrafeSpeed);
                    SetAnimatorMoveSpeed(StrafeSpeed);
                }
            }
            if (!UseRootMotion)
            {
                MoveCharacter(MoveDirection);
            }
        }

        public virtual void ControlRotationType()
        {
            if (_lockAnimMovement || _lockRotation || (_customAction && !_customActionRotateAble) || (_cantInputToAnimator) || CantInput)
            {
                return;
            }

            //��ǲ�� ���ų� ī�޶� ���� ȸ���ϸ� True
            bool validInput = StrafeSpeed.RotateWithCamera && _input != Vector3.zero || (IsStrafing ? StrafeSpeed.RotateWithCamera : FreeSpeed.RotateWithCamera);

            //LockOn�����Ͻ� CurrentTarget������ ȸ��
            if (_lockOn.IsLockingOn)
            {
                RotateToPosition(_lockOn.CurrentTarget.position);
                //MoveDirection = _lockOn.CurrentTarget.position - transform.position;
                return;
            }

            if (Animator.GetBool(AnimatorParameters.IsAim) && _rotateTarget != null)
            {
                //RotateToDirection(_rotateTarget.forward);
                RotateToPosition(_rotateTarget.transform.position + _rotateTarget.forward * AimPoint);
                return;
            }
            var dir = transform.forward;
            if (validInput)
            {
                //AnimatorTag("LockMovement")�� ����
                if (_lockAnimMovement)
                {
                    InputSmooth = Vector3.Lerp(InputSmooth, _input, (IsStrafing ? StrafeSpeed.MovementSmooth : FreeSpeed.MovementSmooth) * Time.fixedDeltaTime);
                }

                //�Ʒ� �ڵ�� �޸��� �������� ĳ���Ͱ� ȸ����
                //Vector3 dir = (IsStrafing&&IsGrounded&&(!IsSprinting || SprintOnlyFree == false) || (FreeSpeed.RotateWithCamera && _input == Vector3.zero)) && _rotateTarget ? _rotateTarget.forward : MoveDirection;
                dir = _rotateTarget.forward;
            }
            else
            {
                dir = MoveDirection;
            }

            RotateToDirection(dir);
        }

        public void UpdateMoveDirection(Transform referenceTransform = null)
        {
            //ī�޶� �ְ� ���� ��� ȸ���� �ƴҰ��
            if (referenceTransform != null && !RotateByWorld)
            {
                var right = referenceTransform.right;
                right.y = 0;
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                MoveDirection = (InputSmooth.x * right) + (InputSmooth.z * forward);
            }
            else
            {
                MoveDirection = new Vector3(InputSmooth.x, 0, InputSmooth.z);
            }
        }

        #endregion

        public override void UpdateAnimatorParameters()
        {
            base.UpdateAnimatorParameters();
            Animator.SetFloat(AnimatorParameters.AimVertical, _aimVertical = Mathf.Clamp(Mathf.Lerp(_aimVertical, CombatInput.TargetXRotation / MaxAimAngle, AimSpeed), -1, 1));
        }

        private int[] _hitSounds = new int[] { 1003, 1004, 1005 };
        public override void TakeDamage(Damage damage)
        {
            if (_isEvasion)
            {
                return;
            }
            var index = Random.Range(0, _hitSounds.Length);
            var tableID = _hitSounds[index];
            //pc hit ����.
            SoundManager.Instance.Play(_hitEffectSoundID);
            SoundManager.Instance.Play(_hitSoundID);
            base.TakeDamage(damage);
            onCancelAction.Invoke();
            //Animator.SetTrigger(_triggerReactionHash);
            //Animator.SetInteger(_reactionIDHash,hitVec);
        }

        private void HPChange(float currentHP)
        {
            if (_hpUI != null)
                _hpUI.SetValue(MaxHealth, CurrentHealth);
        }

        public override void ParrySuccess(Damage damage)
        {
            OnParrySuccess.Invoke(damage);
        }

        public override void GuardSuccess(Damage damage)
        {
            OnGuardSuccess.Invoke(damage);
        }

        #region Evasion

        public void Evation()
        {
            EvasionSetParameter();
            //ȸ�����̰ų� ��ǲ�� ���ٸ� ����
            if (_isEvasion || EvasionMoveDirection == Vector3.zero) { return; }
            if (_evasionCount <= 0) { return; }

            // ȸ�� ���� ����
            //����Ʈ �� ���� �� ��ƼŬ ����
            //�̵� ���� ���� �� ���¹̳� ����
            SetEvasionCount(-1);
            //SetStamina(EvasionStamina);
            _isEvasion = true;
            CantInput = true;
            CombatInput.CombatManager.UnEquipBow();

            //��ƼŬ ���� ����
            var lookVec = transform.position + EvasionMoveDirection;
            EvasionModel.transform.LookAt(lookVec);
            EffectsManager.Get().SetEffect((int)TypeEffects.EVASION_TRANSFORM, transform.position, EvasionModel.transform.forward, null, 2f);
            SetDefaultModelVisible(false);
            PlayEvasionParticle(true);
            //��ƼŬ ����
            //StopCharacter();
            _evasionProgress = 0;
            Animator.ResetTrigger(AnimatorParameters.Evasion);
            Animator.SetTrigger("ResetTrigger");
            //ĳ���� ������ ī�޶� ���� �������� ����
            if (StrafeSpeed.RotateWithCamera)
            {
                RotateToDirection(_rotateTarget.forward, 100f);
            }

            //���� ���
            SoundManager.Instance.Play(_evantionSoundNum, transform.position, false, true);
            onEvasionEvent.Invoke();
            OnFixedUpdate += EvasionMove;
        }

        public void EvasionMove()
        {
            _evasionProgress += Time.fixedDeltaTime * GameTimeManager.Instance.WorldTimeScale;
            if (_evasionProgress >= EvasionTime)
            {

                Animator.SetTrigger(AnimatorParameters.Evasion);
                PlayEvasionParticle(false);
                _isEvasion = false;
                CantInput = false;
                Rigidbody.velocity = Vector3.zero;
                EffectsManager.Get().SetEffect((int)TypeEffects.EVASION_TRANSFORM, transform.position, EvasionModel.transform.forward, null, 2f);
                SetDefaultModelVisible(true);
                OnFixedUpdate -= EvasionMove;
            }
        }

        private void SetDefaultModelVisible(bool visible)
        {
            var renderers = DefaultModel.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = visible;
            }
        }

        private void PlayEvasionParticle(bool value)
        {
            //RotateToDirection(_rotateTarget.forward);
            var particles = EvasionModel.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particle in particles)
            {
                if (value)
                {
                    particle.Play();
                }
                else
                {
                    particle.Stop();
                }
            }
        }

        protected void EvasionSetParameter()
        {
            EvasionMoveDirection = MoveDirection.normalized;
            EvasionRotateDirection = _input.normalized;

            if (StrafeSpeed.RotateWithCamera)
            {
                Animator.SetFloat(AnimatorParameters.EvasionInputHorizontal, EvasionRotateDirection.x);
                Animator.SetFloat(AnimatorParameters.EvasionInputVertical, EvasionRotateDirection.z);
            }
            else
            {
                Animator.SetFloat(AnimatorParameters.EvasionInputHorizontal, 0);
                Animator.SetFloat(AnimatorParameters.EvasionInputVertical, 1f);
            }
        }

        private void EvasionCountRecovery()
        {
            if (_evasionCount >= _evasionMaxCount)
            {
                return;
            }

            _currentEvasionDelayTime += Time.fixedDeltaTime * GameTimeManager.Instance.WorldTimeScale;
            if (_currentEvasionDelayTime >= EvasionDelay)
            {
                _currentEvasionDelayTime = 0f;
                if (_staminaUI != null)
                {
                    SetEvasionCount(1);
                }
            }
        }

        #endregion

        protected void SetStamina(float useStamina)
        {
            ReduceStamima(useStamina, false);
            _currentStamina -= RollStamina;
            SetStaminaRecoveryDelay(StaminaRecoveryDelay);
            //GetStanimaUI().SetValue(MaxStamina, _currentStamina);
        }

        public override void StaminaRecovery()
        {
            base.StaminaRecovery();
            if (_staminaUI != null)
            {
                //_staminaUI.SetValue(MaxStamina, _currentStamina);
            }
        }

        private void SetEvasionCount(int value)
        {
            _evasionCount += value;
            if (_staminaUI != null)
            {
                _staminaUI.SetValue(_evasionCount);
            }
        }
    }
}

