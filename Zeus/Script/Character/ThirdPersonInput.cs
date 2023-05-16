using Cinemachine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class ThirdPersonInput : zMonoBehaviour, IAnimatorMoveReceiver
    {
        public delegate void OnUpdateEvent();
        public event OnUpdateEvent onAnimatorMove;
        [SerializeField]
        protected ThirdPersonController _cc;

        public ThirdPersonController CC
        {
            get
            {
                if(_cc == null)
                {
                   _cc = GetComponent<ThirdPersonController>();
                }

                if (_cc == null)
                {
                    Debug.LogError("ThirdpersonController 스크립트가 없습니다");
                    return null;
                }
                return _cc;                
            }

            set
            {
                _cc = value;
            }
        }

        [Header("Camera")]
        public float TopClamp = -30;
        public float BottomClamp = 30;
        private Vector2 _prevMouseDelta;
        private Transform _cineCameraFollow;
        public float TargetYRotation;
        public float TargetXRotation;

        [Header("Component")]
        protected LockOn _lockOn;

        public Camera CameraMain
        {
            get
            {
                if (!_cameraMain)
                {
                    if (!Camera.main)
                    {
                        Debug.Log("카메라가 없습니다");
                    }
                    else
                    {
                        _cameraMain = Camera.main;
                        CC._rotateTarget = _cameraMain.transform;
                    }
                }
                return _cameraMain;
            }

            set
            {
                _cameraMain = value;
            }
        }
        protected Camera _cameraMain;

        [Header("Event")]
        public UnityEvent OnEnableAnimatorMove = new UnityEvent();

        #region InputCondition

        private bool _moveCondition = true;

        public bool MoveCondition
        {
            get
            {
                if(CC.CantInput || !_moveCondition)
                {
                    return false;
                }
                return true;
            }

            set
            {
                _moveCondition = value;
            }
        }

        private bool _cameraMoveCondition = true;
        public bool CameraMoveCondition
        {
            get
            {
                if(!_cameraMoveCondition)
                {
                    return false;
                }
                return true;
            }

            set
            {
                _cameraMoveCondition = value;
            }
        }

        private bool _evasionCondition = true;
        public bool EvasionCondition
        {
            get
            {
                if ((CC.CantInput && !CC.CharacterState.HasFlag(TypeCharacterState.PARRY)) || _evasionCondition == false || CC._customAction || CC._cantInputExceptionMove || _holdLeftShoulder || CC._input == Vector3.zero)
                {
                    return false;
                }
                return true;
            }

            set
            {
                _evasionCondition = value;
            }
        }

        private bool _runCondition = true;

        public bool RunCondition
        {
            get
            {
                if(CC.CantInput || !_runCondition || CC._customAction || _holdLeftShoulder)
                {
                    return false;
                }
                return true;
            }
            set
            {
                _runCondition = value;
            }
        }

        private bool _quickTabCondition = true;
        public bool QuickTabCondition
        {
            get
            {
                if(!_quickTabCondition || _holdLeftShoulder)
                {
                    return false;
                }
                return true;
            }

            set
            {
                _quickTabCondition = value;
            }
        }

        #endregion

        #region HoldButton

        public Dictionary<PropertyInfo, InputBufferControl> InputBufferDic = new Dictionary<PropertyInfo, InputBufferControl>();

        protected bool _holdLeftShoulder;
        protected bool _holdRightShoulder;

        protected bool _holdLeftTrigger;
        protected bool _holdRightTrigger;
        #endregion
        protected virtual void Start()
        {
            InPutInit();
            CC = GetComponent<ThirdPersonController>();
            EnableOnAnimatorMove();
            ComponentInit();
            Cursor.lockState = CursorLockMode.Locked;

            CC.OnDeadEvent.AddListener((GameObject o) => PlayerDead());
            CC.onResetHealth.AddListener(() => { enabled = true; });
        }

        protected void ComponentInit()
        {
            _lockOn = GetComponent<LockOn>();
        }

        protected virtual void FixedUpdate()
        {
            if (CC == null) { return; }
            //Transform 구성요소가 변경되면 하위에 있는 rigidbody collider가 Transform변경사항에 따라 위치를 변경함
            Physics.SyncTransforms();
            //캐릭터의 체력 및 스태미나 관리 및 슬라이드 상태 공중에 있을때 등의 캐릭터의 상태에 관한 연산 작업을 해줌
            CC.UpdateMotor();
            //캐릭터의 LocomotionType에 따른 속도 조정
            CC.ControlLocomotionType();
            //애니메이터의 파라미터 업데이트
            CC.UpdateAnimator();
            //ThirdpersonController
            CC.UpdateCharacterState();
            //캐릭터 회전
            ControlRotation();
            RotateCamera();
            CC.ControlRotationType();//회전
        }

        internal void InPutInit()
        {
            InputReader.Instance.CallMove += OnMove;
            InputReader.Instance.CallRotateCamera += OnMousePos;
            InputReader.Instance.CallRunEnable += OnRunTrue;
            InputReader.Instance.CallRunDisable += OnRunFalse;
            InputReader.Instance.CallEvasionEnable += OnEvation;
            InputReader.Instance.CallLeftShoulder += OnLeftShoulder;
            InputReader.Instance.CallPause += PauseUI;
            InputReader.Instance.CallQuickTab += OnQuickTab;

            //QuickTab
            InputReader.Instance.CallCategoryChangeLeft += CategoryChangeLeft;
            InputReader.Instance.CallCategoryChangeRight += CategoryChangeRight;
            InputReader.Instance.CallSelectChangeLeft += SelectChangeLeft;
            InputReader.Instance.CallSelectChangeRight += SelectChangeRight;
            InputReader.Instance.CallQuickTabSelect += QuickTabSelect;
            InputReader.Instance.CallQuickTabExit += QuickTabExit;
        }

        protected virtual void OnDestroy()
        {
            if (InputReader.HasInstance)
            {
                InputReader.Instance.CallMove -= OnMove;
                InputReader.Instance.CallRotateCamera -= OnMousePos;
                InputReader.Instance.CallRunEnable -= OnRunTrue;
                InputReader.Instance.CallRunDisable -= OnRunFalse;
                InputReader.Instance.CallEvasionEnable -= OnEvation;
                InputReader.Instance.CallLeftShoulder -= OnLeftShoulder;
                InputReader.Instance.CallPause -= PauseUI;
                InputReader.Instance.CallQuickTab -= OnQuickTab;

                //QuickTab
                InputReader.Instance.CallCategoryChangeLeft -= CategoryChangeLeft;
                InputReader.Instance.CallCategoryChangeRight -= CategoryChangeRight;
                InputReader.Instance.CallSelectChangeLeft -= SelectChangeLeft;
                InputReader.Instance.CallSelectChangeRight -= SelectChangeRight;
                InputReader.Instance.CallQuickTabSelect -= QuickTabSelect;
                InputReader.Instance.CallQuickTabExit -= QuickTabExit;
            }

            if (CC != null)
            {
                CC.OnDeadEvent.RemoveListener((GameObject o) => PlayerDead());
            }
        }

        protected void OnMove(Vector2 value)
        {
            CC._input = new Vector3(value.x, 0f, value.y);
        }

        protected void OnMousePos(Vector2 value)
        {
            if (_lockOn.IsLockingOn)
            {
                TargetXRotation = 0f;
                return;
            }

            if (!CameraMoveCondition) { return; }

            _prevMouseDelta = value;
        }

        protected void OnRunTrue()
        {
            CC.IsSprinting = true;
        }

        protected void OnRunFalse()
        {
            CC.IsSprinting = false;
        }

        protected void OnEvation()
        {
            if (!EvasionCondition || CC._input == Vector3.zero) { return; }
            CC.Evation();
        }

        protected void OnLeftShoulder(bool enabled)
        {
            _holdLeftShoulder = enabled;
        }


        protected void PauseUI()
        {
            if(PlayerUIManager.Get() != null)
            {
                PlayerUIManager.Get().PauseUIvisible();
            }

        }

        protected void OnQuickTab()
        {
            if (!QuickTabCondition) { return; }
            PlayerUIManager.Get().GetUI<PlayerQuickTabUI>(TypePlayerUI.QUICKTAB).SetVisible(true);
        }

        #region QuickTabInput

        protected void CategoryChangeLeft()
        {
            PlayerUIManager.Get().GetUI<PlayerQuickTabUI>(TypePlayerUI.QUICKTAB).MoveTab(-1);
        }

        protected void CategoryChangeRight()
        {
            PlayerUIManager.Get().GetUI<PlayerQuickTabUI>(TypePlayerUI.QUICKTAB).MoveTab(1);
        }

        protected void SelectChangeLeft()
        {
            PlayerUIManager.Get().GetUI<PlayerQuickTabUI>(TypePlayerUI.QUICKTAB).MoveItem(-1);
        }

        protected void SelectChangeRight()
        {
            PlayerUIManager.Get().GetUI<PlayerQuickTabUI>(TypePlayerUI.QUICKTAB).MoveItem(1);
        }

        protected void QuickTabSelect()
        {
            PlayerUIManager.Get().GetUI<PlayerQuickTabUI>(TypePlayerUI.QUICKTAB).UseCurrentItem();
        }

        protected void QuickTabExit()
        {
            PlayerUIManager.Get().GetUI<PlayerQuickTabUI>(TypePlayerUI.QUICKTAB).SetVisible(false);
            //InputReader.Instance.EnableMapPlayerControls(true, false);
            //InputReader.Instance.EnableMapBattleMod(true, false);
            InputReader.Instance.EnableActionMap(TypeInputActionMap.BATTLE);
        }

        #endregion

        internal AnimatorMoveSender _animatorMoveSender { get; set; }

        protected bool _useAnimatorMove { get; set; }

        public bool UseAnimatorMove
        {
            get
            {
                return _useAnimatorMove;
            }
            set
            {
                if (_useAnimatorMove != value)
                {
                    if (value)
                    {
                        _animatorMoveSender = gameObject.AddComponent<AnimatorMoveSender>();
                        OnEnableAnimatorMove?.Invoke();
                    }
                    else
                    {
                        if (_animatorMoveSender)
                        {
                            Destroy(_animatorMoveSender);
                        }
                        OnEnableAnimatorMove?.Invoke();
                    }
                }
                _useAnimatorMove = value;
            }
        }

        public void ControlRotation()
        {
            if (CameraMain == null) { return; }
            CC.UpdateMoveDirection(CameraMain.transform);
        }

        private CinemachineVirtualCamera syncCam;
        public void RotateCamera()
        {
            if (_cineCameraFollow == null)
            {
                var cam = CameraManager.Get().GetCamera(TypeCamera.DEFAULT);
                if (cam != null)
                {
                    _cineCameraFollow = cam.VCamera.Follow;
                }
            }

            if (_cineCameraFollow == null)
                return;


            if (_lockOn.IsLockingOn)
            {
                if (syncCam == null)
                    syncCam = CameraManager.Get().GetCamera(TypeCamera.LOCKON).VCamera;

                TargetYRotation = syncCam.transform.eulerAngles.y;

                TargetXRotation = 0f;
                _prevMouseDelta = Vector2.zero;
                return;
            }

            TargetYRotation += _prevMouseDelta.x;
            TargetXRotation += _prevMouseDelta.y;

            TargetXRotation = Mathf.Clamp(TargetXRotation, TopClamp, BottomClamp);

            _cineCameraFollow.transform.rotation = Quaternion.Euler(Vector3.up * TargetYRotation) * Quaternion.Euler(Vector3.right * TargetXRotation);
        }

        //private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        //{
        //    if (lfAngle < -360f) lfAngle += 360f;
        //    if (lfAngle > 360f) lfAngle -= 360f;
        //    return Mathf.Clamp(lfAngle, lfMin, lfMax);
        //}

        public void EnableOnAnimatorMove()
        {
            UseAnimatorMove = true;
        }

        /// <summary>
        /// Disable OnAnimatorMove event
        /// </summary>
        public void DisableOnAnimatorMove()
        {
            UseAnimatorMove = false;
        }


        public void OnAnimatorMoveEvent()
        {
            if (CC == null)
            {
                return;
            }

            CC.ControlAnimatorRootMotion();
            if (onAnimatorMove != null)
            {
                onAnimatorMove.Invoke();
            }
        }

        protected void PlayerDead()
        {
            enabled = false;
        }
    }

    public interface IAnimatorMoveReceiver
    {
        bool enabled { get; set; }

        void OnAnimatorMoveEvent();
    }

    public class AnimatorMoveSender : MonoBehaviour
    {
        protected void Awake()
        {
            hideFlags = HideFlags.HideInInspector;
            IAnimatorMoveReceiver[] animatorMoves = GetComponents<IAnimatorMoveReceiver>();
            for (int i = 0; i < animatorMoves.Length; i++)
            {
                var receiver = animatorMoves[i];
                animatorMoveEvent += () =>
                {
                    if (receiver.enabled)
                    {
                        receiver.OnAnimatorMoveEvent();
                    }
                };
            }
        }

        public System.Action animatorMoveEvent;

        protected void OnAnimatorMove()
        {
            animatorMoveEvent?.Invoke();
        }
    }
}
