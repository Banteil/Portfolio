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

            //원래는 AttackCondition에서 해야되지만, 아래 조건 추가시 LT1+헤비어택으로 처형 기능이 안나감
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
                //    //해당 키가 이미 버퍼에 있을 경우 리턴
                //    return;
                //}
                //else
                //{
                //    //등록한다
                //    var inputBuffer = new InputBufferControl();
                //    //공격 가능 상태일시 실행 되는 함수
                //    inputBuffer._inputBufferEvent += HeavyAttack;
                //    InputBufferDic.Add(info, inputBuffer);
                //    return;
                //}
                return;
            }

            ////공격 가능 상태에서 딕셔너리에 존재하면 삭제
            //if (InputBufferDic.ContainsKey(info))
            //{
            //    var inputBuffer = InputBufferDic[info];
            //    inputBuffer._inputBufferEvent -= HeavyAttack;
            //    InputBufferDic.Remove(info);
            //}

            if (CombatManager.CurrentWeaponType == TypeWeapon.BOW)//장비가 활일시 조작
                CombatManager.DrawBow();
            else
                CombatManager.HeavyAttack();
        }
        protected void HeavyAttackDisable()
        {
            _holdRightTrigger = false;
            //var varName = nameof(AttackCondition);
            //PropertyInfo info = this.GetType().GetProperty(varName);

            ////키를 뗐을때 인풋버퍼에 해당 인풋이 있는 경우 제거
            //if (InputBufferDic.ContainsKey(info))
            //{
            //    var inputBuffer = InputBufferDic[info];
            //    inputBuffer._inputBufferEvent -= HeavyAttack;
            //    InputBufferDic.Remove(info);
            //}

            if (CombatManager.CurrentWeaponType == TypeWeapon.BOW)//장비가 활일시 조작
                CombatManager.ShootBow();
        }

        private void GuardStateCheck()
        {
            //가드 상태시에 가드 불가상태면 해제(Ex. 가드 중 가드불가 공격 히트시
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
        //가드 해제
        protected void GuardDisable()
        {
            if (!_cc.IsBlocking)
                return;

            _holdRightShoulder = false;
            CombatManager.GuardOff();
        }
        //가드 강제 해제
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

            //아이템 번호는 추후 아잍메 인벤토리 있을시 사용
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

            //아이템 번호는 추후 아잍메 인벤토리 있을시 사용
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
                //장착중인 룬이 없는 경우 리턴
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
                Debug.LogError("스킬 정보가 없습니다");
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