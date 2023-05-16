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
        //True�� ��� LockOn������ ���� ��������. False�� ��� �׻� ��������
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
                    Debug.LogWarning("LockOn ��ũ��Ʈ ����");
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
            //ĳ���� ����� LockOn ���� ����
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
            //LockOn ����� ����ִ��� Ȯ��
            CheckForCharacterAlive();
            //Aim�̹��� ��ġ
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

            //ī�޶� ������ ����� ���� ���
            if (CheckForCharacterOutCamera())
            {
                ResetLockOn();
                StopLockOn();
            }
        }

        ///Tony
        ///����� ī�޶� ������ ������ ���� ����
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
            else if (_inTarget && !result) // ������ Ÿ���� ��Ҿ��µ� �̹��� �����ߴ�.
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
                    //Ÿ�� ����, ���Ӱ��� ������Ʈ�� ����������, Ÿ���� ��� �ִ� ��� AimImage�� �Ҵ�
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
            //������ Ÿ���� ����
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