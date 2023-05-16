using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [SelectionBase]
    public partial class ControlAI : AIMotor, IControlAI
    {
        #region Inspector Variables

        [EditorToolbar("Start")]
        public bool DisableAgentOnStart = true;
        public Vector3 LastHitPosition { get; set; }

        [EditorToolbar("Agent", order = 5)]
        [SerializeField]
        protected bool _useNavMeshAgent = true;

        [SerializeField] protected AIUpdateQuality _updatePathQuality = AIUpdateQuality.Medium;
        [SerializeField][Range(1f, 10f)] protected float _aceleration = 1f;
        [SerializeField][Range(0.05f, 10f)] protected float _stopingDistance = 0.2f;

        [Header("Increase StoppingDistance by speed")]
        [SerializeField]
        [Range(0.05f, 10f)]
        protected float _walkingStopingDistance = 0.0f;

        [SerializeField][Range(0.05f, 10f)] protected float _runningStopingDistance = 0.1f;
        [SerializeField][Range(0.05f, 10f)] protected float _sprintingStopingDistance = 0.15f;

        [EditorToolbar("Detection", order = 7)]
        [vHelpBox("Use a empty trasform inside the headBone transform as reference to the character Eyes",
            vHelpBoxAttribute.MessageType.None)]
        public Transform DetectionPointReference;

        [SerializeField] public AISightMethod SightMethodType = AISightMethod.Center | AISightMethod.Top;
        public AISightMethod SightMethod { get; set; }
        [SerializeField] protected AIUpdateQuality _findTargetUpdateQuality = AIUpdateQuality.High;
        [SerializeField] protected AIUpdateQuality _canseeTargetUpdateQuality = AIUpdateQuality.Medium;

        [SerializeField, Tooltip("find target with current target found")]
        protected bool _findOtherTarget = false;

        [SerializeField][Range(1, 100)] protected int _maxTargetsDetection = 10;
        [SerializeField] protected float _changeTargetDelay = 2f;
        [SerializeField] protected bool _findTargetByDistance = true;
        [SerializeField] protected float _fieldOfView = 90f;
        [SerializeField] protected float _minDistanceToDetect = 3f;
        [SerializeField] protected float _maxDistanceToDetect = 6f;
        [SerializeField][ReadOnly] protected bool _hasPositionOfTheTarget;
        [SerializeField][ReadOnly] protected bool _targetInLineOfSight;

        [vHelpBox("Considerer maxDistanceToDetect value + lostTargetDistance", vHelpBoxAttribute.MessageType.None)]
        [SerializeField]
        protected bool _possibleToLostTarget;

        [SerializeField] protected float _lostTargetDistance = 4f;
        [SerializeField] protected float _timeToLostWithoutSight = 5f;

        [Header("--- Layers to Detect ----")]
        [SerializeField] protected LayerMask _detectLayer;
        [SerializeField] protected TagMask _detectTags;
        [SerializeField] protected LayerMask _obstacles = 1 << 0;

        [EditorToolbar("Debug")]
        [vHelpBox("Debug Options")]
        [SerializeField]
        protected bool _debugVisualDetection;

        [SerializeField] protected bool _debugRaySight;
        [SerializeField] protected bool _debugLastTargetPosition;
        [SerializeField] protected AITarget _currentTarget;
        [SerializeField] protected AIReceivedDamegeInfo _receivedDamage = new AIReceivedDamegeInfo();

        private AIHeadtrack _headtrack;
        private Collider[] _targetsInRange;

        private Vector3 _lastTargetPosition;
        private float _lostTargetTime;
        private UnityEngine.AI.NavMeshHit _navHit;
        private float _changeTargetTime;

        public Vector3 GetInput => Input;
        public bool DoAction => CustomAction;

        public virtual void CreatePrimaryComponents()
        {
            if (GetComponent<Rigidbody>() == null)
            {
                gameObject.AddComponent<Rigidbody>();
                var rigidbody = GetComponent<Rigidbody>();
                rigidbody.mass = 50f;
                rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }

            if (GetComponent<CapsuleCollider>() == null)
            {
                var capsuler = gameObject.AddComponent<CapsuleCollider>();
                if (Animator)
                {
                    var foot = Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    var hips = Animator.GetBoneTransform(HumanBodyBones.Hips);
                    var height = (float)System.Math.Round(Vector3.Distance(foot.position, hips.position) * 2f, 2);
                    capsuler.height = height;
                    capsuler.center = new Vector3(0, (float)System.Math.Round(capsuler.height * 0.5f, 2), 0);
                    capsuler.radius = (float)System.Math.Round(capsuler.height * 0.15f, 2);
                }
            }

            if (GetComponent<UnityEngine.AI.NavMeshAgent>() == null)
                gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        }

        public virtual void CreateSecondaryComponents() { }

        #endregion

        #region NavMeshAgent Variables

        private Vector3 _destination;
        private Vector3 _lasDestination;

        [HideInInspector] public UnityEngine.AI.NavMeshAgent NavMeshAgent;
        private UnityEngine.AI.NavMeshHit _navMeshHit;
        private float _updatePathTime;
        private float _canseeTargetUpdateTime;
        private float _timeToResetOutDistance;
        private float _forceUpdatePathTime;
        private bool _isOutOfDistance;
        private int _findAgentDestinationRadius;
        protected float _updateFindTargetTime;

        #endregion

        #region OVERRIDE METHODS. AI

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (!_debugLastTargetPosition) return;
            if (!_currentTarget.Transform || !_hasPositionOfTheTarget) return;

            var color = _targetInLineOfSight ? Color.green : Color.red;
            color.a = 0.2f;
            Gizmos.color = color;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, _lastTargetPosition + Vector3.up * 1.5f);
            color.a = 1;
            Gizmos.color = color;
            Gizmos.DrawLine(_lastTargetPosition, _lastTargetPosition + Vector3.up * 1.5f);
            var forward = (_lastTargetPosition - transform.position).normalized;
            forward.y = 0;
            var right = Quaternion.AngleAxis(90, Vector3.up) * forward;
            var p1 = _lastTargetPosition + Vector3.up * 1.5f - forward;
            var p2 = _lastTargetPosition + Vector3.up * 1.5f + forward * 0.5f + right * 0.25f;
            var p3 = _lastTargetPosition + Vector3.up * 1.5f + forward * 0.5f - right * 0.25f;
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p1, p3);
            Gizmos.DrawLine(p3, p2);
            Gizmos.DrawSphere(_lastTargetPosition + Vector3.up * 1.5f, 0.1f);
        }

        protected override void Init()
        {
            base.Init();
            _receivedDamage = new AIReceivedDamegeInfo();
            _destination = transform.position;
            _lasDestination = _destination;

            NavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (!NavMeshAgent) return;
            NavMeshAgent.updatePosition = false;
            NavMeshAgent.updateRotation = false;
            NavMeshAgent.radius = CapsuleCollider.radius;
            if (IsOnNavMesh) NavMeshAgent.enabled = true;

            RotateTo(transform.forward);

            if (_currentTarget != null) 
                _currentTarget.InitTarget(_currentTarget.Transform);

            _headtrack = GetComponent<AIHeadtrack>();
            _targetsInRange = new Collider[_maxTargetsDetection];
            AiComponents = new Dictionary<System.Type, IAIComponent>();
            var _aiComponents = GetComponents<IAIComponent>();

            for (int i = 0; i < _aiComponents.Length; i++)
            {
                if (!AiComponents.ContainsKey(_aiComponents[i].ComponentType))
                {
                    AiComponents.Add(_aiComponents[i].ComponentType, _aiComponents[i]);
                }
            }

            StartCoroutine(AlignDetectionPoint());
        }

        protected virtual IEnumerator AlignDetectionPoint()
        {
            yield return new WaitForSeconds(.1f);
            if (DetectionPointReference) DetectionPointReference.rotation = transform.rotation;
        }

        protected override void UpdateAI()
        {
            base.UpdateAI();
            CalcMovementDirection();

            HandleTarget();
            if (_receivedDamage != null) _receivedDamage.Update();
        }

        protected override void OnAnimatorMove()
        {
            if (GameTimeManager.Instance.DeltaTime == 0) return;
            if (!CustomAction && _useNavMeshAgent && NavMeshAgent && NavMeshAgent.enabled)
            {
                NavMeshAgent.velocity = ((Animator.deltaPosition) / GameTimeManager.Instance.DeltaTime) *
                                        Mathf.Clamp(RemainingDistanceWithoutAgent - StopingDistance, 0, 1f);
                NavMeshAgent.speed = Mathf.Lerp(NavMeshAgent.speed, MaxSpeed,
                    _aceleration * GameTimeManager.Instance.DeltaTime);
                NavMeshAgent.nextPosition = Animator.rootPosition;
            }

            base.OnAnimatorMove();
        }

        public override void Stop()
        {
            base.Stop();
            if (!_useNavMeshAgent || !NavMeshAgent || !NavMeshAgent.isOnNavMesh || NavMeshAgent.isStopped) return;
            NavMeshAgent.isStopped = true;
            _destination = transform.position;
            ForceUpdatePath();
            NavMeshAgent.ResetPath();
        }

        public override void DisableAIController()
        {
            if (DisableAgentOnStart && NavMeshAgent)
                NavMeshAgent.enabled = false;
            base.DisableAIController();
        }

        #endregion

        #region METHODS. AIAgent/Interfaces

        #region Protected methods

        protected virtual Dictionary<System.Type, IAIComponent> AiComponents { get; set; }

        protected float GetUpdateTimeFromQuality(AIUpdateQuality quality)
        {
            return quality == AIUpdateQuality.VeryLow ? 2 :
                quality == AIUpdateQuality.Low ? 1f :
                quality == AIUpdateQuality.Medium ? 0.75f :
                quality == AIUpdateQuality.High ? .25f : 0.1f;
        }

        protected virtual void UpdateAgentPath()
        {
            _updatePathTime -= GameTimeManager.Instance.DeltaTime;
            if (_updatePathTime > 0 && _forceUpdatePathTime <= 0f && NavMeshAgent.hasPath) return;
            _forceUpdatePathTime -= GameTimeManager.Instance.DeltaTime;
            _updatePathTime = GetUpdateTimeFromQuality(_updatePathQuality);

            if (!IsDead && IsGrounded)
            {
                var destin = _destination;

                if ((MovementSpeed != AIMovementSpeed.Idle && destin != _lasDestination) || !NavMeshAgent.hasPath)
                {
                    if (NavMeshAgent.enabled && NavMeshAgent.isOnNavMesh)
                    {
                        if (UnityEngine.AI.NavMesh.SamplePosition(destin, out _navHit,
                                CapsuleCollider.radius + _findAgentDestinationRadius, NavMeshAgent.areaMask) &&
                            (_navHit.position - NavMeshAgent.destination).magnitude > _stopingDistance)
                        {
                            NavMeshAgent.destination = (_navHit.position);
                            _lasDestination = destin;
                        }
                        else if ((_navHit.position - NavMeshAgent.destination).magnitude > _stopingDistance)
                        {
                            _findAgentDestinationRadius++;
                            if (_findAgentDestinationRadius >= 10)
                            {
                                _findAgentDestinationRadius = 0;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void CalcMovementDirection()
        {
            if (IsDead) return;

            if (_useNavMeshAgent && NavMeshAgent)
            {
                ControlNavMeshAgent();
                UpdateAgentPath();
            }

            bool forceMovement = !NavMeshAgent.hasPath &&
                                 RemainingDistanceWithoutAgent > NavMeshAgent.stoppingDistance + CapsuleCollider.radius;
            var dir = !forceMovement && NavMeshAgent != null && NavMeshAgent.enabled && _useNavMeshAgent
                ? DesiredVelocity * (!IsInDestination ? 1 : 0)
                : ((new Vector3(_destination.x, transform.position.y, _destination.z) - transform.position).normalized *
                   Mathf.Clamp(RemainingDistanceWithoutAgent - _stopingDistance, 0, 1f));
            //Convert Direction to Input
            var movementInput = transform.InverseTransformDirection(dir);

            if (_useNavMeshAgent && NavMeshAgent.enabled)
            {
                var data = NavMeshAgent.currentOffMeshLinkData;
                if (NavMeshAgent.isOnOffMeshLink)
                {
                    dir = (data.endPos - transform.position);
                    movementInput = transform.InverseTransformDirection(dir);
                }
            }

            if (movementInput.magnitude > 0.1f)
            {
                if (_temporaryDirectionTime <= 0 && IsStrafing == false)
                    SetMovementInput(movementInput, _aceleration);
                else
                    SetMovementInput(movementInput,
                        _temporaryDirectionTime <= 0 ? transform.forward : _temporaryDirection, _aceleration);
            }
            else
                Input = Vector3.zero;

            if (!IsGrounded) NavMeshAgent.enabled = false;
        }

        public virtual Vector3 DesiredVelocity => NavMeshAgent.desiredVelocity;

        protected virtual void CheckAgentDistanceFromAI()
        {
            if (!_useNavMeshAgent || !NavMeshAgent || !NavMeshAgent.enabled) return;
            if (Vector3.Distance(transform.position, NavMeshAgent.nextPosition) > _stopingDistance * 1.5f &&
                !_isOutOfDistance)
            {
                _timeToResetOutDistance = 3f;
                _isOutOfDistance = true;
            }

            if (_isOutOfDistance)
            {
                _timeToResetOutDistance -= GameTimeManager.Instance.DeltaTime;
                if (_timeToResetOutDistance <= 0)
                {
                    _isOutOfDistance = false;
                    if (Vector3.Distance(transform.position, NavMeshAgent.nextPosition) > _stopingDistance)
                    {
                        NavMeshAgent.enabled = false;
                    }
                }
            }
        }

        protected virtual void ControlNavMeshAgent()
        {
            if (IsDead) return;
            if (_useNavMeshAgent && NavMeshAgent)
                NavMeshAgent.stoppingDistance = _stopingDistance;
            if (GameTimeManager.Instance.DeltaTime == 0 || NavMeshAgent.enabled == false)
            {
                if (IsGrounded && !NavMeshAgent.enabled && IsOnNavMesh)
                {
                    NavMeshAgent.enabled = true;
                }
            }

            if (!IsGrounded)
                NavMeshAgent.enabled = false;
            CheckAgentDistanceFromAI();
        }

        protected virtual bool CheckCanSeeTarget()
        {
            if (_currentTarget != null && _currentTarget.Transform != null && _currentTarget.Collider == null &&
                InFOVAngle(_currentTarget.Transform.position, _fieldOfView))
            {
                if (SightMethodType == 0) return true;
                var eyesPoint = DetectionPointReference
                    ? DetectionPointReference.position
                    : transform.position + Vector3.up * (SelfCollider.bounds.size.y * 0.8f);
                if (!Physics.Linecast(eyesPoint, _currentTarget.Transform.position, _obstacles))
                {
                    if (_debugRaySight)
                        Debug.DrawLine(eyesPoint, _currentTarget.Transform.position, Color.green,
                            GetUpdateTimeFromQuality(_canseeTargetUpdateQuality));
                    return true;
                }
                else
                {
                    if (_debugRaySight)
                        Debug.DrawLine(eyesPoint, _currentTarget.Transform.position, Color.red,
                            GetUpdateTimeFromQuality(_canseeTargetUpdateQuality));
                }
            }
            else if (_currentTarget.Collider) return CheckCanSeeTarget(_currentTarget.Collider);

            return false;
        }

        protected virtual bool CheckCanSeeTarget(Collider target)
        {
            if (target != null && InFOVAngle(target.bounds.center, _fieldOfView))
            {
                if (SightMethodType == 0) return true;
                var detectionPoint = DetectionPointReference
                    ? DetectionPointReference.position
                    : transform.position + Vector3.up * (SelfCollider.bounds.size.y * 0.8f);
                if (SightMethodType.Contains<AISightMethod>(AISightMethod.Center))
                    if (!Physics.Linecast(detectionPoint, target.bounds.center, _obstacles))
                    {
                        if (_debugRaySight)
                            Debug.DrawLine(detectionPoint, target.bounds.center, Color.green,
                                GetUpdateTimeFromQuality(_canseeTargetUpdateQuality));
                        return true;
                    }
                    else
                    {
                        if (_debugRaySight)
                            Debug.DrawLine(detectionPoint, target.bounds.center, Color.red,
                                GetUpdateTimeFromQuality(_canseeTargetUpdateQuality));
                    }

                if (SightMethodType.Contains<AISightMethod>(AISightMethod.Top))
                    if (!Physics.Linecast(detectionPoint,
                            target.transform.position + Vector3.up * target.bounds.size.y * 0.9f, _obstacles))
                    {
                        if (_debugRaySight)
                            Debug.DrawLine(detectionPoint,
                                target.transform.position + Vector3.up * target.bounds.size.y * 0.9f, Color.green,
                                GetUpdateTimeFromQuality(_canseeTargetUpdateQuality));
                        return true;
                    }
                    else
                    {
                        if (_debugRaySight)
                            Debug.DrawLine(detectionPoint,
                                target.transform.position + Vector3.up * target.bounds.size.y * 0.9f, Color.red,
                                GetUpdateTimeFromQuality(_canseeTargetUpdateQuality));
                    }

                if (SightMethodType.Contains<AISightMethod>(AISightMethod.Bottom))
                    if (!Physics.Linecast(detectionPoint,
                            target.transform.position + Vector3.up * target.bounds.size.y * 0.1f, _obstacles))
                    {
                        if (_debugRaySight)
                            Debug.DrawLine(detectionPoint,
                                target.transform.position + Vector3.up * target.bounds.size.y * 0.1f, Color.green,
                                GetUpdateTimeFromQuality(_canseeTargetUpdateQuality));
                        return true;
                    }
                    else
                    {
                        if (_debugRaySight)
                            Debug.DrawLine(detectionPoint,
                                target.transform.position + Vector3.up * target.bounds.size.y * 0.1f, Color.red,
                                GetUpdateTimeFromQuality(_canseeTargetUpdateQuality));
                    }
            }

            return false;
        }

        protected virtual bool InFOVAngle(Vector3 viewPoint, float fieldOfView)
        {
            if (CapsuleCollider == null) return false;
            var eyesPoint = (DetectionPointReference
                ? DetectionPointReference.position
                : CapsuleCollider.bounds.center);
            if (Vector3.Distance(eyesPoint, viewPoint) < _minDistanceToDetect) return true;
            if (Vector3.Distance(eyesPoint, viewPoint) > _maxDistanceToDetect) return false;

            var lookDirection = viewPoint - eyesPoint;
            var rot = Quaternion.LookRotation(lookDirection, Vector3.up);
            var detectionAngle = DetectionPointReference ? DetectionPointReference.eulerAngles : transform.eulerAngles;
            var newAngle = rot.eulerAngles - detectionAngle;
            var fovAngleY = newAngle.NormalizeAngle().y;
            var fovAngleX = newAngle.NormalizeAngle().x;
            if (fovAngleY <= (fieldOfView * 0.5f) && fovAngleY >= -(fieldOfView * 0.5f) &&
                fovAngleX <= (fieldOfView * 0.5f) && fovAngleX >= -(fieldOfView * 0.5f))
                return true;

            return false;
        }

        protected virtual void HandleTarget()
        {
            if (_hasPositionOfTheTarget && _currentTarget.Transform)
                LastTargetPosition = _currentTarget.Transform.position;
            _canseeTargetUpdateTime -= GameTimeManager.Instance.DeltaTime;
            if (_canseeTargetUpdateTime > 0) return;
            if (_currentTarget != null && _currentTarget.Transform)
            {
                _targetInLineOfSight = CheckCanSeeTarget();
                if (!_targetInLineOfSight || TargetDistance >= (_maxDistanceToDetect + _lostTargetDistance))
                {
                    if (_lostTargetTime < Time.time)
                    {
                        _hasPositionOfTheTarget = false;
                        _lostTargetTime = Time.time + _timeToLostWithoutSight;
                    }
                }
                else
                {
                    _lostTargetTime = Time.time + _timeToLostWithoutSight;
                    _hasPositionOfTheTarget = true;
                    _currentTarget.IsLost = false;
                }
            }
            else
            {
                _targetInLineOfSight = false;
                _hasPositionOfTheTarget = false;
            }

            HandleLostTarget();
            _canseeTargetUpdateTime = GetUpdateTimeFromQuality(_canseeTargetUpdateQuality);
        }

        protected virtual void HandleLostTarget()
        {
            if (!_possibleToLostTarget) return;
            if (_currentTarget != null && _currentTarget.Transform != null)
            {
                if (_currentTarget.HasHealthController && (_currentTarget.IsDead ||
                                                           TargetDistance >
                                                           (_maxDistanceToDetect + _lostTargetDistance) ||
                                                           (!_targetInLineOfSight && !_hasPositionOfTheTarget)))
                {
                    if (_currentTarget.IsFixedTarget)
                        _currentTarget.IsLost = true;
                    else
                        _currentTarget.ClearTarget();
                }
                else if (!_currentTarget.HasHealthController && (_currentTarget.Transform == null ||
                                                                 !_currentTarget.Transform.gameObject.activeSelf ||
                                                                 TargetDistance > (_maxDistanceToDetect +
                                                                     _lostTargetDistance) ||
                                                                 (!_targetInLineOfSight && !_hasPositionOfTheTarget)))
                {
                    if (_currentTarget.IsFixedTarget)
                        _currentTarget.IsLost = true;
                    else
                        _currentTarget.ClearTarget();
                }
            }
        }

        protected virtual bool IsInLayerMask(int layer, LayerMask layermask)
        {
            return layermask == (layermask | (1 << layer));
        }

        #endregion

        #region Public methods

        public float FieldOfView
        {
            get => _fieldOfView;
            set => _fieldOfView = value;
        }

        public float MinDistanceToDetect
        {
            get => _minDistanceToDetect;
            set => _minDistanceToDetect = value;
        }

        public float MaxDistanceToDetect
        {
            get => _maxDistanceToDetect;
            set => _maxDistanceToDetect = value;
        }

        public AIUpdateQuality UpdatePathQuality
        {
            get => _updatePathQuality;
            set => _updatePathQuality = value;
        }

        public AIUpdateQuality FindTargetUpdateQuality
        {
            get => _findTargetUpdateQuality;
            set => _findTargetUpdateQuality = value;
        }

        public AIUpdateQuality CanseeTargetUpdateQuality
        {
            get => _canseeTargetUpdateQuality;
            set => _canseeTargetUpdateQuality = value;
        }

        public virtual void SetDetectionLayer(LayerMask mask)
        {
            _detectLayer = mask;
        }

        public void SetDetectionLayer(string layerName)
        {
            _detectLayer = 1 << LayerMask.NameToLayer(layerName);
        }

        public virtual void SetDetectionTags(List<string> tags)
        {
            _detectTags = tags;
        }

        public void SetDetectionTag(string tag)
        {
            var tags = new List<string>() { tag };
            _detectTags = tags;
        }

        public virtual void SetObstaclesLayer(LayerMask mask)
        {
            _obstacles = mask;
        }

        public virtual void SetLineOfSight(float fov = -1, float minDistToDetect = -1, float maxDistToDetect = -1,
            float lostTargetDistance = -1)
        {
            if (fov != -1) _fieldOfView = fov;
            if (minDistToDetect != -1) _minDistanceToDetect = minDistToDetect;
            if (maxDistToDetect != -1) _maxDistanceToDetect = maxDistToDetect;
            if (lostTargetDistance != -1) _lostTargetDistance = lostTargetDistance;
        }

        public virtual AIReceivedDamegeInfo ReceivedDamage
        {
            get { return _receivedDamage; }
            protected set { _receivedDamage = value; }
        }

        public virtual bool TargetInLineOfSight
        {
            get { return _targetInLineOfSight; }
        }

        public virtual AITarget CurrentTarget
        {
            get { return _currentTarget; }
            protected set { _currentTarget = value; }
        }

        public virtual Vector3 LastTargetPosition
        {
            get { return _lastTargetPosition; }
            protected set { _lastTargetPosition = value; }
        }

        public virtual float TargetDistance
        {
            get
            {
                if (_currentTarget == null || _currentTarget.IsDead) return Mathf.Infinity;
                return Vector3.Distance(_currentTarget.Transform.position, transform.position);
            }
        }

        public virtual Collider[] GetTargetsInRange()
        {
            return _targetsInRange;
        }

        public virtual void FindTarget()
        {
            FindSpecificTarget(_detectTags, _detectLayer, true);
        }

        public virtual void FindTarget(bool checkForObstacles)
        {
            FindSpecificTarget(_detectTags, _detectLayer, checkForObstacles);
        }

        public virtual void FindSpecificTarget(List<string> m_detectTags, LayerMask m_detectLayer,
            bool checkForObstables = true)
        {
            if (_updateFindTargetTime > Time.time) return;
            _updateFindTargetTime = Time.time + GetUpdateTimeFromQuality(_findTargetUpdateQuality);
            if (!_findOtherTarget && _currentTarget.Transform) return;
            if (_currentTarget.Transform && _currentTarget.IsFixedTarget && !_findOtherTarget) return;
            int targetsCount = Physics.OverlapSphereNonAlloc(transform.position + transform.up, _maxDistanceToDetect,
                _targetsInRange, m_detectLayer);
            if (targetsCount > 0)
            {
                Transform target = _currentTarget != null && _hasPositionOfTheTarget ? _currentTarget.Transform : null;
                var targetDistance = target && _targetInLineOfSight ? TargetDistance : Mathf.Infinity;

                for (int i = 0; i < targetsCount; i++)
                {
                    if (_targetsInRange[i] != null && _targetsInRange[i].transform != transform &&
                        m_detectTags.Contains(_targetsInRange[i].gameObject.tag) &&
                        (!checkForObstables || CheckCanSeeTarget(_targetsInRange[i])))
                    {
                        if (_findTargetByDistance)
                        {
                            var newTargetDistance = Vector3.Distance(_targetsInRange[i].transform.position,
                                transform.position);

                            if (newTargetDistance < targetDistance)
                            {
                                target = _targetsInRange[i].transform;
                                targetDistance = newTargetDistance;
                            }
                        }
                        else
                        {
                            target = _targetsInRange[i].transform;
                            break;
                        }
                    }
                }

                if (_currentTarget == null || target != null && target != _currentTarget.Transform)
                {
                    if (target != null)
                        SetCurrentTarget(target);
                }
            }
        }

        public virtual bool TryGetTarget(out AITarget target)
        {
            return TryGetTarget(_detectTags, out target);
        }

        public virtual bool TryGetTarget(string tag, out AITarget target)
        {
            Collider[] ts = System.Array.FindAll(_targetsInRange, c => c != null && c.gameObject.CompareTag(tag));
            if (ts != null && ts.Length > 1)
            {
                System.Array.Sort(ts, delegate (Collider a, Collider b)
                {
                    return Vector2.Distance(this.transform.position, a.transform.position)
                        .CompareTo(
                            Vector2.Distance(this.transform.position, b.transform.position));
                });
            }

            if (ts != null && ts.Length > 0)
            {
                target = new AITarget();
                target.InitTarget(ts[0].transform);
                return true;
            }

            target = null;
            return false;
        }

        public virtual bool TryGetTarget(List<string> detectTags, out AITarget target)
        {
            Collider[] ts =
                System.Array.FindAll(_targetsInRange, c => c != null && detectTags.Contains(c.gameObject.tag));
            if (ts != null && ts.Length > 1)
            {
                System.Array.Sort(ts, delegate (Collider a, Collider b)
                {
                    return Vector2.Distance(this.transform.position, a.transform.position)
                        .CompareTo(
                            Vector2.Distance(this.transform.position, b.transform.position));
                });
            }

            if (ts != null && ts.Length > 0)
            {
                target = new AITarget();
                target.InitTarget(ts[0].transform);
                return true;
            }

            target = null;
            return false;
        }

        public virtual void SetCurrentTarget(Transform target)
        {
            SetCurrentTarget(target, true);
        }

        public virtual void SetCurrentTarget(Transform target, bool overrideCanseTarget)
        {
            if (_changeTargetTime < Time.time)
            {
                _changeTargetTime = _changeTargetDelay + Time.time;
                _currentTarget.InitTarget(target);
                if (overrideCanseTarget)
                {
                    _currentTarget.IsLost = false;
                    _targetInLineOfSight = true;
                    _hasPositionOfTheTarget = false;
                }

                _updateFindTargetTime = 0f;
                _updatePathTime = 0f;
                _lastTargetPosition = target.position;
                LookToTarget(target, 2);
            }
        }

        public virtual void RemoveCurrentTarget()
        {
            _currentTarget.ClearTarget();
        }

        public virtual void LookAround()
        {
            if (_headtrack) _headtrack.LookAround();
        }

        public virtual void LookTo(Vector3 point, float stayLookTime = 1, float offsetLookHeight = -1)
        {
            if (_headtrack) _headtrack.LookAtPoint(point, stayLookTime, offsetLookHeight);
        }

        public virtual void LookToTarget(Transform target, float stayLookTime = 1, float offsetLookHeight = -1)
        {
            if (_headtrack) _headtrack.LookAtTarget(target, stayLookTime, offsetLookHeight);
        }

        public virtual void SetSpeed(AIMovementSpeed movementSpeed)
        {
            if (this.MovementSpeed != movementSpeed)
            {
                if (movementSpeed == AIMovementSpeed.Idle)
                {
                    Stop();
                }

                base.MovementSpeed = movementSpeed;
            }
        }

        public virtual bool IsInDestination
        {
            get
            {
                if (_useNavMeshAgent && (RemainingDistance <= _stopingDistance || NavMeshAgent.hasPath &&
                        RemainingDistance > _stopingDistance && DesiredVelocity.magnitude < 0.1f)) return true;
                return RemainingDistance <= _stopingDistance;
            }
        }

        public virtual bool IsMoving
        {
            get { return Input.sqrMagnitude > 0.1f; }
        }

        public virtual float RemainingDistance
        {
            get
            {
                return NavMeshAgent && NavMeshAgent.enabled && _useNavMeshAgent && IsOnNavMesh
                    ? NavMeshAgent.remainingDistance
                    : RemainingDistanceWithoutAgent;
            }
        }

        protected virtual float RemainingDistanceWithoutAgent
        {
            get
            {
                return Vector3.Distance(transform.position,
                    new Vector3(_destination.x, transform.position.y, _destination.z));
            }
        }

        public virtual Collider SelfCollider => CapsuleCollider;

        public virtual bool IsOnNavMesh
        {
            get
            {
                if (!_useNavMeshAgent) return false;
                if (NavMeshAgent.enabled) return NavMeshAgent.isOnNavMesh;

                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out _navMeshHit, CapsuleCollider.radius,
                        NavMeshAgent.areaMask))
                {
                    return true;
                }

                return false;
            }
        }

        public virtual void MoveTo(Vector3 newDestination, AIMovementSpeed speed = AIMovementSpeed.Walking)
        {
            if (IsStrafing) _updatePathTime = 0;
            SetFreeLocomotion();
            SetSpeed(speed);
            var dir = newDestination - transform.position;
            dir.y = 0;
            _destination = newDestination;
            _temporaryDirection = transform.forward;
            _temporaryDirectionTime = 0;
        }

        public virtual void StrafeMoveTo(Vector3 newDestination, Vector3 targetDirection,
            AIMovementSpeed speed = AIMovementSpeed.Walking)
        {
            if (_useNavMeshAgent && NavMeshAgent && NavMeshAgent.isOnNavMesh && NavMeshAgent.isStopped)
                NavMeshAgent.isStopped = false;
            SetStrafeLocomotion();
            SetSpeed(speed);
            _destination = newDestination;
            _temporaryDirection = targetDirection;
            _temporaryDirectionTime = 2f;
        }

        /// <summary>
        /// 패트롤, FSM용 이동 함수
        /// </summary>
        /// <param name="newDestination"></param>
        /// <param name="speed"></param>
        public virtual void StrafeMoveTo(Vector3 newDestination, AIMovementSpeed speed = AIMovementSpeed.Walking)
        {
            if (_useNavMeshAgent && NavMeshAgent && NavMeshAgent.isOnNavMesh && NavMeshAgent.isStopped)
                NavMeshAgent.isStopped = false;
            SetStrafeLocomotion();
            SetSpeed(speed);
            _destination = newDestination;
        }

        public virtual void RotateTo(Vector3 targetDirection)
        {
            targetDirection.y = 0;
            if (Vector3.Angle(transform.forward, targetDirection) > 20)
            {
                _temporaryDirection = targetDirection;
                _temporaryDirectionTime = 2f;
            }
        }

        public virtual Vector3 TargetDestination => _destination;

        public virtual float StopingDistance
        {
            get => StopingDistanceRelativeToSpeed + _stopingDistance;
            set => _stopingDistance = value;
        }

        protected virtual float StopingDistanceRelativeToSpeed =>
            MovementSpeed == AIMovementSpeed.Idle ? 1 :
            MovementSpeed == AIMovementSpeed.Running ? _runningStopingDistance :
            MovementSpeed == AIMovementSpeed.Sprinting ? _sprintingStopingDistance : _walkingStopingDistance;


        public override void TakeDamage(Damage damage)
        {
            TargetSettingWhenHit(damage);
            base.TakeDamage(damage);
        }

        private void TargetSettingWhenHit(Damage damage)
        {
            LastHitPosition = damage.HitPosition;
            //Check condition to add a new target
            if (!_currentTarget.Transform || (_currentTarget.Transform && !_currentTarget.IsFixedTarget ||
                                              (_currentTarget.IsFixedTarget && _findOtherTarget)))
            {
                //Check if new target is in detections settings
                if (damage.Sender && IsInLayerMask(damage.Sender.gameObject.layer, _detectLayer) &&
                    _detectTags.Contains(damage.Sender.gameObject.tag))
                {
                    SetCurrentTarget(damage.Sender, false);
                }
            }

            _receivedDamage.UpdateDamage(damage);
            _updatePathTime = 0f;
        }


        public virtual void ForceUpdatePath(float timeInUpdate = 1f)
        {
            _forceUpdatePathTime = timeInUpdate;
        }

        public virtual bool HasComponent<T>() where T : IAIComponent
        {
            return AiComponents.ContainsKey(typeof(T));
        }

        public virtual T GetAIComponent<T>() where T : IAIComponent
        {
            return AiComponents.ContainsKey(typeof(T)) ? (T)AiComponents[typeof(T)] : default(T);
        }

        private List<Collider> _triggers = new List<Collider>();

        float _checkTriggerFrequency;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!_triggers.Contains(other)) _triggers.Add(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (_triggers.Contains(other)) _triggers.Remove(other);
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (_checkTriggerFrequency < Time.time)
            {
                _triggers = _triggers.FindAll(c => c != null);
                _checkTriggerFrequency = Time.time + 2f;
            }
        }

        public bool IsInTriggerWithTag(string tag)
        {
            return _triggers.Exists(c => c != null && c.gameObject.CompareTag(tag));
        }

        public bool IsInTriggerWithName(string name)
        {
            return _triggers.Exists(c => c != null && c.gameObject.name.Equals(name));
        }

        public bool IsInTriggerWithTag(string tag, out Collider result)
        {
            var _c = _triggers.Find(c => c != null && c.gameObject.CompareTag(tag));
            result = _c;
            return _c != null;
        }

        public bool IsInTriggerWithName(string name, out Collider result)
        {
            var _c = _triggers.Find(c => c != null && c.gameObject.name.Equals(name));
            result = _c;
            return _c != null;
        }

        #endregion

        #endregion
    }
}