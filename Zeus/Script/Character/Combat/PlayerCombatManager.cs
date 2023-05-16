using UnityEngine;
using UnityEngine.SceneManagement;

namespace Zeus
{
    public enum TypeGrapHand { LEFT, RIGHT }
    public enum TypeSkillSpawnPos { NONE, RIGHTHAND, LEFTHAND, HANDS }

    [System.Serializable]
    public class PlayerCombatManager : CombatManager
    {
        const string _leftWeaponString = "LeftLowerArm";
        const string _rightWeaponString = "RightLowerArm";
        internal override ZoneInfo ZoneInfo
        {
            get => base.ZoneInfo;
            set
            {
                base.ZoneInfo = value;
                if (value != null)
                {
                    if (TableManager.Instance != null)
                    {
                        TableManager.CurrentPlayerData.SaveZoneData.SceneName = SceneLoadManager.NextScene;
                        TableManager.CurrentPlayerData.SaveZoneData.ZoneID = value.ZoneID;
                    }
                }
            }
        }

        private ThirdPersonController _cc;
        public ThirdPersonController CC
        {
            get
            {
                if (_cc == null)
                {
                    _cc = GetComponent<ThirdPersonController>();
                }
                if (_cc == null)
                {
                    Debug.LogError("ThirdPersonController가 없습니다");
                }
                return _cc;
            }
        }
#if UNITY_EDITOR
        [SerializeField] private GameObject _testWeapon;
#endif

        //전에 장착했던 무기
        internal int PreWeaponID;
        internal int CurrentWeaponID;
        public TypeWeapon CurrentWeaponType;

        [SerializeField] private Transform _rightHandPos;
        [SerializeField] private Transform _leftHandPos;
        [SerializeField] private Transform _rightFingerPos;

        private LockOn _lockOn;

        #region Bow

        private bool _arrowReady;
        private int _bulletCount;
        private const int _maxBullet = 4;

        //활 사운드 아이디
        private const int _bowDrawSoundID = 114;
        private const int _bowShootSoundID = 116;
        //사운드 중지시 필요
        private int _bowDrawSoundInstanceID;

        #endregion

        #region Guard And Parry
        public float ParryDuring = 0.2f;
        protected float _currentParryTime;

        public float ParryDelay = 0.1f;
        [SerializeField]
        protected float _currentParryDelay;
        #endregion

        private const int _dissolveSoundID = 73;

        private void Awake()
        {
            //추후 UI씬을 어디서 불러올지 생각
            SceneManager.LoadScene("PlayerUI", LoadSceneMode.Additive);
        }

        protected override void Start()
        {
            EquipManager.Get().CallWeaponChange += OnWeaponChange;
            EquipManager.Get().CallWeaponDissolveComplete += OnWeaponDissolve;
            CC.onCancelAction += UnEquipBow;
            CC.OnGuardSuccess += GuardSuccess;
            CC.OnParrySuccess += ParrySuccess;
            InputReader.Instance.CallPotion += UnEquipBow;
            CC.onEvasionEvent += ParryEnd;
            BodyInitialized();
            //_defaultAnimator = CC.Animator.runtimeAnimatorController;
            _lockOn = GetComponent<LockOn>();
        }

        private void OnDisable()
        {
            CC.onCancelAction -= UnEquipBow;
            CC.OnGuardSuccess -= GuardSuccess;
            CC.OnParrySuccess += ParrySuccess;
            CC.onEvasionEvent -= ParryEnd;
            if (InputReader.HasInstance)
                InputReader.Instance.CallPotion -= UnEquipBow;
        }

        public override void OnDamageHit(Damage damageInfo)
        {
            base.OnDamageHit(damageInfo);

            var tableData = TableManager.GetWeaponTableData(CurrentWeaponID);
            if (tableData == null) { return; }
            var soundTableData = tableData.HitSoundIDs[damageInfo.AttackerHitProperties.AttackNum];

            var soundData = TableManager.GetSoundTable().GetData(soundTableData);

            if (soundData == null) { return; }

            if (soundData.RandomAssetName.Length == 0)
            {
                SoundManager.Instance.Play(soundTableData, transform.position);
            }
            else
            {
                var randomSound = soundData.RandomAssetName[Random.Range(0, soundData.RandomAssetName.Length)];
                SoundManager.Instance.Play(randomSound, transform.position);
            }
        }

        public void OnWeaponChange(WeaponTableData changeWeapon)
        {
#if UNITY_EDITOR
            if (_testWeapon)
                return;
#endif
            PreWeaponID = CurrentWeaponID;

            if (CC.RigController != null)
            {
                CC.RigController.DeActiveIK();
            }

            ReleaseWeapon();

            CC.Animator.SetInteger(AnimatorParameters.WeaponType, (int)changeWeapon.WeaponCategory);

            HitProperties.SenderInfo.WeaponID = changeWeapon.ID;

            if (!string.IsNullOrEmpty(changeWeapon.LeftWeapon))
            {
                EquipModel(_leftWeaponString);
            }
            if (!string.IsNullOrEmpty(changeWeapon.RightWeapon))
            {
                EquipModel(_rightWeaponString);
            }

            //데미지정보에 무기아이디를 줘서 사운드 찾아온다.
            if (LeftWeapon != null || RightWeapon != null)
            {
                HitProperties.SenderInfo.WeaponID = changeWeapon.ID;
            }

            //if (changeWeapon.WeaponCategory == TypeWeapon.TWOHAND)
            //{
            //    CC.RigController.ActiveIK(_leftWeaponString, RightWeapon.IKHandle);
            //}

            CurrentWeaponType = changeWeapon.WeaponCategory;
            CurrentWeaponID = changeWeapon.ID;
            if (CurrentWeaponType == TypeWeapon.BOW)
            {
                //활 잔탄수 초기화
                _bulletCount = _maxBullet;
                PlayerUIManager.Get().GetUI<PlayerAimTypeUI>(TypePlayerUI.AIM).ReloadBullet();
            }

            //공격 도중 무기를 바꿨을시
            if (CC.IsAttack)
            {
                CC.Animator.SetTrigger("AttackReset");
                LightAttack();
            }
        }

        private void EquipModel(string hand)
        {
            int index = hand.Equals(_leftWeaponString) ? (int)TypeGrapHand.LEFT : (int)TypeGrapHand.RIGHT;
            var weapon = EquipManager.Get().GetWeaponModel(index);
            var parent = hand.Equals(_leftWeaponString) ? _leftHandPos : _rightHandPos;
            weapon.transform.SetParent(parent);
            weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (hand.Equals(_leftWeaponString))
            {
                LeftWeapon = weapon.GetComponent<AttackObject>();
                if (LeftWeapon != null)
                {
                    LeftWeapon.CombatManager = this;
                }
            }
            else if (hand.Equals(_rightWeaponString))
            {
                RightWeapon = weapon.GetComponent<AttackObject>();
                if (RightWeapon != null)
                {
                    RightWeapon.CombatManager = this;
                }
            }

            weapon.SetActive(false);
        }

        public void ReleaseWeapon()
        {
            if (LeftWeapon != null)
            {
                LeftWeapon.gameObject.SetActive(false);
                LeftWeapon = null;
            }

            if (RightWeapon != null)
            {
                RightWeapon.gameObject.SetActive(false);
                RightWeapon = null;
            }
        }

        #region Gaurd And Parry

        public void ParrySuccess(Damage damage)
        {
            GuardEffect();
            var targetVector = damage.Sender.position - transform.position;
            CC.RotateToDirection(targetVector.normalized, 100f);
            CC.Animator.SetTrigger(AnimatorParameters.Parry);
            CC.OnFixedUpdate += ParrySuccessMove;
        }
        public void ParryEffect()
        {
            //var weaponData = TableManager.GetWeaponTableData(_combatInput.CombatManager.CurrentWeaponID);
            //var soundData = TableManager.GetSoundTable().GetData(weaponData.ParrySoundID);

            //SoundManager.Instance.Play(weaponData.ParrySoundID, false);

            ///재질별로 패링 사운드가 날 경우 아래 코드 사용

            //var soundData = TableManager.GetSoundTable().GetData((int)TypeHitMaterial.METAL);
            ////패리성공 사운드.
            //if (soundData.RandomAssetName.Length == 0)
            //{
            //    SoundManager.Instance.Play(_parrySoundID, transform.position);
            //}
            //else
            //{
            //    var randomSound = soundData.RandomAssetName[Random.Range(0, soundData.RandomAssetName.Length)];
            //    SoundManager.Instance.Play(randomSound, transform.position);
            //}

            CameraManager.Get().CameraShake(3f, 0.2f);
            var weaponPosition = LeftWeapon != null ? LeftWeapon.transform.position : RightWeapon.transform.position;
            EffectsManager.Get().SetEffect((int)TypeHitMaterial.METAL, weaponPosition, transform.forward, null, 1f);
        }

        private void ParrySuccessMove()
        {
            _currentParryDelay = 0;
            CC.OnFixedUpdate -= ParryCooldown;
            CC.AddActionState(TypeCharacterState.PARRY);
            //방패를 올린 상태까지 대기
            if (CC.IsParrySuccess)
            {
                var weaponData = TableManager.GetWeaponTableData(CurrentWeaponID);
                var soundData = TableManager.GetSoundTable().GetData(weaponData.ParrySoundID);

                SoundManager.Instance.Play(weaponData.ParrySoundID, false);

                var handGrap = LeftWeapon == null ? TypeGrapHand.RIGHT : TypeGrapHand.LEFT;
                CC.RemoveActionState(TypeCharacterState.PARRY_READY);
                CC.OnFixedUpdate -= ParrySuccessMove;
                GameTimeManager.Instance.SetWorldTimeScale(0.3f, 0.5f);
            }
        }

        //애니메이션으로도 호출
        public void ParryEnd()
        {
            if (!CC.CharacterState.HasFlag(TypeCharacterState.PARRY)) { return; }
            Debug.Log("패리 애니메이션 종료");
            CC.RemoveActionState(TypeCharacterState.PARRY);
            //연속 패리 기능을 위해 남겨둠
        }

        public void Gaurd()//가드 버튼 눌렀을 시 패리가능한 시간을 받아옴
        {
            Debug.Log("패리 인풋");
            CC.AddActionState(TypeCharacterState.GUARD);
            if (_currentParryDelay <= 0)
            {
                CC.AddActionState(TypeCharacterState.PARRY_READY);
                _currentParryDelay = ParryDelay;
                CC.OnFixedUpdate += ParryCooldown;
            }
            else
            {
                Debug.Log($"테스트 : 패리 딜레이중 {_currentParryDelay}");
            }

            //패리 상태시에 다시 패리 인풋을 누른 경우
            if (CC.CharacterState.HasFlag(TypeCharacterState.PARRY))
            {
                //추가적인 작업이 없으면 ! 사용해서 코드 간소화
            }
            else
            {
                CC.Animator.SetBool(AnimatorParameters.IsBlock, true);
                //무기 타입에 따라 왼/오른 손에 있는 무기 활성화
                if (CurrentWeaponType == TypeWeapon.TWOHAND)
                {
                    WeaponDissolve(0f, TypeGrapHand.RIGHT, 0.4f);
                }
                else
                {
                    WeaponDissolve(0f, TypeGrapHand.LEFT, 0.4f);
                }
            }

            _currentParryTime = 0;
            _cc.OnFixedUpdate += GaurdState;
        }

        public void GuardSuccess(Damage damage)
        {
            CC.Animator.Play("SwordAndShield", 3, 0);
            CC.RemoveActionState(TypeCharacterState.PARRY);
            GuardEffect();
            var weaponData = TableManager.GetWeaponTableData(TableManager.CurrentPlayerData.GetWeaponID());
            var soundData = TableManager.GetSoundTable().GetData(weaponData.GuardSoundID);

            SoundManager.Instance.Play(weaponData.GuardSoundID);
            SoundManager.Instance.Play(weaponData.GuardSoundID, false, true);
            CharacterObjectManager.Get().AddHitBackList(CC.GUID, damage);
        }

        private void GuardEffect()
        {
            var tableData = TableManager.GetWeaponTableData(TableManager.CurrentPlayerData.GetWeaponID());
            var effectPath = tableData.GuardEffect;
            var hitPath = tableData.GuardHitEffect;
            if (string.IsNullOrEmpty(hitPath) || string.IsNullOrEmpty(effectPath))
            {
                return;
            }
            //실드 히트 이펙트
            TableManager.Instance.GetGameObjectAsync(hitPath, result =>
            {
                if (result == null)
                {
                    return;
                }
                var hitPosition = LeftWeapon != null ? LeftWeapon.transform.position : RightWeapon.transform.position;
                result.transform.LookAt(transform.forward);
                result.transform.SetPositionAndRotation(hitPosition, result.transform.rotation);
                EffectsManager.Get().SetEffect(result, hitPosition, null, 2f);
            });
        }


        private void GaurdState()
        {
            _currentParryTime += Time.fixedDeltaTime * GameTimeManager.Instance.WorldTimeScale;
            //패리 지속시간 끝났을시
            if (_currentParryTime >= ParryDuring)
            {
                CC.RemoveActionState(TypeCharacterState.PARRY_READY);
            }

            //패리 상태 => 패리 종료 상태로 변경시 가드 애니메이션 출력되도록
            if (CC.CharacterState.HasFlag(TypeCharacterState.PARRY) && !CC.Animator.GetBool(AnimatorParameters.IsBlock))
            {
                CC.Animator.SetBool(AnimatorParameters.IsBlock, true);
                //무기 타입에 따라 왼/오른 손에 있는 무기 활성화
                if (CurrentWeaponType == TypeWeapon.TWOHAND)
                {
                    WeaponDissolve(0f, TypeGrapHand.RIGHT, 0.4f);
                }
                else
                {
                    WeaponDissolve(0f, TypeGrapHand.LEFT, 0.4f);
                }
            }
            else if (!CC.IsBlocking)
            {
                CC.Animator.SetBool(AnimatorParameters.IsBlock, false);
                CC.RemoveActionState(TypeCharacterState.GUARD);
                CC.OnFixedUpdate -= GaurdState;
            }
        }

        private void ParryCooldown()
        {
            _currentParryDelay -= Time.fixedDeltaTime * GameTimeManager.Instance.WorldTimeScale;
            if (_currentParryDelay <= 0)
            {
                Debug.Log("패리 딜레이 종료");
                CC.OnFixedUpdate -= ParryCooldown;
            }
        }

        public void GuardOff()
        {
            CC.RemoveActionState(TypeCharacterState.GUARD | TypeCharacterState.PARRY_READY);
            CC.Animator.SetBool(AnimatorParameters.IsBlock, false);
            //EffectsManager.Get().ReleaseEffect(_guardEffectInstantID);

            //무기 타입에 따라 왼/오른 손에 있는 무기 비활성화
            if (CurrentWeaponType == TypeWeapon.TWOHAND)
            {
                WeaponDissolve(1f, TypeGrapHand.RIGHT);
            }
            else
            {
                WeaponDissolve(1f, TypeGrapHand.LEFT);
            }
        }

        #endregion

        #region LightAttack

        public void LightAttack()
        {
            if (CC.Animator == null) { return; }
            CC.Animator.SetTrigger(AnimatorParameters.LightAttack);
            AttackTarget();
        }

        public void HeavyAttack()
        {
            //카운터 어택일 시
            if (CC.IsParrySuccess)
            {
                CC.Animator.SetTrigger(AnimatorParameters.CounterAttack);
                WeaponDissolve(5f, TypeGrapHand.RIGHT);
                //패리 상태 해제
                ParryEnd();
            }
            else
            {
                //강공격
                if (CC.Animator == null) { return; }
                CC.Animator.SetTrigger(AnimatorParameters.HeavyAttack);
                AttackTarget();
            }
        }
        #endregion

        #region Bow

        //인풋으로 실행
        public void EqiupBow()
        {
            if (CC.Animator == null) { return; }
            CC.Animator.ResetTrigger(AnimatorParameters.LightAttack);
            CC.Animator.SetTrigger("ResetTrigger");
            CC.Animator.Play("Null", CC.FullBodyLayer);

            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null)
            {
                Debug.LogError("보우 오브젝트가 없습니다");
                return;
            }

            bow.HandPos = _rightFingerPos;
            if (_lockOn != null)
            {
                if (_lockOn.IsLockingOn)
                {
                    var targetPos = _lockOn.CurrentTarget.transform.position;
                    _lockOn.StopLockOn();

                    var vCam = CameraManager.Get().GetActiveCamera();
                    var thirdCam = vCam.VCamera.GetCinemachineComponent<Cinemachine.Cinemachine3rdPersonFollow>();

                    var offset = transform.right * thirdCam.ShoulderOffset.x * -1;
                    var lookDir = targetPos - transform.position + offset;
                    Quaternion rot = Quaternion.LookRotation(lookDir.normalized);
                    CC.CombatInput.TargetXRotation = rot.eulerAngles.x * -1;
                    CC.CombatInput.TargetYRotation = rot.eulerAngles.y;
                }
            }

            CameraManager.Get().TurnOnCamera(TypeCamera.AIM);
            PlayerUIManager.Get().GetUI<PlayerAimTypeUI>(TypePlayerUI.AIM).SetVisible(true, 0.5f);
            CC.Animator.SetBool(AnimatorParameters.IsAim, true);
            ProjectileSpawn();
            bow.DrawBowString(true);
        }

        //인풋으로 실행
        public void UnEquipBow()
        {
            if (CC.Animator == null || CurrentWeaponType != TypeWeapon.BOW) { return; }
            CameraManager.Get().TurnOnCamera(TypeCamera.DEFAULT);
            EquipManager.Get().Equip(TypeEquipPosition.WEAPON, TableManager.CurrentPlayerData.GetWeaponID());
            BowVariableInit();
            BowObjectStateInit();
        }

        public void BowVariableInit()
        {
            CC.Animator.SetBool(AnimatorParameters.IsAim, false);
            PlayerUIManager.Get().GetUI<PlayerAimTypeUI>(TypePlayerUI.AIM).SetVisible(false, 0.5f);
            CC.Animator.SetBool("HoldBow", false);
            CC.Animator.Play("Null", CC.BowUpperLayer, 0);
            CC.Animator.SetBool(AnimatorParameters.WeaponEquip, false);
            CC.ResetAttackTriggers();
        }

        //애니메이터에서도 생성
        private void ProjectileSpawn()
        {
            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null || _bulletCount <= 0) return;
            bow.DrawBow(CC._rotateTarget, CC.AimPoint);
            _arrowReady = true;
        }

        public void ProjectileChange(int id)
        {
            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null) return;

            bow.ChangeProjectile(id);
            _bulletCount = _maxBullet;
            _arrowReady = true;
            PlayerUIManager.Get().GetUI<PlayerAimTypeUI>(TypePlayerUI.AIM).ReloadBullet();
        }

        public void BowObjectStateInit()
        {
            if (LeftWeapon == null) { return; }
            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null) return;
            bow.DrawCancel();
        }

        //조준상태(RT 당긴 상태)
        public void DrawBow()
        {
            //화살 없는 상태시에 리턴, 잔여 화살이 없는경우 리턴
            if (!_arrowReady || _bulletCount <= 0) { return; }
            CC.Animator.SetBool("HoldBow", true);

            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null) return;
            bow.Pulling();

            _bowDrawSoundInstanceID = SoundManager.Instance.Play(_bowDrawSoundID);
        }

        //발사(RT 뗀 상태)
        public void ShootBow()
        {
            //화살 없는 상태시에 리턴 및 애니메이션 상태에 따른 리턴
            if (!_arrowReady || !CC.Animator.GetBool("HoldBow")) { return; }
            SoundManager.Instance.StopEffect(_bowDrawSoundInstanceID);
            SoundManager.Instance.Play(_bowShootSoundID);
            CC.Animator.SetBool("HoldBow", false);
            CC.Animator.SetTrigger(AnimatorParameters.HeavyAttack);
            if (CC.Animator == null) { return; }
            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null) { return; }
            bow.Shoot();
            _arrowReady = false;
            bow.DrawBowString(false);

        }

        //활 발사시(애니메이터에서 실행)
        public override void ShootProjectile(AttackInfo attackInfo)
        {
            if (CC.Animator == null) { return; }
            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null) { return; }
            _bulletCount -= 1;
            PlayerUIManager.Get().GetUI<PlayerAimTypeUI>(TypePlayerUI.AIM).SetBullet(_bulletCount);
            bow.Shoot();
        }

        ///활 시위 당긴상태
        ///애니메이션에서 실행시킴
        public void DrawString()
        {
            if (CC.Animator == null) { return; }
            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null) { return; }

            bow.DrawBowString(true);
        }
        #endregion

        #region Excution
        /// <param name="owner">처형 실행자(플레이어)</param>
        /// <param name="target">처형 대상자(몬스터)</param>
        internal override void Excution(Character player, Character target)
        {
            if (target == null) return;

            var excutionIndex = target.Excutions.FindIndex(x => x.DirectionType == TypeExcutionDirection.FRONT);
            if (excutionIndex == -1)
                return;

            // 처형 데이터가 있다면 처형이 가능하므로 진행!
            _lockOn.StopLockOn();

            //타겟 행동 정지 및 플레이어 인풋 막기
            CC.CantInput = true;

            WeaponDissolve(5f, TypeGrapHand.RIGHT);

            var excutionData = target.Excutions[excutionIndex];
            ExcutionManager.Get().Excute(CC, target, excutionData, () =>
            {
                CC.CantInput = false;
            });

        }

        //처형 조건
        //public bool ExcutionCheck()
        //{
        //    //확률
        //    var rand = Random.Range(0, 100);
        //    if (rand < 50) return false;

        //    var Radius = 5f;
        //    var layerMask = 1 << LayerMask.NameToLayer("Character");
        //    var angleRange = 45f;
        //    Collider[] hits = Physics.OverlapSphere(transform.position, Radius, layerMask);

        //    foreach (Collider target in hits)
        //    {
        //        if (target.CompareTag("Enemy"))
        //        {
        //            Vector3 interV = target.gameObject.transform.position - transform.position;

        //            // target과 나 사이의 거리가 radius 보다 작다면
        //            if (interV.magnitude <= Radius)
        //            {
        //                // '타겟-나 벡터'와 '내 정면 벡터'를 내적
        //                float dot = Vector3.Dot(interV.normalized, transform.forward);
        //                // 두 벡터 모두 단위 벡터이므로 내적 결과에 cos의 역을 취해서 theta를 구함
        //                float theta = Mathf.Acos(dot);
        //                // angleRange와 비교하기 위해 degree로 변환
        //                float degree = Mathf.Rad2Deg * theta;

        //                // 시야각 판별
        //                if (degree <= angleRange / 2f)
        //                {
        //                    var targetChar = target.GetComponent<Character>();
        //                    if (targetChar != null)
        //                    {
        //                        var excutionHP = targetChar.MaxHealth * 20 / 100;
        //                        ///처형 조건 => 대상 체력이 최대 체력의 20% 이하로 남아있을때
        //                        /// > 수식을 < 로 바꾸어야 됨 (테스트를 위해 바꾸어두었음)
        //                        if (!targetChar.IsDead && targetChar.CurrentHealth <= excutionHP)
        //                        {
        //                            var tableData = TableManager.GetWeaponTableData(CurrentWeaponID);
        //                            //처형 애니메이션이 있는경우
        //                            if (tableData.ExcutionID != 0)
        //                            {
        //                                Excution(GetComponent<Character>(), targetChar);
        //                                //Excution(targetChar, tableData.ExcutionID);
        //                                return true;
        //                            }
        //                        }
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}
        ////애니메이션 이벤트로 호출
        //public void ExcutionEnd()
        //{
        //    CC._cantInputToAnimator = false;
        //    CC.Animator.runtimeAnimatorController = _defaultAnimator;
        //    CameraManager.Get().TurnOnCamera(TypeCamera.DEFAULT);
        //}

        #endregion

        private bool _autoTarget = true; //설정에 추가될수도있어서 변수로 셋팅해놓는다.
        private Transform _autoTargetTransform;
        protected void AttackTarget()
        {
            WeaponDissolve(5f, TypeGrapHand.RIGHT);

            if (_lockOn == null)
            {
                _lockOn = GetComponent<LockOn>();
            }

            if (_lockOn == null)
            {
                Debug.LogError("Not Found LockOn Component");
                return;
            }


            var targetVector = Vector3.forward;

            //록온 타겟이 있는경우         
            if (_lockOn.CurrentTarget != null)
            {
                targetVector = _lockOn.CurrentTarget.position - transform.position;
            }
            //자동타겟일때 타겟이 없으면 잡아준다. 타겟이도중에 죽었다면 다른 타겟이있으면 타겟으로 타겟이 없으면 카메라 방향으로 간다.
            else if (_lockOn.CurrentTarget == null && CC._rotateTarget != null)
            {
                //기본으로 가메라가 보고있는 방향.
                targetVector = CC.StrafeSpeed.RotateWithCamera ? CC._rotateTarget.forward : transform.forward;

                //오토타겟이고 인풋이 없다. 
                if (_autoTarget)
                {
                    //첫타면 걍 잡아준다.
                    if (HitProperties.AttackNum == 0)
                        _autoTargetTransform = _lockOn.GetTarget();
                    else
                    {
                        //첫타가 아니라면 죽었다면 바꿔준다.
                        if (_autoTargetTransform != null && !_lockOn.IsCharacterAlive(_autoTargetTransform))
                        {
                            _autoTargetTransform = _lockOn.GetTarget();
                        }
                    }

                    targetVector = _autoTargetTransform != null ? _autoTargetTransform.position - transform.position : targetVector;
                }
            }
            else
            {
                targetVector = _lockOn.CurrentTarget.position - transform.position;
            }

            CC.RotateToDirection(targetVector.normalized, 100f);
        }
        public Vector3 GetSkillSpawnPos(TypeSkillSpawnPos type)
        {
            switch (type)
            {
                case TypeSkillSpawnPos.RIGHTHAND:
                    return _rightHandPos.position;

                case TypeSkillSpawnPos.LEFTHAND:
                    return _leftHandPos.position;

                case TypeSkillSpawnPos.HANDS:
                    var pos = new Vector3((_leftHandPos.position.x + _rightHandPos.position.x) / 2, (_leftHandPos.position.y + _rightHandPos.position.y) / 2, (_leftHandPos.position.z + _rightHandPos.position.z) / 2);
                    return pos;

                default:
                    return transform.position;
            }
        }

        internal void WeaponDissolve(float lifeTime, TypeGrapHand hand, float duration = 0.7f)
        {
            var weaponID = TableManager.GetWeaponTableData(CurrentWeaponID);

            //Weapon On.
            if (LeftWeapon != null && hand == TypeGrapHand.LEFT)
            {
                if (!LeftWeapon.gameObject.activeSelf)
                {
                    //SoundManager.Instance.Play(_materializeSoundID_1);
                    //SoundManager.Instance.Play(_materializeSoundID_2);
                    //for(int i = 0; i < weaponID.MaterializeSound.Length; i++)
                    //{
                    //    SoundManager.Instance.Play(weaponID.MaterializeSound[i]);
                    //}
                }
                EquipManager.Get().DoWeaponDissolve(LeftWeapon.gameObject, (int)TypeGrapHand.LEFT, duration, lifeTime);
            }
            if (RightWeapon != null && hand == TypeGrapHand.RIGHT)
            {
                if (!RightWeapon.gameObject.activeSelf)
                {
                    //SoundManager.Instance.Play(_materializeSoundID_1);
                    //SoundManager.Instance.Play(_materializeSoundID_2);

                    //for (int i = 0; i < weaponID.MaterializeSound.Length; i++)
                    //{
                    //    SoundManager.Instance.Play(weaponID.MaterializeSound[i]);
                    //}
                }
                EquipManager.Get().DoWeaponDissolve(RightWeapon.gameObject, (int)TypeGrapHand.RIGHT, duration, lifeTime);
            }
            CC.Animator.SetBool(AnimatorParameters.WeaponEquip, true);
            if (CurrentWeaponType == TypeWeapon.TWOHAND)
            {
                CC.RigController.ActiveIK(_leftWeaponString, RightWeapon.IKHandle);
            }
        }

        private void OnWeaponDissolve(bool visible)
        {
            //무기 장착중 해제했을때만 사운드 나도록 수정
            if (CC.Animator.GetBool(AnimatorParameters.WeaponEquip))
            {
                SoundManager.Instance.Play(_dissolveSoundID);
            }

            CC.Animator.SetBool(AnimatorParameters.WeaponEquip, visible);
            if (!visible) { CC.RigController.DeActiveIK(); }
        }
    }
}