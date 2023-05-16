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
                    Debug.LogError("ThirdPersonController�� �����ϴ�");
                }
                return _cc;
            }
        }
#if UNITY_EDITOR
        [SerializeField] private GameObject _testWeapon;
#endif

        //���� �����ߴ� ����
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

        //Ȱ ���� ���̵�
        private const int _bowDrawSoundID = 114;
        private const int _bowShootSoundID = 116;
        //���� ������ �ʿ�
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
            //���� UI���� ��� �ҷ����� ����
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

            //������������ ������̵� �༭ ���� ã�ƿ´�.
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
                //Ȱ ��ź�� �ʱ�ȭ
                _bulletCount = _maxBullet;
                PlayerUIManager.Get().GetUI<PlayerAimTypeUI>(TypePlayerUI.AIM).ReloadBullet();
            }

            //���� ���� ���⸦ �ٲ�����
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

            ///�������� �и� ���尡 �� ��� �Ʒ� �ڵ� ���

            //var soundData = TableManager.GetSoundTable().GetData((int)TypeHitMaterial.METAL);
            ////�и����� ����.
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
            //���и� �ø� ���±��� ���
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

        //�ִϸ��̼����ε� ȣ��
        public void ParryEnd()
        {
            if (!CC.CharacterState.HasFlag(TypeCharacterState.PARRY)) { return; }
            Debug.Log("�и� �ִϸ��̼� ����");
            CC.RemoveActionState(TypeCharacterState.PARRY);
            //���� �и� ����� ���� ���ܵ�
        }

        public void Gaurd()//���� ��ư ������ �� �и������� �ð��� �޾ƿ�
        {
            Debug.Log("�и� ��ǲ");
            CC.AddActionState(TypeCharacterState.GUARD);
            if (_currentParryDelay <= 0)
            {
                CC.AddActionState(TypeCharacterState.PARRY_READY);
                _currentParryDelay = ParryDelay;
                CC.OnFixedUpdate += ParryCooldown;
            }
            else
            {
                Debug.Log($"�׽�Ʈ : �и� �������� {_currentParryDelay}");
            }

            //�и� ���½ÿ� �ٽ� �и� ��ǲ�� ���� ���
            if (CC.CharacterState.HasFlag(TypeCharacterState.PARRY))
            {
                //�߰����� �۾��� ������ ! ����ؼ� �ڵ� ����ȭ
            }
            else
            {
                CC.Animator.SetBool(AnimatorParameters.IsBlock, true);
                //���� Ÿ�Կ� ���� ��/���� �տ� �ִ� ���� Ȱ��ȭ
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
            //�ǵ� ��Ʈ ����Ʈ
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
            //�и� ���ӽð� ��������
            if (_currentParryTime >= ParryDuring)
            {
                CC.RemoveActionState(TypeCharacterState.PARRY_READY);
            }

            //�и� ���� => �и� ���� ���·� ����� ���� �ִϸ��̼� ��µǵ���
            if (CC.CharacterState.HasFlag(TypeCharacterState.PARRY) && !CC.Animator.GetBool(AnimatorParameters.IsBlock))
            {
                CC.Animator.SetBool(AnimatorParameters.IsBlock, true);
                //���� Ÿ�Կ� ���� ��/���� �տ� �ִ� ���� Ȱ��ȭ
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
                Debug.Log("�и� ������ ����");
                CC.OnFixedUpdate -= ParryCooldown;
            }
        }

        public void GuardOff()
        {
            CC.RemoveActionState(TypeCharacterState.GUARD | TypeCharacterState.PARRY_READY);
            CC.Animator.SetBool(AnimatorParameters.IsBlock, false);
            //EffectsManager.Get().ReleaseEffect(_guardEffectInstantID);

            //���� Ÿ�Կ� ���� ��/���� �տ� �ִ� ���� ��Ȱ��ȭ
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
            //ī���� ������ ��
            if (CC.IsParrySuccess)
            {
                CC.Animator.SetTrigger(AnimatorParameters.CounterAttack);
                WeaponDissolve(5f, TypeGrapHand.RIGHT);
                //�и� ���� ����
                ParryEnd();
            }
            else
            {
                //������
                if (CC.Animator == null) { return; }
                CC.Animator.SetTrigger(AnimatorParameters.HeavyAttack);
                AttackTarget();
            }
        }
        #endregion

        #region Bow

        //��ǲ���� ����
        public void EqiupBow()
        {
            if (CC.Animator == null) { return; }
            CC.Animator.ResetTrigger(AnimatorParameters.LightAttack);
            CC.Animator.SetTrigger("ResetTrigger");
            CC.Animator.Play("Null", CC.FullBodyLayer);

            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null)
            {
                Debug.LogError("���� ������Ʈ�� �����ϴ�");
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

        //��ǲ���� ����
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

        //�ִϸ����Ϳ����� ����
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

        //���ػ���(RT ��� ����)
        public void DrawBow()
        {
            //ȭ�� ���� ���½ÿ� ����, �ܿ� ȭ���� ���°�� ����
            if (!_arrowReady || _bulletCount <= 0) { return; }
            CC.Animator.SetBool("HoldBow", true);

            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null) return;
            bow.Pulling();

            _bowDrawSoundInstanceID = SoundManager.Instance.Play(_bowDrawSoundID);
        }

        //�߻�(RT �� ����)
        public void ShootBow()
        {
            //ȭ�� ���� ���½ÿ� ���� �� �ִϸ��̼� ���¿� ���� ����
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

        //Ȱ �߻��(�ִϸ����Ϳ��� ����)
        public override void ShootProjectile(AttackInfo attackInfo)
        {
            if (CC.Animator == null) { return; }
            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null) { return; }
            _bulletCount -= 1;
            PlayerUIManager.Get().GetUI<PlayerAimTypeUI>(TypePlayerUI.AIM).SetBullet(_bulletCount);
            bow.Shoot();
        }

        ///Ȱ ���� ������
        ///�ִϸ��̼ǿ��� �����Ŵ
        public void DrawString()
        {
            if (CC.Animator == null) { return; }
            BowObject bow = LeftWeapon.GetComponent<BowObject>();
            if (bow == null) { return; }

            bow.DrawBowString(true);
        }
        #endregion

        #region Excution
        /// <param name="owner">ó�� ������(�÷��̾�)</param>
        /// <param name="target">ó�� �����(����)</param>
        internal override void Excution(Character player, Character target)
        {
            if (target == null) return;

            var excutionIndex = target.Excutions.FindIndex(x => x.DirectionType == TypeExcutionDirection.FRONT);
            if (excutionIndex == -1)
                return;

            // ó�� �����Ͱ� �ִٸ� ó���� �����ϹǷ� ����!
            _lockOn.StopLockOn();

            //Ÿ�� �ൿ ���� �� �÷��̾� ��ǲ ����
            CC.CantInput = true;

            WeaponDissolve(5f, TypeGrapHand.RIGHT);

            var excutionData = target.Excutions[excutionIndex];
            ExcutionManager.Get().Excute(CC, target, excutionData, () =>
            {
                CC.CantInput = false;
            });

        }

        //ó�� ����
        //public bool ExcutionCheck()
        //{
        //    //Ȯ��
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

        //            // target�� �� ������ �Ÿ��� radius ���� �۴ٸ�
        //            if (interV.magnitude <= Radius)
        //            {
        //                // 'Ÿ��-�� ����'�� '�� ���� ����'�� ����
        //                float dot = Vector3.Dot(interV.normalized, transform.forward);
        //                // �� ���� ��� ���� �����̹Ƿ� ���� ����� cos�� ���� ���ؼ� theta�� ����
        //                float theta = Mathf.Acos(dot);
        //                // angleRange�� ���ϱ� ���� degree�� ��ȯ
        //                float degree = Mathf.Rad2Deg * theta;

        //                // �þ߰� �Ǻ�
        //                if (degree <= angleRange / 2f)
        //                {
        //                    var targetChar = target.GetComponent<Character>();
        //                    if (targetChar != null)
        //                    {
        //                        var excutionHP = targetChar.MaxHealth * 20 / 100;
        //                        ///ó�� ���� => ��� ü���� �ִ� ü���� 20% ���Ϸ� ����������
        //                        /// > ������ < �� �ٲپ�� �� (�׽�Ʈ�� ���� �ٲپ�ξ���)
        //                        if (!targetChar.IsDead && targetChar.CurrentHealth <= excutionHP)
        //                        {
        //                            var tableData = TableManager.GetWeaponTableData(CurrentWeaponID);
        //                            //ó�� �ִϸ��̼��� �ִ°��
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
        ////�ִϸ��̼� �̺�Ʈ�� ȣ��
        //public void ExcutionEnd()
        //{
        //    CC._cantInputToAnimator = false;
        //    CC.Animator.runtimeAnimatorController = _defaultAnimator;
        //    CameraManager.Get().TurnOnCamera(TypeCamera.DEFAULT);
        //}

        #endregion

        private bool _autoTarget = true; //������ �߰��ɼ����־ ������ �����س��´�.
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

            //�Ͽ� Ÿ���� �ִ°��         
            if (_lockOn.CurrentTarget != null)
            {
                targetVector = _lockOn.CurrentTarget.position - transform.position;
            }
            //�ڵ�Ÿ���϶� Ÿ���� ������ ����ش�. Ÿ���̵��߿� �׾��ٸ� �ٸ� Ÿ���������� Ÿ������ Ÿ���� ������ ī�޶� �������� ����.
            else if (_lockOn.CurrentTarget == null && CC._rotateTarget != null)
            {
                //�⺻���� ���޶� �����ִ� ����.
                targetVector = CC.StrafeSpeed.RotateWithCamera ? CC._rotateTarget.forward : transform.forward;

                //����Ÿ���̰� ��ǲ�� ����. 
                if (_autoTarget)
                {
                    //ùŸ�� �� ����ش�.
                    if (HitProperties.AttackNum == 0)
                        _autoTargetTransform = _lockOn.GetTarget();
                    else
                    {
                        //ùŸ�� �ƴ϶�� �׾��ٸ� �ٲ��ش�.
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
            //���� ������ ������������ ���� ������ ����
            if (CC.Animator.GetBool(AnimatorParameters.WeaponEquip))
            {
                SoundManager.Instance.Play(_dissolveSoundID);
            }

            CC.Animator.SetBool(AnimatorParameters.WeaponEquip, visible);
            if (!visible) { CC.RigController.DeActiveIK(); }
        }
    }
}