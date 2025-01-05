using Cysharp.Threading.Tasks;
using UnityEngine;

namespace starinc.io.gallaryx
{
    public enum MoveState
    {
        Idle,
        Walking,
        Running,
    }

    public class MoveAbility : BaseAbility
    {
        #region Cache
        private readonly int _hashMoveMagnitude = Animator.StringToHash("MoveMagnitude");
        private LayerMask _groundLayerMask;

        private float _h, _v;
        private Quaternion _rot;
        private InputState _inputRunning;
        private JumpAbility _jumpAbility;
        private AutomaticViewingAbility _autoAbility;
        private GameObject _arrivalSign;
        private GameObject _arrivalSignPrefab;
        #endregion

        [SerializeField] private float _smoothRotateValue = 30f;
        [SerializeField] private float _smoothMoveValue = 0.1f;
        [SerializeField] private float _climbSpeed = 1f;
        [SerializeField] private float _climbCheckDistance = 0.5f;
        [SerializeField] private float _groundCheckDistance = 0.5f;

        public MoveState CurrentMoveState;
        public float MaxSpeed
        {
            get
            {
                switch (CurrentMoveState)
                {
                    case MoveState.Idle:
                        return 0f;
                    case MoveState.Walking:
                        return 0.5f;
                    case MoveState.Running:
                        return 1f;
                    default:
                        return 0f;
                }
            }
        }

        public bool JumpPossible { get { return _jumpAbility != null; } }
        public bool IsMoving { get { return (Mathf.Abs(_h) + Mathf.Abs(_v)) != 0; } }
        public bool IsRunning { get { return _inputRunning == InputState.Stay; } }

        protected override void OnAwake()
        {
            base.OnAwake();
            _groundLayerMask = LayerMask.NameToLayer("Ground");
            _arrivalSignPrefab = Resources.Load<GameObject>($"Prefabs/UI/ArrivalSign");
        }

        protected override void Initialize()
        {
            base.Initialize();
            _jumpAbility = _characterController.GetAbility<JumpAbility>();
            _autoAbility = _characterController.GetAbility<AutomaticViewingAbility>();

            if (_characterController.IsControlCharacter)
            {
                var cameraController = FindFirstObjectByType<CameraController>();
                cameraController.ClickCallback += Call_ClickMove;
                InputManager.Instance.ControlKeyInputCallback += Call_InputControlKey;
            }
            _characterController.ChangeCharacterTypeCallback += Call_ChangeType;
        }

        protected override void AbilityInputState()
        {
            _h = AbilityInputEvent.InputHorizontal?.Invoke() ?? 0f;
            _v = AbilityInputEvent.InputVertical?.Invoke() ?? 0f;
            _inputRunning = AbilityInputEvent.InputRunKey?.Invoke() ?? InputState.None;
            _rot = RotationCalculation();
            CheckMoveState();
        }

        private void CheckMoveState()
        {
            if (IsMoving)
            {
                if (IsRunning)
                    CurrentMoveState = MoveState.Running;
                else
                    CurrentMoveState = MoveState.Walking;
            }
            else
                CurrentMoveState = MoveState.Idle;
        }

        protected override void AbilityInputAction()
        {            
            Rotate();
            Move();
        }
        private void Rotate()
        {
            CharacterTransform.rotation = !Util.IsUIFocusing ? _rot : CharacterTransform.rotation;
        }

        private Quaternion RotationCalculation()
        {
            var vector = IsMoving ? _characterController.Brain.GetMoveDirection : _characterController.Brain.TargetRotationValue;
            var resultRot = vector != Vector3.zero ? Quaternion.LookRotation(vector) : CharacterTransform.rotation;
            return Quaternion.Slerp(CharacterTransform.rotation, resultRot, _smoothRotateValue * Time.fixedDeltaTime);
        }

        protected void Move()
        {
            if (Util.IsUIFocusing)
            {
                _characterController.AnimatorComponent.SetFloat(_hashMoveMagnitude, 0, _smoothMoveValue, Time.fixedDeltaTime);
                return;
            }

            if (!JumpPossible)
            {
                if (IsMoving)
                {
                    RaycastHit hit;
                    Vector3 rayOrigin = _characterController.transform.position + Vector3.up * 0.1f; // 발 위치에서 약간 위로 Ray 쏘기
                    if (Physics.Raycast(rayOrigin, _characterController.transform.forward, out hit, _climbCheckDistance, 1 << _groundLayerMask))
                    {
                        Debug.DrawRay(_characterController.transform.position + Vector3.up * 0.1f, _characterController.transform.forward * _climbCheckDistance, Color.blue);
                        Vector3 newPosition = _characterController.transform.position;
                        newPosition.y += _climbSpeed * Time.deltaTime;
                        _characterController.transform.position = newPosition;
                    }
                    else
                        Debug.DrawRay(_characterController.transform.position + Vector3.up * 0.1f, _characterController.transform.forward * _climbCheckDistance, Color.red);
                }
                _characterController.AnimatorComponent.SetFloat(_hashMoveMagnitude, MaxSpeed, _smoothMoveValue, Time.fixedDeltaTime);
            }
            else
            {
                if (_jumpAbility.IsGround)
                {
                    if (IsMoving)
                    {
                        RaycastHit hit;
                        Vector3 rayOrigin = _characterController.transform.position + Vector3.up * 0.1f; // 발 위치에서 약간 위로 Ray 쏘기
                        if (Physics.Raycast(rayOrigin, _characterController.transform.forward, out hit, _groundCheckDistance))
                        {
                            if (hit.collider.gameObject.layer == _groundLayerMask)
                            {
                                Debug.DrawRay(_characterController.transform.position + Vector3.up * 0.1f, _characterController.transform.forward * _climbCheckDistance, Color.blue);
                                Vector3 newPosition = _characterController.transform.position;
                                newPosition.y += _climbSpeed * Time.deltaTime;
                                _characterController.transform.position = newPosition;
                            }
                            else
                                Debug.DrawRay(_characterController.transform.position + Vector3.up * 0.1f, _characterController.transform.forward * _climbCheckDistance, Color.red);
                        }                        
                    }
                    _characterController.AnimatorComponent.SetFloat(_hashMoveMagnitude, MaxSpeed, _smoothMoveValue, Time.fixedDeltaTime);
                }
                else
                {
                    if (!_jumpAbility.IsJumping)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(_characterController.transform.position, Vector3.down, out hit, _groundCheckDistance, 1 << _groundLayerMask))
                        {
                            Vector3 newPosition = _characterController.transform.position;
                            newPosition.y = hit.point.y;
                            _characterController.transform.position = newPosition;
                        }
                    }
                    var wallRayCheck = Physics.RaycastAll(CharacterTransform.position + new Vector3(0f, _characterController.BodyCollider.height * 0.5f, 0f), CharacterTransform.forward, 1f, _jumpAbility.CheckJumpLayerMask);
                    if (IsMoving && wallRayCheck.Length < 1)
                        _characterController.RigidBodyComponent.MovePosition(CharacterTransform.position + (CharacterTransform.forward * (_jumpAbility.JumpMoveSpeed * MaxSpeed) * Time.fixedDeltaTime));
                }
            }
        }

        public override void RemoveAbility()
        {
            base.RemoveAbility();
            var cameraController = FindFirstObjectByType<CameraController>();
            if (cameraController != null)
                cameraController.ClickCallback -= Call_ClickMove;
            if (InputManager.HasInstance)
                InputManager.Instance.ControlKeyInputCallback -= Call_InputControlKey;
            _characterController.InputActionCallback -= Call_ArriveDestination;

        }

        #region Callback
        private async void Call_ClickMove(RaycastHit hit)
        {
            if (hit.collider.gameObject.layer == _groundLayerMask)
            {
                if (Util.IsMobileWebPlatform)
                {
                    InputManager.Instance.MobileVirtualController.InactiveRunButton();
                    await UniTask.Yield();
                }

                if(_autoAbility != null && _autoAbility.IsProcessing)
                {
                    _autoAbility.EndAutomaticViewing();
                    await UniTask.Yield();
                }

                _characterController.Type = CharacterType.AI;
                _characterController.Brain.GoalLocation = hit.point;
                _characterController.InputActionCallback -= Call_ArriveDestination;
                _characterController.InputActionCallback += Call_ArriveDestination;

                if(_arrivalSign == null)
                    _arrivalSign = Instantiate(_arrivalSignPrefab);
                _arrivalSign.transform.position = hit.point + new Vector3(0, 0.01f);
            }
        }

        private void Call_ArriveDestination()
        {
            if (_characterController.Brain.ArrivalAtDestination)
            {
                _characterController.Type = CharacterType.Player;
                _characterController.InputActionCallback -= Call_ArriveDestination;
            }
        }

        private void Call_InputControlKey()
        {
            if (IsAI) _characterController.Type = CharacterType.Player;
        }

        private void Call_ChangeType(CharacterType type)
        {
            if(type == CharacterType.Player)
            {
                if (_arrivalSign != null)
                {
                    Destroy(_arrivalSign);
                    _arrivalSign = null;
                }
            }
        }

        protected override void Call_CharacterInteractionState(bool isInteraction)
        {            
            if (!_characterController.IsControlCharacter) return;
            if (isInteraction && IsAI)
            {
                var aiBrain = _characterController.Brain as AIBrain;
                aiBrain.ActiveAI = false;
            }
        }
        #endregion
    }
}
