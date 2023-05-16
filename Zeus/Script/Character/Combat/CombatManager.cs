using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class CombatManager : zMonoBehaviour
    {
        #region SeralizedProperties in CustomEditor
        public List<BodyMember> Members = new List<BodyMember>();

        public TypeHitMaterial HitMaterial; //���� hitcollder ��.
        public AttackObject LeftWeapon;
        public AttackObject RightWeapon;
        #endregion

        [Tooltip("NPC ONLY- Ideal distance for the attack")]
        public float DefaultAttackDistance = 1f;
        [Tooltip("Default cost for stamina when attack")]
        public float DefaultStaminaCost = 20f;
        [Tooltip("Default recovery delay for stamina when attack")]
        public float DefaultStaminaRecoveryDelay = 1f;
        [Range(0, 100)]
        public int DefaultDefenseRate = 100;
        [Range(0, 180)]
        public float DefaultDefenseRange = 90;

        [HideInInspector]
        public Character Character;
        public HitProperties HitProperties; //AttackObject�� ī�ǿ����� ���� ���ⱳü�� ���ϸ��̼ǿ����� ���� ����ȴ�. �ܺο��� �����ص� �ǹ̰� ����.
        internal virtual ZoneInfo ZoneInfo { get; set; } //�⺻ null.

        protected virtual void Start()
        {
            BodyInitialized();
            WeaponInitialized();
        }

        public virtual void BodyInitialized()
        {
            var animationEventReceiver = gameObject.ComponentAdd<AnimationEventReceiver>();
            animationEventReceiver.CombatManager = this;

            Character = gameObject.GetComponent<Character>();
            foreach (BodyMember member in Members)
            {
                if (member.HitCollider == null)
                {
                    var hitColliders = GetComponentsInChildren<HitCollider>();
                    if (hitColliders.Length > 0)
                        member.HitCollider = Array.Find(hitColliders, a => a.HitColliderName.Equals(member.BodyPart));

                    if (member.HitCollider == null)
                        continue;

                    member.TypeEquipPosition = ConvertToEquipPosition(member.BodyPart.ToEnum<HumanBodyBones>());
                    member.HitCollider.HitMaterial = HitMaterial;
                }
            }

            foreach (BodyMember member in Members)
            {
                if (member.AttackObject == null)
                {
                    var attackObjects = GetComponentsInChildren<AttackObject>();
                    if (attackObjects.Length > 0)
                        member.AttackObject = Array.Find(attackObjects, a => a.AttackObjectName.Equals(member.BodyPart));

                    if (member.AttackObject == null)
                        continue;

                    member.AttackObject.CombatManager = this;
                }
            }
        }

        private void WeaponInitialized()
        {
            foreach (BodyMember member in Members)
            {
                if (member.HitCollider == null)
                    continue;

                if (member.BodyPart == HumanBodyBones.LeftLowerArm.ToString())
                {
                    var weapon = member.HitCollider.GetComponentInChildren<AttackObject>(true);
                    LeftWeapon = weapon;
                }
                if (member.BodyPart == HumanBodyBones.RightLowerArm.ToString())
                {
                    var weapon = member.HitCollider.GetComponentInChildren<AttackObject>(true);
                    RightWeapon = weapon;
                }
            }

            if (LeftWeapon != null)
            {
                LeftWeapon.CombatManager = this;
                LeftWeapon.SetActiveDamage(false);
            }
            if (RightWeapon != null)
            {
                RightWeapon.CombatManager = this;
                RightWeapon.SetActiveDamage(false);
            }
        }

        private TypeEquipPosition ConvertToEquipPosition(HumanBodyBones bones)
        {
            var result = TypeEquipPosition.HAND;
            switch (bones)
            {
                case HumanBodyBones.Hips:
                    {
                        result = TypeEquipPosition.BODY;
                    }
                    break;
                case HumanBodyBones.LeftUpperLeg:
                case HumanBodyBones.RightUpperLeg:
                case HumanBodyBones.LeftLowerLeg:
                case HumanBodyBones.RightLowerLeg:
                case HumanBodyBones.LeftFoot:
                case HumanBodyBones.RightFoot:
                    {
                        result = TypeEquipPosition.FOOT;
                    }
                    break;
                case HumanBodyBones.Spine:
                    break;
                case HumanBodyBones.Chest:
                    break;
                case HumanBodyBones.UpperChest:
                    break;
                case HumanBodyBones.Neck:
                    break;
                case HumanBodyBones.Head:
                    break;
                case HumanBodyBones.LeftShoulder:
                    break;
                case HumanBodyBones.RightShoulder:
                    break;
                case HumanBodyBones.LeftUpperArm:
                    break;
                case HumanBodyBones.RightUpperArm:
                    break;
                case HumanBodyBones.LeftLowerArm:
                    break;
                case HumanBodyBones.RightLowerArm:
                    break;
                case HumanBodyBones.LeftHand:
                    break;
                case HumanBodyBones.RightHand:
                    break;
                case HumanBodyBones.LeftToes:
                    break;
                case HumanBodyBones.RightToes:
                    break;
                case HumanBodyBones.LeftEye:
                    break;
                case HumanBodyBones.RightEye:
                    break;
                case HumanBodyBones.Jaw:
                    break;
                case HumanBodyBones.LeftThumbProximal:
                    break;
                case HumanBodyBones.LeftThumbIntermediate:
                    break;
                case HumanBodyBones.LeftThumbDistal:
                    break;
                case HumanBodyBones.LeftIndexProximal:
                    break;
                case HumanBodyBones.LeftIndexIntermediate:
                    break;
                case HumanBodyBones.LeftIndexDistal:
                    break;
                case HumanBodyBones.LeftMiddleProximal:
                    break;
                case HumanBodyBones.LeftMiddleIntermediate:
                    break;
                case HumanBodyBones.LeftMiddleDistal:
                    break;
                case HumanBodyBones.LeftRingProximal:
                    break;
                case HumanBodyBones.LeftRingIntermediate:
                    break;
                case HumanBodyBones.LeftRingDistal:
                    break;
                case HumanBodyBones.LeftLittleProximal:
                    break;
                case HumanBodyBones.LeftLittleIntermediate:
                    break;
                case HumanBodyBones.LeftLittleDistal:
                    break;
                case HumanBodyBones.RightThumbProximal:
                    break;
                case HumanBodyBones.RightThumbIntermediate:
                    break;
                case HumanBodyBones.RightThumbDistal:
                    break;
                case HumanBodyBones.RightIndexProximal:
                    break;
                case HumanBodyBones.RightIndexIntermediate:
                    break;
                case HumanBodyBones.RightIndexDistal:
                    break;
                case HumanBodyBones.RightMiddleProximal:
                    break;
                case HumanBodyBones.RightMiddleIntermediate:
                    break;
                case HumanBodyBones.RightMiddleDistal:
                    break;
                case HumanBodyBones.RightRingProximal:
                    break;
                case HumanBodyBones.RightRingIntermediate:
                    break;
                case HumanBodyBones.RightRingDistal:
                    break;
                case HumanBodyBones.RightLittleProximal:
                    break;
                case HumanBodyBones.RightLittleIntermediate:
                    break;
                case HumanBodyBones.RightLittleDistal:
                    break;
                case HumanBodyBones.LastBone:
                    break;
                default:
                    {
                        result = TypeEquipPosition.NONE;
                    }
                    break;
            }

            return result;
        }

        public virtual void ShootProjectile(AttackInfo attackInfo)
        {
            HitProperties.SetAttackInfo(attackInfo);
        }

        internal void SetActiveAttack(AttackInfo attackInfo, bool active = true)
        {
            HitProperties.SetAttackInfo(attackInfo);

            for (int i = 0; i < attackInfo.BodyParts.Count; i++)
            {
                var bodyPart = attackInfo.BodyParts[i];
                SetActiveAttack(bodyPart, attackInfo, active);
            }
        }

        protected void SetActiveAttack(string bodyPart, AttackInfo attackInfo, bool active = true)
        {
            if (attackInfo.CombatAttackType == AttackType.Unarmed)
            {
                /// find attackObject by bodyPart
                var bodyMember = Members.Find(member => member.BodyPart == bodyPart);
                if (bodyMember != null)
                {
                    var attackObject = bodyMember.Transform.GetComponent<AttackObject>();
                    if (attackObject != null)
                    {
                        attackObject.CombatManager = this;
                        attackObject.SetActiveDamage(active, attackInfo.RangeAttackIndex);
                    }
                }
            }
            else if (attackInfo.CombatAttackType == AttackType.MeleeWeapon)
            {
                ///if AttackType == MeleeWeapon
                ///this work just for Right and Left Lower Arm         
                if (bodyPart == "RightLowerArm" && RightWeapon != null)
                {
                    RightWeapon.CombatManager = this;
                    RightWeapon.SetActiveDamage(active, attackInfo.RangeAttackIndex);
                }
                else if (bodyPart == "LeftLowerArm" && LeftWeapon != null)
                {
                    LeftWeapon.CombatManager = this;
                    LeftWeapon.SetActiveDamage(active, attackInfo.RangeAttackIndex);
                }
            }
        }

        //���� ��Ʈ �� ó��, AttackObect���� ȣ���
        public void OnHit(HitBox hitBox, HitCollider other, AttackObject attackObject)
        {
            #region Parry
            attackObject.IgnoreGameObjects.Add(other.Owner.gameObject);
            if (other.Owner.CharacterState.HasFlag(TypeCharacterState.PARRY_READY) && attackObject.DamageInfo.AttackerHitProperties.DamageTypeID == DamageType.PARRY)
            {
                ///�и� �� ���ο� ������Ÿ���� ����
                ///��Ʈ �� ���׼� �� �˹��� �������� ó���Ͽ� �ش� ����� ���
                Damage parry = new Damage();
                parry.AttackerHitProperties = new HitProperties(attackObject.DamageInfo.AttackerHitProperties);
                parry.HitPosition = other.transform.position;
                parry.Sender = other.gameObject.transform;
                parry.AttackerHitProperties.UseRecoil = true;
                parry.AttackerHitProperties.ReactionID = 1;
                Character.TriggerDamageReaction(parry);
                return;
            }
            #endregion
            //ó���� �ߵ��Ǹ� ���̻� �������� �ʾƵ� ���� �״´�.
            if (PalyeExcution(other.Owner, attackObject.DamageInfo))
                return;

            //��Ʈ ���� �� ȣ��Ǵ� ������ �̺�Ʈ(�ش� ������ ��ƼŬ ��)
            attackObject.HitAfterEvent();
            //��Ʈ�� ���� �� ȣ��Ǵ� �̺�Ʈ(����, �� �ǰ� ����)
            OnDamageHit(attackObject.DamageInfo);
        }

        //������ ���� ĳ������ ó��
        public void OnTakeDamage(AttackObject attackObject)
        {
            //������ ó�� �� ���� ���� ���¸�
            if (Character.CharacterState.HasFlag(TypeCharacterState.GUARD) && attackObject.DamageInfo.AttackerHitProperties.DamageTypeID == DamageType.NORMAL)
            {
                attackObject.IgnoreGameObjects.Add(Character.gameObject);
                Character.GuardSuccess(attackObject.DamageInfo);
                return;
            }

            //������ ó�� �� �и� ���� ���¸�
            if (Character.CharacterState.HasFlag(TypeCharacterState.PARRY_READY) && attackObject.DamageInfo.AttackerHitProperties.DamageTypeID == DamageType.PARRY)
            {
                attackObject.IgnoreGameObjects.Add(Character.gameObject);
                Character.ParrySuccess(attackObject.DamageInfo);
                return;
            }
            Character.gameObject.ApplyDamage(attackObject.DamageInfo);
        }

        private bool PalyeExcution(Character victim, Damage DamageInfo)
        {
            var excution = UnityEngine.Random.Range(0, 3) > 1;
            if (!excution || victim == null)
                return false;

            if (victim.TypeCharacter != TypeCharacter.PLAYERBLE)
            {
                var tableData = TableManager.GetWeaponTableData(EquipManager.Get().EquipedWeaponID);
                var excutionIndex = victim.Excutions.FindIndex(x => x.DirectionType == TypeExcutionDirection.FRONT);
                if (excutionIndex != -1 && tableData.ExcutionID != 0 && DamageInfo.DamageValue >= victim.CurrentHealth)
                {
                    Excution(null, victim);
                    return true;
                }
            }

            return false;
        }

        //������ ���� �� �̺�Ʈ ó��
        public virtual void OnDamageHit(Damage damageinfo)
        {
            if (AnimatorSpeedManager.Get() != null)
            {
                AnimatorSpeedManager.Get().SetAnimatorSpeed(Character.Animator, 0.1f, 0.15f);
            }
        }

        internal virtual void Excution(Character owner, Character target) { }
    }

    #region Secundary Classes
    [Serializable]
    public class BodyMember
    {
        public Transform Transform;
        public string BodyPart;
        public TypeEquipPosition TypeEquipPosition;
        public HitCollider HitCollider;
        public AttackObject AttackObject;
        public bool IsHuman;

        public void SetActiveHit(bool active)
        {
            HitCollider?.ActiveHitCollider(active);
        }

        public void SetActiveDamage(bool active)
        {
            AttackObject?.SetActiveDamage(active);
        }
    }

    public enum HumanBones
    {
        RightHand, RightLowerArm, RightUpperArm, RightShoulder,
        LeftHand, LeftLowerArm, LeftUpperArm, LeftShoulder,
        RightFoot, RightLowerLeg, RightUpperLeg,
        LeftFoot, LeftLowerLeg, LeftUpperLeg,
        Chest, Head
    }

    public enum AttackType
    {
        Unarmed, MeleeWeapon, Projectile
    }
    #endregion
}