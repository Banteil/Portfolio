using System;
using UnityEngine;
using UnityEngine.AI;

namespace starinc.io.gallaryx
{
    public class AIBrain : BaseBrain
    {
        #region Cache
        private const string NAVMESHAGENTOBJECT = "NavMeshAgentObject";
        private CameraController _cameraController;
        private NavMeshAgent _navAgent;
        private Transform _targetTransform;

        public Transform TargetTransform 
        { 
            get { return _targetTransform; } 
            set {  _targetTransform = value; } 
        }

        public override Vector3 GoalLocation
        {
            get { return _navAgent.destination; }
            set { _navAgent.destination = value; }
        }

        public override Vector3 GetMoveDirection
        {
            get
            {
                var dir = _navAgent.desiredVelocity.normalized;
                dir.y = 0f;
                return dir;
            }
        }

        public override Vector3 TargetRotationValue
        {
            get
            {
                if (_targetTransform != null)
                {
                    var dir = (_targetTransform.position - _character.transform.position).normalized;
                    dir.y = 0f;
                    return dir;
                }
                else
                {
                    if (_character.IsControlCharacter && _cameraController.Perspective == CameraPerspective.FirstPerson)
                    {
                        var cameraDir = Util.GetDirectionWithoutYAxis(_character.transform.position, Camera.main.transform.position).normalized;
                        return cameraDir;
                    }
                    return _character.transform.forward;
                }
            }
            set
            {
                _character.transform.forward = value;
            }
        }

        private bool _prevArrivalAtDestination = false;
        public override bool ArrivalAtDestination
        {
            get
            {
                var dis = (_navAgent.destination - _character.transform.position).magnitude;
                var result = dis <= _navAgent.stoppingDistance;
                if (!_prevArrivalAtDestination && result)
                    StoppingDistanceCallback?.Invoke();
                _prevArrivalAtDestination = result;
                return result;
            }
        }

        public bool ActiveAI
        {
            get { return !_navAgent.isStopped; }
            set
            {
                _navAgent.isStopped = !value;
                ActiveAICallback?.Invoke();
            }
        }

        public event Action ActiveAICallback;

        #endregion

        public override void OnEnable(CharacterController characterController)
        {
            base.OnEnable(characterController);
            NavMeshAgentSetting();
            _cameraController = GameObject.FindFirstObjectByType<CameraController>();
        }

        /// <summary>
        /// 활성화 시 Agent 생성 및 초기화
        /// </summary>
        private void NavMeshAgentSetting()
        {
            var checkPresenceObject = _character.transform.Find(NAVMESHAGENTOBJECT);
            var navMeshAgentObj = checkPresenceObject == null ? new GameObject(NAVMESHAGENTOBJECT) : checkPresenceObject.gameObject;
            navMeshAgentObj.transform.SetParent(_character.transform, false);
            navMeshAgentObj.layer = LayerMask.NameToLayer("Ignore Raycast");

            _navAgent = Util.GetOrAddComponent<NavMeshAgent>(navMeshAgentObj);
            _navAgent.speed = 0.1f;
            _navAgent.angularSpeed = 0f;
            _navAgent.acceleration = 0f;
            _navAgent.stoppingDistance = _character.BodyCollider.radius + 0.01f;
            _navAgent.radius = _character.BodyCollider.radius;
            _navAgent.height = _character.BodyCollider.height;
        }

        protected override void SetInputEvent()
        {
            var aiInputEvent = new InputEvent();
            aiInputEvent.InputHorizontal = () => !ArrivalAtDestination ? 1f : 0f;
            aiInputEvent.InputVertical = () => !ArrivalAtDestination ? 1f : 0f;
            aiInputEvent.InputRunKey = () => IsRunning ? InputState.Stay : InputState.None;
            aiInputEvent.InputJumpKey = () => IsJumping ? InputState.Up : InputState.None;
            BrainInputEvent = aiInputEvent;
        }

        /// <summary>
        /// Agent가 캐릭터와 동일한 포지션에 있도록 포지션 초기화
        /// </summary>
        public override void OnUpdate()
        {
            _navAgent.transform.localPosition = Vector3.zero;
        }

        /// <summary>
        /// 비활성화 시 Agent 제거
        /// </summary>
        public override void OnDisable()
        {
            RemoveNaveMeshAgent();
        }
        
        /// <summary>
        /// Agent를 제거하는 함수
        /// </summary>
        private void RemoveNaveMeshAgent()
        {
            var checkPresenceObject = _character.transform.Find(NAVMESHAGENTOBJECT);
            if(checkPresenceObject != null)
                UnityEngine.Object.Destroy(checkPresenceObject.gameObject);
        }
    }
}
