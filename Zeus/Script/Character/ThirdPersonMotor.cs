using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class ThirdPersonMotor : Character, IAnimatorStateInfoController
    {
        #region Variable
        [Header("Stamina")]
        public float MaxStamina = 200f;
        public float StaminaRecoveryValue = 1.2f;
        public float StaminaRecoveryDelay = 1f;
        internal float _currentStamina;
        internal float _currentStaminaRecoveryDelay;
        public float SprintStamina = 30f;
        public float EvasionStamina = 10;
        public float RollStamina = 25f;

        [Header("Event")]
        public UnityEvent OnStaminaEnd;
        #endregion

        #region Character Variable
        [Header("Movement")]
        public float SpeedMultiplier = 1;
        public bool RotateByWorld = false;

        public MovementSpeed FreeSpeed, StrafeSpeed;

        //불필요한지 확인
        public bool UseRootMotion = false;

        //불필요한지 확인
        [Range(0.01f, 0.1f)]
        public float LeanSmooth = 0.05f;

        [Header("Falling")]
        [Tooltip("Apply extra gravity when the character is not grounded")]
        public float ExtraGravity = -10f;

        [Header("Evasion")]
        protected bool _isEvasion;
        protected float _evasionProgress;
        public AnimationCurve EvasionMoveCurve = new AnimationCurve();
        public float EvasionTime;
        public float EvasionSpeed;

        public enum TypeGroundCheckMethod
        {
            Low, High
        }

        [Header("Ground")]
        public LayerMask GroundLayer = 1 << 0;
        [Tooltip("Ground Check Method To check ground Distance and ground angle\n*Simple: Use just a single Raycast\n*Normal: Use Raycast and SphereCast\n*Complex: Use SphereCastAll")]
        public TypeGroundCheckMethod GroundCheckMethod = TypeGroundCheckMethod.High;
        public float GroundDetectionDistance = 10f;
        [Range(0, 10)]
        public float GroundMinDistance = 0.1f;
        [Range(0, 10)]
        public float GroundMaxDistance = 0.5f;
        internal float _stopMoveWeight;
        internal float _sprintWeight;
        internal float _groundDistance;
        public RaycastHit GroundHit;

        public bool debugWindow;
        public AnimatorStateInfos _animatorStateInfos;
        public AnimatorStateInfos AnimatorStateInfos { get => _animatorStateInfos; protected set => _animatorStateInfos = value; }

        #endregion

        #region Action

        public bool IsStrafing
        {
            get
            {
                return _isStrafing;
            }

            set
            {
                _isStrafing = value;
            }
        }

        public bool IsGrounded { get; set; }
        public bool DisableCheckGround { get; set; }
        public bool IsSprinting { get; set; }

        protected bool _cantInput;

        public bool CantInput
        {
            get
            {
                if (_cantInputToAnimator || _cantInput)
                {
                    return true;
                }
                return false;
            }

            set
            {
                _cantInput = value;
            }
        }


        #region AnimationTags

        internal bool
            LookForwardInRoll,
            _cantInputToAnimator,
            _cantInputExceptionMove,
            IsAttack,
            IsCantAttack,
            _useHandIK,
            _ignoreHandIK,
            IsParrySuccess,
            NotMoveToRootMotion
            ;

        internal bool _customAction;//애니메이션 쪽에서 커스텀액션인지 판별하기 위해 사용하는 것으로 추정(추가적으로 무슨 기능인지 찾아봐야됨
        #endregion

        #endregion

        #region HideVariable

        internal float _defaultSpeedMultiplier = 1;
        internal float _inputMagnitude;
        internal float _rotationMagnitude;
        internal float _verticalSpeed;
        internal float _horizontalSpeed;
        internal float _aimVerticalSpeed;
        internal float _moveSpeed;
        internal float _verticalVelocity;
        internal float _colliderRadius, _colliderHeight;
        internal float _heightReached;
        internal bool _lockMovement = false;
        internal bool _lockRotation = false;
        internal bool _lockSetMoveSpeed = false;
        internal bool _isStrafing;
        [HideInInspector]
        public bool ApplyingStepOffset;
        protected internal bool _customActionRotateAble;
        protected internal bool _animRootMotion;
        protected internal bool _lockAnimMovement;// internaly used with the vAnimatorTag("LockMovement"), use on the animator to lock the movement of a specific animation clip
        protected internal bool _lockAnimRotation;
        [SerializeField]
        protected Vector3 _lastCharacterAngle;//Last angle of the character used to calculate rotationMagnitude;
        [SerializeField]
        internal Transform _rotateTarget;
        [SerializeField]
        internal Vector3 _input;
        internal Vector3 _oldInput;
        internal Vector3 _colliderCenter;
        public Vector3 InputSmooth;
        public Vector3 MoveDirection;
        //회피시 방향
        public Vector3 EvasionMoveDirection;
        //회피시 인풋 방향
        public Vector3 EvasionRotateDirection;
        internal AnimatorStateInfo _baseLayerInfo, _underBodyInfo, _rightArmInfo, _leftArmInfo, _fullBodyInfo, _upperBodyInfo;

        public int BaseLayer { get { return Animator.GetLayerIndex("Base Layer"); } }
        public int UnderBodyLayer { get { return Animator.GetLayerIndex("UnderBody"); } }
        public int RightArmLayer { get { return Animator.GetLayerIndex("RightArm"); } }
        public int BowUpperLayer { get { return Animator.GetLayerIndex("BowUpperLayer"); } }
        public int UpperBodyLayer { get { return Animator.GetLayerIndex("UpperBodyLayer"); } }
        public int FullBodyLayer { get { return Animator.GetLayerIndex("FullBody"); } }


        public float ColliderRaidusDefault
        {
            get; protected set;
        }

        public float ColliderHeightDefault
        {
            get; protected set;
        }

        public Vector3 ColliderCenterDefault
        {
            get; protected set;
        }
        #endregion

        #region Components
        internal PhysicMaterial _frictionPhysics, _maxFrictionPhysics, _slippyPhysics;         // create PhysicMaterial for the Rigidbody
        public PhysicMaterial CurrentMaterialPhysics { get; protected set; }
        #endregion

        protected override void Init()
        {
            base.Init();
            Animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            _frictionPhysics = new PhysicMaterial();
            _frictionPhysics.name = "frictionPhysics";
            _frictionPhysics.staticFriction = .25f;
            _frictionPhysics.dynamicFriction = .25f;
            _frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;

            _maxFrictionPhysics = new PhysicMaterial();
            _maxFrictionPhysics.name = "maxFrictionPhysics";
            _maxFrictionPhysics.staticFriction = 1f;
            _maxFrictionPhysics.dynamicFriction = 1f;
            _maxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;

            _slippyPhysics = new PhysicMaterial();
            _slippyPhysics.staticFriction = 0f;
            _slippyPhysics.dynamicFriction = 0f;
            _slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;

            _colliderCenter = ColliderCenterDefault = CapsuleCollider.center;
            _colliderRadius = ColliderRaidusDefault = CapsuleCollider.radius;
            _colliderHeight = ColliderHeightDefault = CapsuleCollider.height;

            Collider[] allColliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < allColliders.Length; i++)
            {
                Physics.IgnoreCollision(CapsuleCollider, allColliders[i]);
            }

            if (FillHealthOnStart)
            {
                CurrentHealth = MaxHealth;
            }

            currentHealthRecoveryDelay = healthRecoveryDelay;
            _currentStamina = MaxStamina;
            IsGrounded = true;

            _heightReached = transform.position.y;
            IsStrafing = true;
        }

        public void UpdateMotor()//캐릭터 체력 등 상태 체크
        {
            CheckHealth();            
            CheckGround();
            CheckStamina();
            StaminaRecovery();
        }

        public override void TakeDamage(Damage damage)
        {
            if (CurrentHealth <= 0)
            {
                return;
            }

            base.TakeDamage(damage);
        }

        public void ReduceStamima(float value, bool accumulative)//스태미너 감소
        {
            if (_customAction)
            {
                return;
            }

            if (accumulative)
            {
                _currentStamina -= value * Time.fixedDeltaTime;
            }
            else
            {
                _currentStamina -= value;
            }

            if (_currentStamina < 0)
            {
                _currentStamina = 0;
                OnStaminaEnd.Invoke();
            }
        }

        public void ChangeStamina(int value)
        {
            _currentStamina += value;
            _currentStamina = Mathf.Clamp(_currentStamina, 0, MaxStamina);
        }

        public void ChangeMaxStamina(int value)
        {
            MaxStamina += value;
            if (MaxStamina < 0)
            {
                MaxStamina = 0;
            }
        }

        public void DeathBehaviour()
        {
            //플레이어 무브 인풋 락
            _lockAnimMovement = true;
            Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            Animator.SetBool("IsDead", true);
        }

        void CheckHealth()
        {
            if (IsDead && CurrentHealth > 0)
            {
                IsDead = false;
            }
        }

        void CheckStamina()
        {
            if (IsSprinting)
            {
                _currentStaminaRecoveryDelay = 0.25f;
                ReduceStamima(SprintStamina, true);
            }
        }

        public virtual void StaminaRecovery()
        {
            if (_currentStaminaRecoveryDelay > 0)
            {
                _currentStaminaRecoveryDelay -= Time.fixedDeltaTime;
            }
            else
            {
                if (_currentStamina > MaxStamina)
                {
                    _currentStamina = MaxStamina;
                }

                if (_currentStamina < MaxStamina)
                {
                    _currentStamina += StaminaRecoveryValue;
                }
            }
        }

        public  void SetStaminaRecoveryDelay(float delay)
        {
            _currentStaminaRecoveryDelay = delay;
        }

        #region Locomotion

        public void SetControllerMoveSpeed(MovementSpeed speed)
        {
            _moveSpeed = Mathf.Lerp(_moveSpeed, IsSprinting ? speed.SprintSpeed : speed.RunningSpeed, speed.MovementSmooth * Time.fixedDeltaTime);
        }

        public virtual void MoveCharacter(Vector3 direction)
        {
            if (_isEvasion)
            {
                var evasionPersent = _evasionProgress / EvasionTime;
                SpeedMultiplier = EvasionSpeed * EvasionMoveCurve.Evaluate(evasionPersent);
            }
            else
            {
                SpeedMultiplier = 1;
            }
            //매끄러운 움직임 계산

            InputSmooth = Vector3.Lerp(InputSmooth, _input, (IsStrafing ? StrafeSpeed.MovementSmooth : FreeSpeed.MovementSmooth)
                * (UseRootMotion ? Time.deltaTime : Time.fixedDeltaTime));

            //루트 모션 사용중일 경우 인풋만 받고 리턴
            if (_animRootMotion && !_isEvasion && !NotMoveToRootMotion || (CantInput && !_isEvasion))
            {
                return;
            }
            if (!IsGrounded)
            {
                return;
            }

            direction.y = 0;
            //magnitude : 벡터의 길이
            direction = direction.normalized * Mathf.Clamp(direction.magnitude, 0, 1f);
            Vector3 targetPosition = Rigidbody.position + direction *
                (_moveSpeed * SpeedMultiplier) * (UseRootMotion ? Time.deltaTime : Time.fixedDeltaTime);
            Vector3 targetVelocity = (targetPosition - transform.position) / (UseRootMotion ? Time.deltaTime : Time.fixedDeltaTime);

            targetVelocity.y = Rigidbody.velocity.y;            
            Rigidbody.velocity = targetVelocity;
        }

        public void StopCharacterWithLerp()
        {
            //IsSprinting = false;
            _sprintWeight = 0f;
            _horizontalSpeed = 0f;
            _verticalSpeed = 0f;
            MoveDirection = Vector3.zero;
            //_input = Vector3.Lerp(_input, Vector3.zero, 2f * Time.fixedDeltaTime);
            InputSmooth = Vector3.Lerp(InputSmooth, Vector3.zero, 2f * Time.fixedDeltaTime);
            Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, Vector3.zero, 4f * Time.fixedDeltaTime);
            _inputMagnitude = Mathf.Lerp(_inputMagnitude, 0f, 2f * Time.fixedDeltaTime);
            _moveSpeed = Mathf.Lerp(_moveSpeed, 0f, 2f * Time.fixedDeltaTime);


            Animator.SetFloat(AnimatorParameters.InputMagnitude, 0f, 0.2f, Time.fixedDeltaTime);
            Animator.SetFloat(AnimatorParameters.InputVertical, 0f, 0.2f, Time.fixedDeltaTime);
            Animator.SetFloat(AnimatorParameters.InputHorizontal, 0f, 0.2f, Time.fixedDeltaTime);
            Animator.SetFloat(AnimatorParameters.RotationMagnitude, 0f, 0.2f, Time.fixedDeltaTime);
        }

        public void StopCharacter()
        {
            IsSprinting = false;
            _sprintWeight = 0f;
            _horizontalSpeed = 0f;
            _verticalSpeed = 0f;
            MoveDirection = Vector3.zero;
            _input = Vector3.zero;
            InputSmooth = Vector3.zero;
            Rigidbody.velocity = Vector3.zero;
            _inputMagnitude = 0f;
            Animator.SetFloat(AnimatorParameters.InputMagnitude, 0f, 0.25f, Time.fixedDeltaTime);
            Animator.SetFloat(AnimatorParameters.InputVertical, 0f, 0.25f, Time.fixedDeltaTime);
            Animator.SetFloat(AnimatorParameters.InputHorizontal, 0f, 0.25f, Time.fixedDeltaTime);
            Animator.SetFloat(AnimatorParameters.RotationMagnitude, 0f, 0.25f, Time.fixedDeltaTime);
        }

        public void RotateToPosition(Vector3 position)
        {
            Vector3 desiredDirection = position - transform.position;
            RotateToDirection(desiredDirection.normalized);
        }

        public void RotateToDirection(Vector3 direction)
        {
            RotateToDirection(direction, IsStrafing ? StrafeSpeed.RotationSpeed : FreeSpeed.RotationSpeed);
        }

        //회전
        public void RotateToDirection(Vector3 direction, float rotationSpeed)
        {
            if(_lockAnimRotation || (_animRootMotion && IsAttack && !_customActionRotateAble))
            {
                return;
            }

            direction.y = 0f;
            if(direction.normalized.magnitude == 0)
            {
                direction = transform.forward;
            }

            var euler = transform.rotation.eulerAngles.NormalizeAngle();
            var targetEuler = Quaternion.LookRotation(direction.normalized).eulerAngles.NormalizeAngle();
            euler.y = Mathf.LerpAngle(euler.y, targetEuler.y, rotationSpeed * GameTimeManager.Instance.FixedDeltaTime);
            Quaternion _newRotation = Quaternion.Euler(euler);
            transform.rotation = _newRotation;
        }
        #endregion

        #region Evasion

        protected void EvasionBehavior()
        {
            if (!_isEvasion) { return; }
            //Debug.Log("회피");
            MoveCharacter(EvasionMoveDirection);
        }

        #endregion

        
        #region GroundCheck

        protected void CheckGround()
        {
            CheckGroundDistance();
            ControlMaterialPhysics();
            if (IsDead || _customAction || DisableCheckGround)
            {
                IsGrounded = true;
                _heightReached = transform.position.y;
                return;
            }

            if(_groundDistance <= GroundMinDistance || ApplyingStepOffset)//땅에 있는 상태
            {
                IsGrounded = true;

                if(!ApplyingStepOffset &&  _groundDistance >0.05f && ExtraGravity != 0)
                {
                    Rigidbody.AddForce(transform.up * (ExtraGravity * 2 * Time.fixedDeltaTime), ForceMode.VelocityChange);
                }

                _heightReached = transform.position.y;
            }
            else
            {
                if(_groundDistance >= GroundMaxDistance)
                {
                    IsGrounded = false;

                    _verticalVelocity = Rigidbody.velocity.y;
                    if(!ApplyingStepOffset && ExtraGravity != 0)
                    {
                        Rigidbody.AddForce(transform.up * ExtraGravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    }
                }
                else if(!ApplyingStepOffset && ExtraGravity != 0)
                {
                    Rigidbody.AddForce(transform.up * (ExtraGravity * 2 * Time.fixedDeltaTime), ForceMode.VelocityChange);
                }
            }
        }

        private void ControlMaterialPhysics()
        {
            var targetMaterialPhysics = CurrentMaterialPhysics;
            if (IsGrounded || _input.magnitude < 0.1f && targetMaterialPhysics != _maxFrictionPhysics)
            {
                targetMaterialPhysics = _maxFrictionPhysics;
            }
            else if (IsGrounded && _input.magnitude > 0.1f && targetMaterialPhysics != _frictionPhysics)
            {
                targetMaterialPhysics = _frictionPhysics;
            }
            else if (targetMaterialPhysics != _slippyPhysics && (!IsGrounded))
            {
                targetMaterialPhysics = _slippyPhysics;
            }

            if (CurrentMaterialPhysics != targetMaterialPhysics)
            {
                CapsuleCollider.material = targetMaterialPhysics;
                CurrentMaterialPhysics = targetMaterialPhysics;
            }
        }

        protected void CheckGroundDistance()
        {
            if (IsDead)
            {
                return;
            }

            if(CapsuleCollider != null)
            {
                float radius = CapsuleCollider.radius * 0.9f;
                var dist = GroundDetectionDistance;

                Ray ray2 = new Ray(transform.position + new Vector3(0, _colliderHeight / 2, 0), Vector3.down);

                if (Physics.Raycast(ray2, out GroundHit, (_colliderHeight / 2) + dist, GroundLayer) && !GroundHit.collider.isTrigger)
                {
                    dist = transform.position.y - GroundHit.point.y;
                }

                if(GroundCheckMethod == TypeGroundCheckMethod.High && dist >= GroundMinDistance)
                {
                    Vector3 pos = transform.position + Vector3.up * CapsuleCollider.radius;
                    Ray ray = new Ray(pos, -Vector3.up);
                    if(Physics.SphereCast(ray,radius, out GroundHit, CapsuleCollider.radius) && !GroundHit.collider.isTrigger)
                    {
                        Physics.Linecast(GroundHit.point + (Vector3.up), GroundHit.point + Vector3.down * 0.15f, out GroundHit, GroundLayer);
                        float newDist = transform.position.y - GroundHit.point.y;
                        if (dist > newDist)
                        {
                            dist = newDist;
                        }
                    }
                }
                _groundDistance = (float)System.Math.Round(dist, 2);
            }
        }

        public float GetGroundAngle()
        {
            var groundAngle = Vector3.Angle(GroundHit.normal, Vector3.up);
            return groundAngle;
        }

        public float GroundAngleFromDirection()
        {
            var dir = IsStrafing && _input.magnitude > 0 ? (transform.right * _input.x + transform.forward * _input.z).normalized : transform.forward;
            var movementAngle = Vector3.Angle(dir, GroundHit.normal) - 90;
            return movementAngle;
        }
        #endregion
    }

    [System.Serializable]
    public class MovementSpeed
    {
        [Range(1f, 20f)]
        public float MovementSmooth = 6f;
        [Range(0f, 1f)]
        public float AnimationSmooth = 0.2f;
        public float RotationSpeed = 20f;
        public bool WalkByDefalut = false;
        public bool RotateWithCamera = false;
        public float WalkSpeed = 2f;
        public float RunningSpeed = 4f;
        public float SprintSpeed = 6f;
    }
}

