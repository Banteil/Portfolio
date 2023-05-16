using UnityEngine;
namespace Zeus
{
    public class BowObject : AttackObject
    {
        [Header("Bow")]
        //활 시위 당긴상태
        private bool _drawString;
        public Transform BowString;
        private Vector3 _bowStringDefaultPos;
        public Transform BowBorn;
        [HideInInspector]
        //플레이어 손 위치
        public Transform HandPos;
        public ArrowProjectile CurrentProtectile;
        private Transform _arrowModel;
        public int DefaultProjectileID;

        //AIM
        //메인 카메라
        private Transform _aimTarget;
        //에임 거리
        private float _aimDistance;
        //최종 벡터
        private Vector3 _aimVector;
        private Vector3 _aimPos;
        private PlayerAimTypeUI _aimUI;

        [SerializeField]
        private float MaxPullTime = 3f;
        private float _pullGauge = 0f;
        private bool _pulling;

        protected override void Initialization()
        {
            base.Initialization();
            if (PlayerUIManager.Get() != null)
            {
                _aimUI = PlayerUIManager.Get().GetUI<PlayerAimTypeUI>(TypePlayerUI.AIM);
            }
        }

        private void OnEnable()
        {
            _bowStringDefaultPos = BowString.localPosition;
        }

        private void OnDisable()
        {
            DrawCancel();
        }

        private void FixedUpdate()
        {
            if (_pulling)
            {
                StringPositionUpdate();
                _pullGauge += Time.fixedDeltaTime;
            }

            if (_aimTarget != null)
            {
                SetVector();
            }

            if (_aimUI != null)
            {
                //UpdateCrossHair();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(_aimPos, _aimVector);
        }

        private void SetVector()
        {
            if (CurrentProtectile == null)
            {
                //_aimVector = Camera.main.transform.position + Camera.main.transform.forward;
                return;
            }
            var fowardVec = BowBorn.transform.position + (BowBorn.transform.right * -1) * _aimDistance;
            var aimTargetVec = _aimTarget.transform.position + _aimTarget.forward * _aimDistance;
            var vec = new Vector3(aimTargetVec.x, fowardVec.y, aimTargetVec.z);
            _aimVector = aimTargetVec;
            _aimPos = _aimTarget.transform.position + _aimTarget.forward * Vector3.Distance(BowBorn.position, _aimTarget.position);
            CurrentProtectile.transform.position = _aimPos;
            CurrentProtectile.transform.LookAt(_aimVector);
        }

        private void UpdateCrossHair()
        {
            if (CurrentProtectile != null)
            {
                var crossHair = _aimUI.BowCrossHair;
                crossHair.transform.position = Camera.main.WorldToScreenPoint(_aimVector);
            }
        }

        private void StringPositionUpdate()
        {
            if (_drawString)
            {
                BowString.position = HandPos.position;
            }
            else
            {
                BowString.localPosition = _bowStringDefaultPos;
            }
        }

        public void DrawBow(Transform aimTarget, float aimDistance)
        {
            SetProjectile(CombatManager.tag);
            _aimTarget = aimTarget;
            _aimDistance = aimDistance;
            _pulling = true;
        }

        //애니메이터 및 PlayerCombatManager에서 실행
        //활 시위를 당긴 상태
        public void DrawBowString(bool value)
        {
            _drawString = value;
        }

        public void Pulling()
        {
            _pulling = true;
        }

        public void Shoot()
        {
            if (CurrentProtectile == null) { return; }
            CurrentProtectile.transform.SetParent(null);
            _arrowModel.transform.SetParent(null);
            CurrentProtectile.LookPoint = null;

            CurrentProtectile.transform.LookAt(_aimVector);
            _arrowModel.transform.LookAt(_aimVector);
            BowString.localPosition = _bowStringDefaultPos;
            _pulling = false;
            _pullGauge = 0;
            var powerMultiplier = MaxPullTime != 0 ? _pullGauge / MaxPullTime : 0;
            CurrentProtectile.Fire(powerMultiplier);
            CurrentProtectile = null;
        }

        public void DrawCancel()
        {
            if (CurrentProtectile != null)
            {
                if (CurrentProtectile.gameObject.activeInHierarchy)
                {
                    Destroy(CurrentProtectile.gameObject);
                    Destroy(_arrowModel.gameObject);
                }
            }
            CurrentProtectile = null;
            _pulling = false;
            _pullGauge = 0;
            BowString.localPosition = _bowStringDefaultPos;
        }

        protected void SetProjectile(string tag)
        {
            var ob = EquipManager.Get().GetWeaponModel(DefaultProjectileID, false);
            if (ob == null)
            {
                Debug.LogError($"Not Found Arrow ID : {DefaultProjectileID}");
                return;
            }
            var arrow = Instantiate(ob);
            arrow.SetActive(true);
            CurrentProtectile = arrow.GetComponent<ArrowProjectile>();
            CombatManager.HitProperties.AttackNum = 0;
            CurrentProtectile.Owner = CombatManager;
            CurrentProtectile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            CurrentProtectile.LookPoint = BowBorn;
            CurrentProtectile.tag = tag;
            CurrentProtectile.Initialized();
            _arrowModel = CurrentProtectile.ProjectileMovement.ArrowModel;
            _arrowModel.transform.SetParent(HandPos);
            _arrowModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        public void ChangeProjectile(int id)
        {
            DrawCancel();

            var ob = EquipManager.Get().GetWeaponModel(id, false);
            if (ob == null)
            {
                Debug.LogError($"Not Found Arrow ID : {DefaultProjectileID}");
                return;
            }
            var arrow = Instantiate(ob);
            arrow.SetActive(true);
            CurrentProtectile = arrow.GetComponent<ArrowProjectile>();
            CombatManager.HitProperties.AttackNum = 0;
            CurrentProtectile.Owner = CombatManager;
            CurrentProtectile.transform.SetParent(HandPos);
            CurrentProtectile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            CurrentProtectile.LookPoint = BowBorn;
            CurrentProtectile.tag = tag;
            CurrentProtectile.Initialized();
            _arrowModel = CurrentProtectile.ProjectileMovement.ArrowModel;
            _arrowModel.transform.SetParent(HandPos);
            _arrowModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            Pulling();
            DrawBowString(true);
        }
    }
}