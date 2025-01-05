using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.gallaryx
{
    public enum CameraMovementType
    {
        Manual,
        Auto
    }

    public enum CameraPerspective
    {
        FirstPerson,
        ThirdPerson,
    }

    public class CameraController : BaseController
    {
        private CameraMovementType _movementType = CameraMovementType.Manual;
        public CameraMovementType MovementType
        {
            get { return _movementType; }
            set { _movementType = value; }
        }

        private CameraPerspective _perspective = CameraPerspective.ThirdPerson;
        public CameraPerspective Perspective
        {
            get { return _perspective; }
            set
            {
                _perspective = value;
                switch (_perspective)
                {
                    case CameraPerspective.ThirdPerson:
                        _mainCamera.cullingMask |= 1 << LayerMask.NameToLayer("Player");
                        break;
                    case CameraPerspective.FirstPerson:
                        _mainCamera.cullingMask = _mainCamera.cullingMask & ~(1 << LayerMask.NameToLayer("Player"));
                        break;
                    default:
                        Debug.LogError("카메라 시점 처리 오류");
                        break;
                }
            }
        }
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private float _minDistance = 0.7f;
        [SerializeField] private float _maxDistance = 8f;
        [SerializeField] private float _viewChangeCriteria = 1f;
        [SerializeField] private float _scrollSpeed = 50f;
        [SerializeField] private float _zoomSpeed = 100f;
        [SerializeField] private float _autoMovementSpeed = 2f;
        [SerializeField] private float _smoothSpeed = 10f;
        [SerializeField] private float _cameraRadius = 0.3f;

        private CinemachineCamera _cinemachineCamera;
        private CinemachinePositionComposer _positionComposer;
        private CinemachinePanTilt _panTilt;

        private InputState _mouseLeftClickState;
        private bool _isDrag, _isUIClick;
        private float _scrollAxis;
        private Vector3 _startMousePos;
        private LayerMask _obstaclesLayer;

        public bool IsWheel { get { return _scrollAxis != 0; } }

        public event Action<RaycastHit> ClickCallback;

        protected override void OnAwake()
        {
            _cinemachineCamera = GetComponentInChildren<CinemachineCamera>();
            _positionComposer = _cinemachineCamera.GetComponent<CinemachinePositionComposer>();
            _panTilt = _cinemachineCamera.GetComponent<CinemachinePanTilt>();
            _obstaclesLayer = (1 << LayerMask.NameToLayer("Obstacles")) | (1 << LayerMask.NameToLayer("Ground"));
            GameManager.Instance.RequireObjectsSpawnCallback += Call_PlayerSpawn;
        }

        protected override void InputStateFunction()
        {
            _mouseLeftClickState = InputManager.Instance.KeyInputEvent.InputMouseLeftKey?.Invoke() ?? InputState.None;
            _scrollAxis = Util.IsMobileWebPlatform ? InputManager.Instance.TouchDeltaMag : InputManager.Instance.KeyInputEvent.InputMouseWheel?.Invoke() ?? 0f;
        }

        private void LateUpdate()
        {
            MouseControl();
            InputLateActionFunction();
            CollisionCheck();
        }

        protected override void InputActionFunction()
        {
            CheckDragInput();
            Click();
        }

        private void InputLateActionFunction()
        {
            if (IsWheel && !Util.IsUIFocusing && !Util.IsPointerOverUI())
                Zoom();

            if (_movementType == CameraMovementType.Auto)
            {
                _panTilt.TiltAxis.Value = 0f;
                var rot = Quaternion.Slerp(_cinemachineCamera.transform.rotation, Quaternion.LookRotation(_cinemachineCamera.Follow.forward), _autoMovementSpeed * Time.fixedDeltaTime);
                _panTilt.PanAxis.Value = rot.eulerAngles.y;
            }
        }

        /// <summary>
        /// 드래그 조작을 했는지 여부를 체크
        /// </summary>
        private void CheckDragInput()
        {
            switch (_mouseLeftClickState)
            {
                case InputState.Down:
                    _startMousePos = Input.mousePosition;
                    break;
                case InputState.Up:
                    var distance = _startMousePos - Input.mousePosition;
                    _isDrag = distance.magnitude > (Util.IsMobileWebPlatform ? float.Epsilon + 4f : float.Epsilon);
                    UIManager.Instance.IsUIClick = false;
                    break;
            }
        }

        /// <summary>
        /// 마우스 왼쪽 클릭 액션 함수
        /// </summary>
        private void Click()
        {
            if (_isDrag)
            {
                _isDrag = false;
                return;
            }

            if(_mouseLeftClickState == InputState.Down)
            {
                _isUIClick = Util.IsPointerOverUI();
            }
            if (_mouseLeftClickState == InputState.Up)
            {
                if(_isUIClick)
                {
                    _isUIClick = false;
                    return;
                }
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.PositiveInfinity))
                {
                    ClickCallback?.Invoke(hit);
                }
            }
        }

        private void CollisionCheck()
        {
            if (_cinemachineCamera.LookAt == null) return;
            Vector3 direction = (_mainCamera.transform.position - _cinemachineCamera.LookAt.position).normalized; // 타겟에서 카메라 방향으로 벡터를 만듦
            RaycastHit hit;

            // 타겟에서 카메라 방향으로 레이를 쏨
            if (Physics.Raycast(_cinemachineCamera.LookAt.position, direction, out hit, _positionComposer.CameraDistance, _obstaclesLayer))
            {
                float targetDistance = Mathf.Clamp(hit.distance - _cameraRadius, 0, _positionComposer.CameraDistance);
                _positionComposer.CameraDistance = Mathf.Lerp(_positionComposer.CameraDistance, targetDistance, Time.deltaTime * _smoothSpeed);

                if (_positionComposer.CameraDistance <= _viewChangeCriteria)
                    Perspective = CameraPerspective.FirstPerson;
                else
                    Perspective = CameraPerspective.ThirdPerson;
            }
        }

        /// <summary>
        /// 카메라 줌 인, 아웃 기능 함수
        /// </summary>
        private void Zoom()
        {
            if (_cinemachineCamera.LookAt == null || UIManager.Instance.InteractUI) return;
            var zoomDistance = _positionComposer.CameraDistance - (_scrollAxis * Time.deltaTime * (Util.IsMobileWebPlatform ? _zoomSpeed * 0.5f : _zoomSpeed));
            Vector3 direction = (_mainCamera.transform.position - _cinemachineCamera.LookAt.position).normalized;
            RaycastHit hit;
            var isCollision = Physics.Raycast(_mainCamera.transform.position, direction, out hit, _cameraRadius + 0.1f, _obstaclesLayer);
            _positionComposer.CameraDistance = Mathf.Clamp(zoomDistance, _minDistance, isCollision ? _positionComposer.CameraDistance : _maxDistance);

            if (_positionComposer.CameraDistance <= _viewChangeCriteria)
                Perspective = CameraPerspective.FirstPerson;
            else
                Perspective = CameraPerspective.ThirdPerson;
        }

        #region Callback
        /// <summary>
        /// 마우스 조작, 드래그 시 화면 회전하는 기능
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public void MouseControl()
        {
            if (EventSystem.current == null || Util.IsPointerOverUI() || UIManager.Instance.IsUIClick || UIManager.Instance.InteractUI) return;
            if (Util.IsMobileWebPlatform && Input.touchCount >= 2) return;

            float targetPanValue = Input.GetAxis("Mouse X") * (Util.IsMobileWebPlatform ? _scrollSpeed * 0.3f : _scrollSpeed) * Time.deltaTime;
            targetPanValue = Util.IsMobileWebPlatform ? -targetPanValue : targetPanValue;
            if (_mouseLeftClickState == InputState.Stay && _movementType == CameraMovementType.Manual)
            {
                // Lerp를 이용해 현재 값과 목표 값 사이를 보간하여 부드럽게 이동
                _panTilt.PanAxis.Value = Mathf.Lerp(
                    _panTilt.PanAxis.Value,
                    _panTilt.PanAxis.Value + targetPanValue,
                    _smoothSpeed
                );
            }
        }

        /// <summary>
        /// 플레이어 캐릭터가 스폰되면 카메라의 타겟을 세팅해주는 콜백
        /// </summary>
        private void Call_PlayerSpawn()
        {
            _cinemachineCamera.Follow = MainSceneManager.Instance.Player.transform;
            _cinemachineCamera.LookAt = MainSceneManager.Instance.Player.AnimatorComponent.GetBoneTransform(HumanBodyBones.Neck);
            _positionComposer.CameraDistance = Util.IsMobileWebPlatform ? _minDistance : _maxDistance;
            if (_positionComposer.CameraDistance <= _viewChangeCriteria)
                Perspective = CameraPerspective.FirstPerson;
            else
                Perspective = CameraPerspective.ThirdPerson;

            float characterYaw = _cinemachineCamera.Follow.eulerAngles.y;
            float cameraYaw = _panTilt.PanAxis.Value;
            float angleDifference = Mathf.DeltaAngle(cameraYaw, characterYaw);
            _panTilt.PanAxis.Value += angleDifference;
        }
        #endregion
    }
}
