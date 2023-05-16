using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Zeus
{
    public class CombatInput : ThirdPersonInput
    {
        #region Variable

        internal PlayerCombatManager CombatManager;

        #region Conditions

        protected bool _isAttacking;
        public bool IsAttacking
        {
            get => _isAttacking || _cc.IsAnimatorTag("Attack");
            protected set
            {
                _isAttacking = value;
            }
        }

        public bool _attackCondition;
        public bool AttackCondition
        {
            get
            {
                if ((CC.CantInput && !CC.CharacterState.HasFlag(TypeCharacterState.PARRY)) || CC.IsAnimatorTag("WeaponSwap") || (CC._cantInputExceptionMove && CombatManager.CurrentWeaponType != TypeWeapon.BOW) || CC.IsCantAttack)
                {
                    return false;
                }
                return true;
            }
        }

        private bool _guardCondition = true;
        public bool GuardCondition
        {
            get
            {
                if ((CC.CantInput && !CC.CharacterState.HasFlag(TypeCharacterState.PARRY)) || !_guardCondition || _cc.IsAnimatorTag("WeaponSwap") || _cc._cantInputExceptionMove || !_holdLeftShoulder || CombatManager.CurrentWeaponType == TypeWeapon.BOW)
                {
                    return false;
                }
                return true;
            }
            set
            {
                _guardCondition = value;
            }
        }

        private bool _skillCondition = true;
        public bool SkillCondition
        {
            get
            {
                if (CC.CantInput || !_skillCondition || _cc.IsAnimatorTag("WeaponSwap") || _cc._cantInputExceptionMove || !_holdLeftShoulder || CC.IsCantAttack)
                {
                    return false;
                }
                return true;
            }

            set
            {
                _skillCondition = value;
            }
        }

        private bool _weaponSwapCondition = true;

        public bool WeaponSwapCondition
        {
            get
            {
                if (_cc._cantInputToAnimator || _weaponSwapCondition == false || _cc._cantInputExceptionMove || _cc.IsCantAttack)
                {
                    return false;
                }
                return true;
            }
            set
            {
                _weaponSwapCondition = value;
            }
        }

        private bool _bowSwapCondition = true;

        public bool BowSwapCondition
        {
            get
            {
                if (_cc._cantInputToAnimator || !_bowSwapCondition)
                {
                    return false;
                }
                return true;
            }

            set
            {
                _bowSwapCondition = value;
            }
        }

        #endregion

        public bool IsArmed => throw new System.NotImplementedException();

        public bool IsBlocking => throw new System.NotImplementedException();

        public Transform FighterTransform => throw new System.NotImplementedException();

        public GameObject FighterGameObject => throw new System.NotImplementedException();

        public ICharacter Character => throw new System.NotImplementedException();
        #endregion
        public void OnDisableAttack()
        {
            IsAttacking = false;
        }

        public void OnEnableAttack()
        {
            if (CombatManager == null)
            {
                CombatManager = GetComponent<PlayerCombatManager>();
            }

            if (CombatManager == null)
            {
                return;
            }

            IsAttacking = true;
            _cc.IsSprinting = false;
        }
        public void ResetAttackTriggers()
        {
            _cc.Animator.ResetTrigger(AnimatorParameters.LightAttack);
        }

        protected override void Start()
        {
            base.Start();
            CombatManager = GetComponent<PlayerCombatManager>();
            InputReader.Instance.CallLightAttack += LightAttack;
            InputReader.Instance.CallHeavyAttack += HeavyAttack;
            InputReader.Instance.CallGuardEnable += GuardEnable;
            InputReader.Instance.CallGuardDiable += GuardDisable;
            InputReader.Instance.CallHeavyAttackDisable += HeavyAttackDisable;
            InputReader.Instance.CallEquipBow += EquipBow;
            InputReader.Instance.CallUnEquipBow += UnEquipBow;
            InputReader.Instance.CallItemChangeLeft += PrevWeaponSwap;
            InputReader.Instance.CallItemChangeRight += NextWeaponSwap;
            InputReader.Instance.CallWeaponSkill_1 += WeaponSkill_1;
            InputReader.Instance.CallWeaponSkill_2 += WeaponSkill_2;
            InputReader.Instance.CallRuneSkill += RuneSkill;

            //StateCheck
            CC.onAttackStateChange += AttackStateCheck;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (InputReader.HasInstance)
            {
                InputReader.Instance.CallLightAttack -= LightAttack;
                InputReader.Instance.CallHeavyAttack -= HeavyAttack;
                InputReader.Instance.CallGuardEnable -= GuardEnable;
                InputReader.Instance.CallGuardDiable -= GuardDisable;
                InputReader.Instance.CallHeavyAttackDisable -= HeavyAttackDisable;
                InputReader.Instance.CallEquipBow -= EquipBow;
                InputReader.Instance.CallUnEquipBow -= UnEquipBow;
                InputReader.Instance.CallItemChangeLeft -= PrevWeaponSwap;
                InputReader.Instance.CallItemChangeRight -= NextWeaponSwap;
                InputReader.Instance.CallWeaponSkill_1 -= WeaponSkill_1;
                InputReader.Instance.CallWeaponSkill_2 -= WeaponSkill_2;
                InputReader.Instance.CallRuneSkill -= RuneSkill;

                //StateCheck
                CC.onAttackStateChange -= AttackStateCheck;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            //CheckInputBuffer();
            GuardStateCheck();
        }

        protected void LightAttack()
        {
            if (!AttackCondition) { return; }

            //������ AttackCondition���� �ؾߵ�����, �Ʒ� ���� �߰��� LT1+���������� ó�� ����� �ȳ���
            if (_holdLeftShoulder) { return; }

            CombatManager.LightAttack();
        }

        protected void HeavyAttack()
        {
            _holdRightTrigger = true;
            //var varName = nameof(AttackCondition);
            //PropertyInfo info = this.GetType().GetProperty(varName);
            if (!AttackCondition)
            {
                //if (InputBufferDic.ContainsKey(info))
                //{
                //    //�ش� Ű�� �̹� ���ۿ� ���� ��� ����
                //    return;
                //}
                //else
                //{
                //    //����Ѵ�
                //    var inputBuffer = new InputBufferControl();
                //    //���� ���� �����Ͻ� ���� �Ǵ� �Լ�
                //    inputBuffer._inputBufferEvent += HeavyAttack;
                //    InputBufferDic.Add(info, inputBuffer);
                //    return;
                //}
                return;
            }

            ////���� ���� ���¿��� ��ųʸ��� �����ϸ� ����
            //if (InputBufferDic.ContainsKey(info))
            //{
            //    var inputBuffer = InputBufferDic[info];
            //    inputBuffer._inputBufferEvent -= HeavyAttack;
            //    InputBufferDic.Remove(info);
            //}

            if (CombatManager.CurrentWeaponType == TypeWeapon.BOW)//��� Ȱ�Ͻ� ����
                CombatManager.DrawBow();
            else
                CombatManager.HeavyAttack();
        }
        protected void HeavyAttackDisable()
        {
            _holdRightTrigger = false;
            //var varName = nameof(AttackCondition);
            //PropertyInfo info = this.GetType().GetProperty(varName);

            ////Ű�� ������ ��ǲ���ۿ� �ش� ��ǲ�� �ִ� ��� ����
            //if (InputBufferDic.ContainsKey(info))
            //{
            //    var inputBuffer = InputBufferDic[info];
            //    inputBuffer._inputBufferEvent -= HeavyAttack;
            //    InputBufferDic.Remove(info);
            //}

            if (CombatManager.CurrentWeaponType == TypeWeapon.BOW)//��� Ȱ�Ͻ� ����
                CombatManager.ShootBow();
        }

        private void GuardStateCheck()
        {
            //���� ���½ÿ� ���� �Ұ����¸� ����(Ex. ���� �� ����Ұ� ���� ��Ʈ��
            if ((!_holdLeftShoulder && _holdRightShoulder))
            {
                GuardCancel();
            }
        }

        protected void GuardEnable()
        {
            if (_cc.IsBlocking)
                return;

            _holdRightShoulder = true;
            if (!GuardCondition) { return; }
            CombatManager.Gaurd();
        }
        //���� ����
        protected void GuardDisable()
        {
            if (!_cc.IsBlocking)
                return;

            _holdRightShoulder = false;
            CombatManager.GuardOff();
        }
        //���� ���� ����
        protected void GuardCancel()
        {
            if (!_cc.IsBlocking)
                return;

            _holdRightShoulder = false;
            CombatManager.GuardOff();
        }

        protected void EquipBow()
        {
            if (!BowSwapCondition) { return; }

            var tableID = EquipManager.Get().EquipedWeaponID;
            var tableData = TableManager.GetWeaponTableData(tableID);
            var weapontType = TypeWeapon.BOW;

            if (tableData.WeaponCategory == weapontType)
                return; //weapontType = TypeWeapon.NONE;

            //������ ��ȣ�� ���� �Ɵ�� �κ��丮 ������ ���
            var itemID = TableManager.CurrentPlayerData.GetWeaponID(TypeEquipPosition.BOW);
            EquipManager.Get().Equip(TypeEquipPosition.BOW, itemID);

            CombatManager.EqiupBow();
            WeaponDissolve(0f, TypeGrapHand.LEFT);
        }

        protected void UnEquipBow()
        {
            if (!BowSwapCondition) { return; }
            CombatManager.UnEquipBow();
        }

        protected void PrevWeaponSwap()
        {
            if (!WeaponSwapCondition) { return; }
            UnEquipBow();
            var tableID = EquipManager.Get().EquipedWeaponID;
            var tableData = TableManager.GetWeaponTableData(tableID);
            var weapontType = TypeWeapon.ONEHANDANDSHEILD;

            if (tableData.WeaponCategory == weapontType)
                return; //weapontType = TypeWeapon.NONE;

            //������ ��ȣ�� ���� �Ɵ�� �κ��丮 ������ ���
            var itemID = TableManager.GetNextWeaponID(weapontType);
            EquipManager.Get().Equip(TypeEquipPosition.WEAPON, itemID);
        }

        protected void NextWeaponSwap()
        {
            if (!WeaponSwapCondition) { return; }
            UnEquipBow();
            var tableID = EquipManager.Get().EquipedWeaponID;
            var tableData = TableManager.GetWeaponTableData(tableID);
            var weapontType = TypeWeapon.TWOHAND;

            if (tableData.WeaponCategory == weapontType)
                return; // weapontType = TypeWeapon.NONE;

            var itemID = TableManager.GetNextWeaponID(weapontType);
            EquipManager.Get().Equip(TypeEquipPosition.WEAPON, itemID);
        }

        protected void WeaponSkill_1()
        {
            if (!SkillCondition) { return; }

            WeaponSkillFire(0);
        }

        protected void WeaponSkill_2()
        {
            if (!SkillCondition) { return; }

            WeaponSkillFire(1);
        }

        protected void RuneSkill()
        {
            if (!SkillCondition) { return; }

            var runeID = TableManager.CurrentPlayerData.GetEquipRuneID();

            if (runeID == 0)
            {
                //�������� ���� ���� ��� ����
                return;
            }

            var runeSkillID = TableManager.GetRuneTableData(runeID).SkillID;
            var cooltime = GameTimeManager.Instance.GetCoolTime(runeSkillID);
            if (cooltime != null)
                return;

            if (CombatManager.CurrentWeaponType == TypeWeapon.BOW)
            {
                CombatManager.UnEquipBow();
            }

            CC.Animator.SetInteger("SkillAniNum", 0);
            CC.Animator.SetTrigger(AnimatorParameters.Skill);

            if (PlayerUIManager.Get() != null)
            {
                PlayerUIManager.Get().SetCoolTime(runeSkillID, 2, 15f);
            }
        }

        private void WeaponSkillFire(int index)
        {
            var weaponId = CombatManager.CurrentWeaponType != TypeWeapon.BOW ? TableManager.CurrentPlayerData.GetWeaponID() : TableManager.CurrentPlayerData.GetWeaponID(TypeEquipPosition.BOW);
            var weaponTableData = TableManager.GetWeaponTableData(weaponId);
            var skillId = TableManager.GetWeaponSkillID(weaponId, index);
            var weaponskillData = Array.Find(weaponTableData.SkillDatas, x => x.SkillID == skillId);
            if (weaponskillData == null)
            {
                Debug.LogError("��ų ������ �����ϴ�");
                return;
            }

            var cooltime = GameTimeManager.Instance.GetCoolTime(weaponskillData.SkillID);
            if (cooltime != null)
                return;

            CC.Animator.SetInteger("SkillAniNum", weaponskillData.SkillAnimationID);
            CC.Animator.SetTrigger(AnimatorParameters.Skill);
            WeaponDissolve(5f, TypeGrapHand.RIGHT);
            if (PlayerUIManager.Get() != null)
            {
                PlayerUIManager.Get().SetCoolTime(weaponskillData.SkillID, index, 15f);
            }

            if (CombatManager.CurrentWeaponType == TypeWeapon.BOW)
            {
                CombatManager.ProjectileChange(weaponskillData.SkillID);
            }
        }

        private void WeaponDissolve(float lifeTime = 5f, TypeGrapHand hand = TypeGrapHand.RIGHT)
        {
            CombatManager.WeaponDissolve(lifeTime, hand);
        }

        private void AttackStateCheck()
        {
            if (AttackCondition && _holdRightTrigger && CombatManager.CurrentWeaponType == TypeWeapon.BOW)
            {
                HeavyAttack();
            }
        }

        private void CheckInputBuffer()
        {
            if (InputBufferDic.Count <= 0) { return; }

            foreach (var inputInfo in InputBufferDic)
            {
                if ((bool)inputInfo.Key.GetValue(this))
                {
                    inputInfo.Value.DoInput();
                    return;
                }
            }
        }
    }

    public class InputBufferControl
    {
        public delegate void InputBufferEvent();
        public event InputBufferEvent _inputBufferEvent;

        public void DoInput()
        {
            _inputBufferEvent.Invoke();
        }
    }
}