using Cinemachine;
using UnityEngine;

namespace Zeus
{
    public class LockOn : LockOnBehaviour
    {
        #region

        [Header("Player")]
        protected ThirdPersonController _cc;
        protected PlayerCombatManager _combatManager;

        //[Header("Cinemachine")]
        //public CinemachineVirtualCamera CineCamera;

        public RectTransform AimImagePrefab;
        public Canvas AimImageContainer;
        public Vector2 AimImageSize = new Vector2(30, 30);
        //True일 경우 LockOn상태일 때만 켜져있음. False의 경우 항상 켜져있음
        public bool HideSprite = true;
        [Range(-0.5f, 0.5f)]
        public float SpriteHeight = 0.25f;
        public float CameraHeightOffset;

        internal bool IsLockingOn;
        private RectTransform _aimImage;
        protected bool _inTarget;
        #endregion

        public RectTransform AimImage
        {
            get
            {
                if (_aimImage)
                {
                    return _aimImage;
                }
                if (AimImageContainer)
                {
                    _aimImage = Instantiate(AimImagePrefab, Vector2.zero, Quaternion.identity);
                    _aimImage.SetParent(AimImageContainer.transform);
                    _aimImage.sizeDelta = AimImageSize;
                    return _aimImage;
                }
                else
                {
                    Debug.LogWarning("LockOn 스크립트 에러");
                }
                return null;
            }
            set
            {
                _aimImage = value;
            }
        }

        private bool _lockOnCondition = true;
        public bool LockOnCondition
        {
            get
            {
                if (_cc._cantInputToAnimator || !_lockOnCondition || _cc._cantInputExceptionMove || _combatManager.CurrentWeaponType == TypeWeapon.BOW)
                {
                    return false;
                }
                return true;
            }

            set
            {
                _lockOnCondition = value;
            }
        }

        void Start()
        {
            Init();
            //캐릭터 사망시 LockOn 상태 해제
            GetComponent<HealthController>().OnDeadEvent.AddListener((GameObject g) =>
            {
                IsLockingOn = false;
                LockOnTarget(false);
                UpdateLockOn();
            });

            if (!AimImageContainer)
            {
                AimImageContainer = gameObject.GetComponentInChildren<Canvas>(true);
            }

            _cc = GetComponent<ThirdPersonController>();
            if (_cc != null)
            {
                InputReader.Instance.CallLockOn += LockOnInput;
                InputReader.Instance.CallNextTarget += ChangeTargetLeft;
                InputReader.Instance.CallPreviousTarget += ChangeTargetRight;
            }

            _combatManager = GetComponent<PlayerCombatManager>();
            AimImageActive(false);
        }

        private void OnDestroy()
        {
            if (InputReader.HasInstance)
            {
                InputReader.Instance.CallLockOn -= LockOnInput;
                InputReader.Instance.CallNextTarget -= ChangeTargetLeft;
                InputReader.Instance.CallPreviousTarget -= ChangeTargetRight;
            }
        }

        private void LateUpdate()
        {
            UpdateLockOn();
        }

        protected virtual void UpdateLockOn()
        {
            //LockOn 대상이 살아있는지 확인
            CheckForCharacterAlive();
            //Aim이미지 위치
            UpdateAimImage();
        }

        protected virtual void LockOnInput()
        {
            if (!LockOnCondition) { return; }
            var locking = CurrentTarget == null;
            IsLockingOn = LockOnTarget(locking);
        }


        public virtual void ChangeTargetLeft()
        {
            base.ChangeTarget(TypeTargetSearch.LEFT);
        }

        public virtual void ChangeTargetRight()
        {
            base.ChangeTarget(TypeTargetSearch.RIGHT);
        }

        protected override void SetTarget()
        {
            if (CurrentTarget == null)
                return;

            CameraManager.Get().GetCamera(TypeCamera.LOCKON).VCamera.LookAt = CurrentTarget;
            CameraManager.Get().TurnOnCamera(TypeCamera.LOCKON);
            var boundY = CurrentTarget.GetComponent<Collider>().bounds;
            var camData = CameraManager.Get().GetCamera(TypeCamera.LOCKON);
            var composer = camData.VCamera.GetCinemachineComponent<CinemachineComposer>();

            composer.m_TrackedObjectOffset.y = Mathf.Clamp(Vector3.Distance(boundY.min, boundY.max) / 2, 0, 2f);

            //CineCamera.enabled = true;
            //CineCamera.LookAt = CurrentTarget;

            //CineCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.y = Vector3.Distance(boundY.min, boundY.max) / 2;
        }

        public virtual void StopLockOn()
        {
            IsLockingOn = false;
            _inTarget = false;
            _target = null;
            AimImageActive(false);
            CameraManager.Get().TurnOnCamera(TypeCamera.DEFAULT);
            CameraManager.Get().GetCamera(TypeCamera.LOCKON).VCamera.LookAt = null;
            CameraManager.Get().GetCamera(TypeCamera.LOCKON).VCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.y = 0f;
        }

        private void CheckForCharacterAlive()
        {
            if (_inTarget && !IsTargetCharacterAlive())
            {
                ResetLockOn();
                StopLockOn();
            }
            if (_target == null) { return; }

            //카메라 밖으로 대상이 나간 경우
            if (CheckForCharacterOutCamera())
            {
                ResetLockOn();
                StopLockOn();
            }
        }

        ///Tony
        ///대상이 카메라 밖으로 나가면 락온 해제
        private bool CheckForCharacterOutCamera()
        {
            var result = Util.CheckObjectIsInCamera(_camera, _target.position);
            return !result;
        }

        protected bool LockOnTarget(bool doLockOn)
        {
            var result = SetLockOn(doLockOn);

            if (doLockOn && result)
            {
                SetTarget();
                AimImageActive(true);
            }
            else if (_inTarget && !result) // 전에는 타겟을 잡았었는데 이번에 실패했다.
            {
                StopLockOn();
            }

            _inTarget = result;

            return _inTarget;
        }

        private void AimImageActive(bool value)
        {
            AimImage.gameObject.SetActive(value);
        }

        protected void UpdateAimImage()
        {
            if (!AimImageContainer || !AimImage) { return; }

            if (HideSprite)
            {
                AimImage.sizeDelta = AimImageSize;
                if (CurrentTarget && !AimImage.transform.gameObject.activeSelf && IsTargetCharacterAlive())
                {
                    //타겟 존재, 에임게임 오브젝트가 꺼져있있음, 타겟이 살아 있는 경우 AimImage를 켠다
                    AimImage.transform.gameObject.SetActive(true);
                }
                else if (!CurrentTarget && AimImage.transform.gameObject.activeSelf)
                {
                    AimImage.transform.gameObject.SetActive(false);
                }
                else if (_aimImage.transform.gameObject.activeSelf && !IsTargetCharacterAlive())
                {
                    AimImage.transform.gameObject.SetActive(false);
                }
                else
                {
                    AimImage.transform.gameObject.SetActive(false);
                }
            }
            //에임이 타겟을 쫓음
            if (CurrentTarget && AimImage && AimImageContainer)
            {
                AimImage.anchoredPosition = CurrentTarget.GetScreenPointOffBoundsCenter(AimImageContainer, _camera, SpriteHeight);
            }
            else if (AimImageContainer)
            {
                AimImage.anchoredPosition = Vector2.zero;
            }

        }
    }
}