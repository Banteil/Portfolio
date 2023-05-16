using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Zeus
{
    public enum AIMovementSpeed
    {
        Idle,
        Walking,
        Running,
        Sprinting
    }

    public abstract class AIMotor : Character, IAnimatorStateInfoController
    {
        #region AI VARIABLES

        #region Inspector Variables

        [EditorToolbar("Start")] 
        public bool DisableControllerOnStart;
        public bool SuperArmorState;

        [EditorToolbar("Movement", order = 1)]
        [SerializeField]
        protected AIMovementSpeed _currentSpeed;

        //[Tooltip("Change the velocity of all animations")]
        //public float AnimatorSpeed = 1;
        [Tooltip("Smooth the  InputMagniture Animator parameter Update")]
        public float InputMagnitudeSmooth = 0.2f;

        [vHelpBox(
            "When checked, make sure to reset the speed values to 1 to use the root motion original speed, increase or decrease this value to have extraSpeed",
            vHelpBoxAttribute.MessageType.Info)]
        [Tooltip(
            "Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
        public bool UseRootMotion;

        public MovementSpeedInfo FreeSpeed, StrafeSpeed;

        [vHelpBox("Check this options only if the Agent needs to walk on complex meshes.",
            vHelpBoxAttribute.MessageType.Info)]
        [EditorToolbar("Step Offset", order = 2)]
        [SerializeField]
        protected bool _useStepOffSet = true;

        [zHideInInspector("useStepOffSet")]
        [Tooltip("ADJUST IN PLAY MODE - Offset height limit for sters - GREY Raycast in front of the legs")]
        [SerializeField]
        protected float _stepOffsetEnd = 0.45f;

        [zHideInInspector("useStepOffSet")]
        [Tooltip(
            "ADJUST IN PLAY MODE - Offset height origin for sters, make sure to keep slight above the floor - GREY Raycast in front of the legs")]
        [SerializeField]
        protected float _stepOffsetStart = 0.05f;

        [zHideInInspector("useStepOffSet")]
        [Tooltip("Higher value will result jittering on ramps, lower values will have difficulty on steps")]
        [SerializeField]
        protected float _stepSmooth = 4f;

        [EditorToolbar("Ground", order = 3)]
        public float CheckGroundDistanceVariable = 0.3f;

        [vHelpBox("Make sure to bake the navmesh and use the correct layer on your ground mesh.",
            vHelpBoxAttribute.MessageType.Info)]
        public LayerMask GroundLayer = 1 << 0;

        [SerializeField][Range(0, 1)] protected float _headDetectStart = 0.4f;
        [SerializeField] protected float _headDetectHeight = 0.4f;
        [SerializeField] protected float _headDetectMargin = 0.02f;

        [EditorToolbar("Events")]
        public UnityEvent OnAfterDieEvent;
        public UnityEvent OnEnableController;
        public UnityEvent OnDisableController;

        #endregion

        #region Hide Inspector Variables

        public bool IsStrafing { get; private set; }
        protected bool IsGrounded { get; private set; }

        [HideInInspector] public PhysicMaterial FrictionPhysics, MaxFrictionPhysics, SlippyPhysics;
        [HideInInspector] public Vector3 TargetDirection;
        [HideInInspector] public Vector3 Input;
        [HideInInspector] public bool LockMovement, LockRotation, StopMove;

        public AnimatorStateInfos AnimatorStateInfos { get; private set; }

        public AIMovementSpeed MovementSpeed
        {
            get => _currentSpeed;
            protected set => _currentSpeed = value;
        }

        public bool UseCustomRotationSpeed { get; set; }
        public float CustomRotationSpeed { get; set; }

        private UnityEvent _onUpdateAI = new();
        private bool _isStrafingRef;
        private bool _isGroundedRef;
        private float _verticalVelocityRef;
        private float _groundDistanceRef;

        [HideInInspector] public bool CustomAction;

        #endregion

        #region Protected Variables

        protected float _direction;
        protected float _speed;
        protected float _strafeMagnitude;
        protected float _verticalVelocity;
        protected Vector3 _temporaryDirection;
        protected float _temporaryDirectionTime;

        protected float _velocity;
        protected float _colliderHeight;
        protected RaycastHit _groundHit;
        protected Quaternion _freeRotation;
        protected Vector3 _lastCharacterAngle;
        protected bool _deadProcess;
        protected RuntimeAnimatorController _runtimeAni;

        #endregion

        #region Animator Variables

        private AnimatorStateInfo BaseLayerInfo, RightArmInfo, LeftArmInfo, FullBodyInfo, UpperBodyInfo, UnderBodyInfo;

        protected bool IsMainRuntimeAnimation => Animator.runtimeAnimatorController == _runtimeAni;

        #endregion

        #endregion

        #region PROTECTED VIRTUAL METHODS.UNITY

        protected virtual void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                CapsuleCollider = GetComponent<CapsuleCollider>();
                _colliderHeight = CapsuleCollider.height;
            }

            Color color;
            // radius of SphereCast
            var radius = CapsuleCollider.radius + _headDetectMargin;
            // Position of SphereCast origin stating in base of capsule
            var pos = transform.position + Vector3.up * _colliderHeight * _headDetectStart;
            // ray for SphereCast
            var ray2 = new Ray(pos, Vector3.up);
            if (!Application.isPlaying && Physics.SphereCast(ray2, radius,
                    (_headDetectHeight + (CapsuleCollider.radius))))
            {
                color = Color.red;
            }
            else
            {
                color = Color.green;
            }

            color.a = 0.4f;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(pos + Vector3.up * ((_headDetectHeight + (CapsuleCollider.radius))), radius);
        }

        protected override void Init()
        {
            base.Init();
            if (Animator)
            {
                AnimatorStateInfos = new AnimatorStateInfos(Animator);
                AnimatorStateInfos.RegisterListener();

                _triggerResetStateHash = new AnimatorParameter(Animator, "ResetState");
                _recoilIDHash = new AnimatorParameter(Animator, "RecoilID");
                _triggerRecoilHash = new AnimatorParameter(Animator, "TriggerRecoil");
                _runtimeAni = Animator.runtimeAnimatorController;
            }

            // slides the character through walls and edges
            FrictionPhysics = new PhysicMaterial
            {
                name = "frictionPhysics",
                staticFriction = .25f,
                dynamicFriction = .25f,
                frictionCombine = PhysicMaterialCombine.Multiply
            };

            // prevents the collider from slipping on ramps
            MaxFrictionPhysics = new PhysicMaterial
            {
                name = "maxFrictionPhysics",
                staticFriction = 1f,
                dynamicFriction = 1f,
                frictionCombine = PhysicMaterialCombine.Maximum
            };

            // air physics 
            SlippyPhysics = new PhysicMaterial
            {
                name = "slippyPhysics",
                staticFriction = 0f,
                dynamicFriction = 0f,
                frictionCombine = PhysicMaterialCombine.Minimum
            };

            TargetDirection = transform.forward;
            _colliderHeight = CapsuleCollider.height;
            //currentHealth = maxHealth;
            var AllColliders = GetComponentsInChildren<Collider>();
            foreach (var collider in AllColliders)
            {
                Physics.IgnoreCollision(CapsuleCollider, collider);
            }

            IsGroundedAnim = IsGrounded = true;
            if (DisableControllerOnStart)
            {
                DisableAIController();
            }
        }


        private void OnEnable()
        {
            if (AnimatorStateInfos != null && Animator != null)
            {
                AnimatorStateInfos.RegisterListener();
            }
        }

        private void OnDisable()
        {
            if (AnimatorStateInfos != null && Animator)
            {
                AnimatorStateInfos.RemoveListener();
            }
        }

        protected override void Update()
        {
            base.Update();
            UpdateAI();
        }
        #endregion

        #region PROTECTED VIRTUAL METHODS.Update AI

        protected virtual void UpdateAI()
        {
            UpdateLocomotion();
            UpdateAnimator();
            AnimatorDeath();
            _onUpdateAI.Invoke();
        }

        protected virtual void SetMovementInput(Vector3 input, float smooth)
        {
            TargetDirection = transform.TransformDirection(input).normalized;
            Input = Vector3.Lerp(Input, input, smooth * GameTimeManager.Instance.DeltaTime);
        }

        protected virtual void SetMovementInput(Vector3 input, Vector3 targetDirection, float smooth)
        {
            TargetDirection = targetDirection.normalized;
            Input = Vector3.Lerp(Input, input, smooth * GameTimeManager.Instance.DeltaTime);
        }

        protected virtual void UpdateLocomotion()
        {
            if (!IsMainRuntimeAnimation) return;
            StepOffset();
            ControlLocomotion();
            PhysicsBehaviour();
            CheckGroundDistance();
        }

        protected virtual void ControlLocomotion()
        {
            if (IsDead || !IsGrounded || IsStop)
            {
                return;
            }

            CalculateRotationMagnitude();

            if (IsStrafing)
            {
                StrafeMovement();
            }
            else
            {
                FreeMovement();
            }

            if (_temporaryDirectionTime > 0) _temporaryDirectionTime -= GameTimeManager.Instance.DeltaTime;
        }

        protected virtual void StrafeMovement()
        {
            StrafeLimitSpeed(MaxSpeed);
            if (StopMove)
            {
                _strafeMagnitude = 0f;
            }

            var rotDir = TargetDirection.normalized;
            rotDir.y = 0;
            if (rotDir.magnitude > 0.1f && Input.magnitude > 0.1f)
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

        protected virtual void CalculateRotationMagnitude()
        {
            var eulerDifference = this.transform.eulerAngles - _lastCharacterAngle;
            var magnitude = (eulerDifference.NormalizeAngle().y /
                             (IsStrafing ? StrafeSpeed.RotationSpeed : FreeSpeed.RotationSpeed));
            _lastCharacterAngle = transform.eulerAngles;
        }

        protected virtual void StrafeLimitSpeed(float value)
        {
            var speed = Mathf.Clamp(Input.z, -MaxSpeed, MaxSpeed);
            var direction = Mathf.Clamp(Input.x, -MaxSpeed, MaxSpeed);
            _speed = speed;
            _direction = direction;
            var newInput = new Vector2(_speed, _direction);
            _strafeMagnitude = Mathf.Clamp(newInput.magnitude, 0, MaxSpeed);
        }

        protected virtual float RotationSpeed
        {
            get
            {
                if (LockRotation)
                {
                    return 0f;
                }
                else
                {
                    return UseCustomRotationSpeed ? CustomRotationSpeed :
                        IsStrafing ? StrafeSpeed.RotationSpeed : FreeSpeed.RotationSpeed;
                }
            }
        }

        protected virtual void FreeMovement()
        {
            if (!Animator)
            {
                return;
            }

            // set speed to both vertical and horizontal inputs
            _speed = Mathf.Abs(Input.x) + Mathf.Abs(Input.z);
            //Limit speed by movementSpeedType
            _speed = Mathf.Clamp(_speed, 0, MaxSpeed);
            if (StopMove)
            {
                _speed = 0f;
            }

            Animator.SetFloat("InputMagnitude", _speed, InputMagnitudeSmooth,
                GameTimeManager.Instance.DeltaTime);

            if (Input.magnitude > 0.1f && TargetDirection.magnitude > 0.2f && !CustomAction)
            {
                Vector3 lookDirection = TargetDirection.normalized;
                _freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
                var eulerY = _freeRotation.eulerAngles.y;
                var euler = new Vector3(transform.eulerAngles.x, eulerY, transform.eulerAngles.z);
                if (!LockRotation && _speed > 0.1f)
                {
                    Rotate(Quaternion.Euler(euler));
                }
            }
            else
            {
                if (_temporaryDirectionTime > 0)
                {
                    Rotate(_temporaryDirection);
                }
            }
        }

        protected virtual void Rotate(Vector3 targetDirection)
        {
            targetDirection.y = 0f;
            if (targetDirection.magnitude > 0.1f)
                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(targetDirection, Vector3.up),
                    RotationSpeed * GameTimeManager.Instance.DeltaTime);
        }

        protected virtual void Rotate(Quaternion targetRotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation,
                RotationSpeed * GameTimeManager.Instance.DeltaTime);
        }

        protected virtual bool IsSprinting => MovementSpeed == AIMovementSpeed.Sprinting;

        protected virtual float MaxSpeed
        {
            get
            {
                switch (MovementSpeed)
                {
                    case AIMovementSpeed.Idle:
                        return 0;
                    case AIMovementSpeed.Walking:
                        return 0.5f;
                    case AIMovementSpeed.Running:
                        return 1;
                    case AIMovementSpeed.Sprinting:
                        return 1.5f;
                    default:
                        return 0;
                }
            }
        }

        protected virtual void StepOffset()
        {
            if (Input.sqrMagnitude < 0.1 || !IsGrounded || !_useStepOffSet || IsStop ||
                CapsuleCollider == null)
            {
                return;
            }

            var _hit = new RaycastHit();
            var _movementDirection = IsStrafing && Input.magnitude > 0
                ? (transform.right * Input.x + transform.forward * Input.z).normalized
                : transform.forward;
            Ray rayStep =
                new Ray(
                    (transform.position + new Vector3(0, _stepOffsetEnd, 0) +
                     _movementDirection * ((CapsuleCollider).radius + 0.05f)), Vector3.down);

            if (Physics.Raycast(rayStep, out _hit, _stepOffsetEnd - _stepOffsetStart, GroundLayer))
            {
                if (_hit.point.y >= (transform.position.y) && _hit.point.y <= (transform.position.y + _stepOffsetEnd))
                {
                    var speed = IsStrafing ? Mathf.Clamp(Input.magnitude, 0, 1f) : _speed;
                    var velocityDirection = (_hit.point - transform.position);
                    var vel = Rigidbody.velocity;
                    vel.y = (velocityDirection * (_stepSmooth * (speed * (_velocity > 1 ? _velocity : 1)))).y;
                    Rigidbody.velocity = vel;
                }
            }
        }

        protected virtual void CheckGroundDistance()
        {
            if (CapsuleCollider != null && (Rigidbody.velocity.y > 0.1f || Rigidbody.velocity.y < -0.1f))
            {
                var dist = 10f;

                if (Physics.Raycast(transform.position + transform.up * (CapsuleCollider.height * 0.5f), Vector3.down,
                        out _groundHit, CapsuleCollider.height, GroundLayer))
                {
                    dist = transform.position.y - _groundHit.point.y;
                }
                else if (Physics.SphereCast(transform.position + transform.up * CapsuleCollider.radius,
                             CapsuleCollider.radius * 0.5f, Vector3.down, out _groundHit, CheckGroundDistanceVariable,
                             GroundLayer))
                {
                    dist = transform.position.y - _groundHit.point.y;
                }

                GroundDistanceAnim = dist;

                if (dist >= CheckGroundDistanceVariable)
                {
                    IsGrounded = false;
                    _verticalVelocity = Rigidbody.velocity.y;
                }

                if (!CustomAction && dist < CheckGroundDistanceVariable * 0.9f)
                {
                    IsGrounded = true;
                }
            }
            else
            {
                GroundDistanceAnim = 0f;
                IsGrounded = true;
            }
        }

        protected virtual void PhysicsBehaviour()
        {
            if (CapsuleCollider == null) return;

            if (IsGrounded && Input == Vector3.zero)
            {
                CapsuleCollider.material = MaxFrictionPhysics;
            }
            else if (IsGrounded && Input != Vector3.zero)
            {
                CapsuleCollider.material = UseRootMotion ? MaxFrictionPhysics : FrictionPhysics;
            }
            else
            {
                CapsuleCollider.material = SlippyPhysics;
            }
        }

        public override bool IsDead
        {
            get => base.IsDead;
            set
            {
                base.IsDead = value;
                if (value)
                {
                    if (IsGrounded)
                    {
                        //if (Rigidbody) Rigidbody.isKinematic = true;
                        //if (CapsuleCollider) CapsuleCollider.enabled = false;
                        OnDeadEvent.Invoke(gameObject);
                    }
                }

                if (Animator && IsMainRuntimeAnimation)
                {
                    Animator.SetBool("IsDead", value);
                }
            }
        }

        #endregion

        #region PROTECTED VIRTUAL METHODS.Update Animator

        protected virtual void UpdateAnimator()
        {
            if (Animator == null || !Animator.isActiveAndEnabled || !IsMainRuntimeAnimation)
            {
                return;
            }

            AnimatorLayerControl();
            AnimatorLocomotion();
            ActionsControl();
        }

        protected virtual bool IsStrafingAnim
        {
            get => _isStrafingRef;
            set
            {
                if (_isStrafingRef == value && Animator.GetBool("IsStrafing") == value) return;
                _isStrafingRef = value;
                Animator.SetBool("IsStrafing", value);
            }
        }

        protected virtual bool IsGroundedAnim
        {
            get => _isGroundedRef;
            set
            {
                if (_isGroundedRef == value) return;
                _isGroundedRef = value;
                Animator.SetBool("IsGrounded", value);
            }
        }

        protected virtual float GroundDistanceAnim
        {
            get => _groundDistanceRef;
            set
            {
                if (_groundDistanceRef != value)
                {
                    _groundDistanceRef = value;
                    Animator.SetFloat("GroundDistance", value);
                }
            }
        }

        protected float VerticalVelocityAnim
        {
            get => _verticalVelocityRef;
            set
            {
                if (_verticalVelocityRef != value)
                {
                    _verticalVelocityRef = value;
                    Animator.SetFloat("VerticalVelocity", value);
                }
            }
        }

        protected virtual void AnimatorLocomotion()
        {
            var canMove = !StopMove && !LockMovement && !AnimatorStateInfos.HasTag("LockMovement");
            Animator.SetFloat("InputHorizontal", canMove && IsStrafing && !IsSprinting ? _direction : 0f, .2f,
                GameTimeManager.Instance.DeltaTime);
            Animator.SetFloat("InputVertical", canMove ? _speed : 0f, .2f, GameTimeManager.Instance.DeltaTime);

            IsStrafingAnim = IsStrafing;
            IsGroundedAnim = IsGrounded;
            VerticalVelocityAnim = _verticalVelocity;
        }

        private void AnimatorLayerControl()
        {
            if (AnimationStateTable.BaseLayer != -1)
            {
                BaseLayerInfo = Animator.GetCurrentAnimatorStateInfo(AnimationStateTable.BaseLayer);
            }

            if (AnimationStateTable.UnderBodyLayer != -1)
            {
                UnderBodyInfo = Animator.GetCurrentAnimatorStateInfo(AnimationStateTable.UnderBodyLayer);
            }

            if (AnimationStateTable.RightArmLayer != -1)
            {
                RightArmInfo = Animator.GetCurrentAnimatorStateInfo(AnimationStateTable.RightArmLayer);
            }

            if (AnimationStateTable.LeftArmLayer != -1)
            {
                LeftArmInfo = Animator.GetCurrentAnimatorStateInfo(AnimationStateTable.LeftArmLayer);
            }

            if (AnimationStateTable.UpperBodyLayer != -1)
            {
                UpperBodyInfo = Animator.GetCurrentAnimatorStateInfo(AnimationStateTable.UpperBodyLayer);
            }

            if (AnimationStateTable.FullBodyLayer != -1)
            {
                FullBodyInfo = Animator.GetCurrentAnimatorStateInfo(AnimationStateTable.FullBodyLayer);
            }
        }

        private void AnimatorDeath()
        {
            if (!IsDead || _deadProcess) return;

            switch (DeathByType)
            {
                // death by animation
                case DeathBy.Animation:
                    {
                        var deadLayer = 0;
                        var info = AnimatorStateInfos.GetStateInfoUsingTag("Dead");
                        if (info != null)
                        {
                            if (!Animator.IsInTransition(deadLayer) && info.NormalizedTime >= 0.99f &&
                                GroundDistanceAnim <= 0.15f)
                            {
                                AfterDieProcess();
                            }
                        }
                        break;
                    }
                // death by animation & ragdoll after a time
                case DeathBy.AnimationWithRagdoll:
                    {
                        var deadLayer = 0;
                        var info = AnimatorStateInfos.GetStateInfoUsingTag("Dead");
                        if (info != null)
                        {
                            if (!Animator.IsInTransition(deadLayer) && info.NormalizedTime >= 0.8f &&
                                GroundDistanceAnim <= 0.15f)
                            {
                                OnActiveRagdollEvent.Invoke(null);
                                AfterDieProcess();
                            }
                        }
                        break;
                    }
                // death by ragdoll
                case DeathBy.Ragdoll:
                    {
                        OnActiveRagdollEvent.Invoke(null);
                        AfterDieProcess();
                        break;
                    }
                case DeathBy.Dissolve:
                    {
                        _deadProcess = true;

                        var colliders = GetComponentsInChildren<Collider>();
                        foreach (var col in colliders)
                        {
                            if (col.Equals(CapsuleCollider))
                            {
                                continue;
                            }

                            Destroy(col);
                        }

                        var renderers = gameObject.GetComponentsInChildren<Renderer>();
                        if (renderers != null)
                        {
                            foreach (var renderer in renderers)
                            {
                                foreach (var item in renderer.materials)
                                {
                                    if (item.HasProperty("_AdvancedDissolveCutoutStandardClip"))
                                    {
                                        item.DOFloat(1f, "_AdvancedDissolveCutoutStandardClip", 2f).SetEase(Ease.InSine)
                                            .onComplete = AfterDieProcess;
                                    }
                                }
                            }
                        }

                        break;
                    }
            }
        }

        public virtual void AfterDieProcess()
        {
            switch (AfterDieType)
            {
                case AfterDie.RemoveComponents:
                    {
                        var colliders = GetComponentsInChildren<Collider>(true);
                        foreach (var col in colliders)
                        {
                            Destroy(col);
                        }

                        var rigidBodies = GetComponentsInChildren<Rigidbody>(true);
                        foreach (var rigidBody in rigidBodies)
                        {
                            Destroy(rigidBody);
                        }

                        var animators = GetComponentsInChildren<Animator>(true);
                        foreach (var animator in animators)
                        {
                            Destroy(animator);
                        }

                        var navMeshAgents = GetComponentsInChildren<NavMeshAgent>(true);
                        foreach (var navMesh in navMeshAgents)
                        {
                            Destroy(navMesh);
                        }

                        var comps = GetComponentsInChildren<MonoBehaviour>(true);
                        for (int i = 0; i < comps.Length; i++)
                        {
                            Destroy(comps[i]);
                        }

                        break;
                    }
                case AfterDie.DestroySelf:
                    {
                        Destroy(gameObject);
                        break;
                    }
                case AfterDie.InactiveSelf:
                    {
                        gameObject.SetActive(false);
                        break;
                    }
            }

            OnAfterDieEvent?.Invoke();
        }

        private void ControlSpeed(float velocity)
        {
            if (GameTimeManager.Instance.DeltaTime == 0)
            {
                return;
            }

            var canMove = !StopMove && !LockMovement && !AnimatorStateInfos.HasTag("LockMovement");
            if (!canMove)
            {
                velocity = 0;
            }

            var rigidVelocity = Rigidbody.velocity;
            if (UseRootMotion && !CustomAction && canMove)
            {
                _velocity = velocity;
                var animatorDeltaPos = Animator.deltaPosition;
                var deltaPosition = new Vector3(animatorDeltaPos.x, transform.position.y, animatorDeltaPos.z);
                var v = (deltaPosition * (velocity > 0 ? velocity : 1f)) / GameTimeManager.Instance.DeltaTime;

                v.y = rigidVelocity.y;
                Rigidbody.velocity = Vector3.Lerp(rigidVelocity, v, 20f * GameTimeManager.Instance.DeltaTime);
            }
            else if (IsDead || !canMove || CustomAction)
            {
                _velocity = velocity;
                var v = Vector3.zero;
                v.y = Rigidbody.velocity.y;
                Rigidbody.velocity = v;
                Rigidbody.position = Animator.rootPosition;
            }
            else
            {
                if (IsStrafing)
                {
                    var v = (transform.TransformDirection(new Vector3(Input.x, 0, Input.z)) *
                                 (velocity > 0 ? velocity : 1f));
                    v.y = rigidVelocity.y;
                    Rigidbody.velocity = Vector3.Lerp(rigidVelocity, v, 20f * GameTimeManager.Instance.DeltaTime);
                }
                else
                {
                    var _targetVelocity = transform.forward * velocity * _speed;
                    _targetVelocity.y = rigidVelocity.y;
                    Rigidbody.velocity = _targetVelocity;
                }
            }
        }

        private void ActionsControl()
        {
            UpdateLockMovement();
            UpdateLockRotation();
            UpdateCustomAction();
        }

        private void UpdateLockMovement()
        {
            LockMovement = IsAnimatorTag("LockMovement");
        }

        private void UpdateLockRotation()
        {
            LockRotation = IsAnimatorTag("LockRotation");
        }

        public virtual void UpdateCustomAction()
        {
            CustomAction = IsAnimatorTag("CustomAction");
        }

        protected virtual void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (!Animator || !IsGrounded) return;

            var rigidVelocity = Rigidbody.velocity;
            // use root rotation for custom actions or 
            if (CustomAction || Input.magnitude < 0.1f)
            {
                Rigidbody.velocity.Set(rigidVelocity.x, 0, rigidVelocity.z);
                Rigidbody.position = Animator.rootPosition;
                transform.rotation = Animator.rootRotation;
                return;
            }

            if (LockMovement)
            {
                Rigidbody.velocity.Set(rigidVelocity.x, 0, rigidVelocity.z);
                Rigidbody.position = Animator.rootPosition;
                if (LockRotation)
                {
                    transform.rotation = Animator.rootRotation;
                }
                return;
            }

            if (LockRotation)
            {
                transform.rotation = Animator.rootRotation;
            }

            var a_strafeSpeed = Mathf.Abs(_strafeMagnitude);
            switch (IsStrafing)
            {
                // strafe extra speed
                case true when a_strafeSpeed <= 0.5f:
                    ControlSpeed(StrafeSpeed.WalkSpeed);
                    break;
                case true when a_strafeSpeed is > 0.5f and <= 1f:
                    ControlSpeed(StrafeSpeed.RunningSpeed);
                    break;
                case true:
                    ControlSpeed(StrafeSpeed.SprintSpeed);
                    break;
                // free extra speed                
                case false when _speed <= 0.5f:
                    ControlSpeed(FreeSpeed.WalkSpeed);
                    break;
                case false when _speed > 0.5 && _speed <= 1f:
                    ControlSpeed(FreeSpeed.RunningSpeed);
                    break;
                case false:
                    ControlSpeed(FreeSpeed.SprintSpeed);
                    break;
            }
        }

        #endregion

        #region PUBLIC VIRTUAL METHODS. AI

        /// <summary>
        /// Check  Current Animator State tag
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private bool IsAnimatorTag(string tag)
        {
            if (Animator == null)
            {
                return false;
            }

            if (AnimatorStateInfos.HasTag(tag))
            {
                return true;
            }

            if (BaseLayerInfo.IsTag(tag))
            {
                return true;
            }

            if (UnderBodyInfo.IsTag(tag))
            {
                return true;
            }

            if (RightArmInfo.IsTag(tag))
            {
                return true;
            }

            if (LeftArmInfo.IsTag(tag))
            {
                return true;
            }

            if (UpperBodyInfo.IsTag(tag))
            {
                return true;
            }

            if (FullBodyInfo.IsTag(tag))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set Movement speed to 0 (zero)
        /// </summary>
        public virtual void Stop()
        {
            //targetDirection = transform.forward;
            if (Input != Vector3.zero)
            {
                // _turnOnSpotDirection = transform.forward;
                Input = Vector3.zero;
            }

            MovementSpeed = AIMovementSpeed.Idle;
        }

        /// <summary>
        /// Set Movement speed to WalkSpeed
        /// </summary>
        public virtual void Walk()
        {
            MovementSpeed = AIMovementSpeed.Walking;
        }

        /// <summary>
        /// Set Movement speed to RunSpeed
        /// </summary>
        public virtual void Run()
        {
            MovementSpeed = AIMovementSpeed.Running;
        }

        /// <summary>
        /// Set Movement speed to SprintSpeed
        /// </summary>
        public virtual void Sprint()
        {
            MovementSpeed = AIMovementSpeed.Sprinting;
        }

        /// <summary>
        /// Set Strafe Locomotion type
        /// </summary>
        public virtual void SetStrafeLocomotion()
        {
            IsStrafing = true;
        }

        /// <summary>
        /// Set Free Locomotion type
        /// </summary>
        public virtual void SetFreeLocomotion()
        {
            IsStrafing = false;
        }

        public virtual void EnableAIController()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            Rigidbody.isKinematic = false;
            CapsuleCollider.isTrigger = false;
            enabled = true;
            OnEnableController.Invoke();
        }

        public virtual void DisableAIController()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            TargetDirection = transform.forward;
            Input = Vector3.zero;
            MovementSpeed = AIMovementSpeed.Idle;
            if (Animator.isActiveAndEnabled)
            {
                Animator.SetFloat("InputHorizontal", 0f);
                Animator.SetFloat("InputVertical", 0f);
                Animator.SetFloat("InputMagnitude", 0f);
            }

            Debug.Log("DisableAIController");
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.isKinematic = true;
            enabled = false;
            OnDisableController.Invoke();
        }

        #endregion

        #region OVERRIDE METHODS. HealthController/ICharacter interface

        public override void TriggerDamageReaction(Damage damage)
        {
            if (ReactionPrevention || (SuperArmorState && !damage.AttackerHitProperties.UseRecoil)) return;
            base.TriggerDamageReaction(damage);
        }

        #endregion
    }
}