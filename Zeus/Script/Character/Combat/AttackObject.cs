using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Zeus
{
    [ClassHeader("Attack Object", OpenClose = false)]
    public class AttackObject : zMonoBehaviour
    {
        public string AttackObjectName;
        public Damage DamageInfo;
        public List<HitBox> HitBoxes;
        public HitBox[] CustomHitBoxes;
        public int DamageModifier;
        public bool CanApplyDamage;
        public TypeHitMaterial MaterialType; //������ ������Ʈ�� ����.
        internal HashSet<GameObject> IgnoreGameObjects;
        [SerializeField]
        private HitProperties _hitProperties;

        [SerializeField]
        internal HitProperties HitProperties
        {
            get { return _hitProperties; }
        }

        //null�������ؾ��Ѵ�.
        public CombatManager CombatManager
        {
            get { return _combatManager; }
            set
            {
                _combatManager = value;
                DamageInfo.AttackerHitProperties = value.HitProperties;
                IgnoreInitalized();
            }
        }
        protected CombatManager _combatManager;
        public Transform IKHandle;

        /// <summary>
        /// Attack Object name is used to be selected from the <see cref="CombatManager.Members"/> list
        /// </summary>

        void Start()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            enabled = HitBoxes.Count > 0;
            for (int i = 0; i < HitBoxes.Count; i++)
            {
                HitBoxes[i].CallHit = OnHit;
            }

            foreach (var item in CustomHitBoxes)
            {
                item.CallHit = OnHit;
            }

            IgnoreInitalized();
        }

        //��� ���ִ� ��Ʈ�ڽ����� ��Ƽ�� ���¸� ��ȯ
        public virtual void SetActiveDamage(bool value, int customHitBoxIndex = -1)
        {
            CanApplyDamage = value;

            //Debug.Log($"SetActiveDamage  CombatManager.gameObject.name : {CombatManager.gameObject.name} / AttackObject Name : {gameObject.name} / CanApplyDamage : {CanApplyDamage} / openType : {openType}");

            if (value == false && IgnoreGameObjects != null && IgnoreGameObjects.Count > 0)
            {
                IgnoreGameObjects.Clear();
                IgnoreInitalized();
            }

            if (customHitBoxIndex != -1 && CustomHitBoxes.Length > 0) 
            {
                CustomHitBoxes[customHitBoxIndex].Trigger.enabled = value;
            }
            else
            {
                for (int i = 0; i < HitBoxes.Count; i++)
                {
                    var hitbox = HitBoxes[i];
                    hitbox.Trigger.enabled = value;
                }
            }
         }

        protected virtual void IgnoreInitalized()
        {
            IgnoreGameObjects ??= new();

            var owner = CombatManager != null ? CombatManager.gameObject : gameObject;
            IgnoreGameObjects.Add(owner);
        }

        public virtual void HitBoxColliderInfo(Collider col, bool _active)
        {
            SetActiveDamage(col.enabled);
        }

        /// Tony Notice
        /// ���� �ʿ�
        //ObjectHitBox���� ȣ���, �������� �ִ� �� �Լ� //hitbox�� ���⿡ �޸� collider ��, collider ohter�� ������.
        public virtual void OnHit(HitBox hitBox, Collider other)
        {
            //other ������Ʈ ������Ʈ üũ
            var hitCollider = other.GetComponentInParent<HitCollider>();
            if (hitCollider == null)
            {
                //Debug.LogError($"Not Found HitCollider other.name : {other.name} / rootname = {other.transform.root.name}");
                return;
            }

            if (hitCollider.Owner == null)
            {
                Debug.LogError("hitCollider.Owner is null  hitColliderName === " + hitCollider.name);
                return;
            }

            var inDamage = hitBox.TriggerType.HasFlag(HitBoxType.Damage);
            //var isRecoil = hitBox.TriggerType.HasFlag(HitBoxType.Recoil);

            //�������� �� �ִ� ������ ��
            if (!CanApplyDamage && inDamage)
            {
                Debug.LogError($"CanApplyDamage false DamageInfo.DamageValue : {DamageInfo.DamageValue} / AttackObject Name : {gameObject.name} / other : {other.name} / hitCollider owner : {hitCollider.Owner.name} / hitBox trigger type : {hitBox.TriggerType}");
                return;
            }

            //�浹�� �����ϴ� ������Ʈ�� ���
            if (IgnoreGameObjects.Contains(hitCollider.Owner.gameObject))
            {
                return;
            }

            //���� ������Ʈ DamageInfo �� �ʱ�ȭ

            if (DamageInfo.AttackerHitProperties == null)
            {
                DamageInfo.AttackerHitProperties = new HitProperties(HitProperties);
            }

            DamageInfo.Receiver = other.transform;

            if (DamageInfo.Sender == null)
                DamageInfo.Sender = CombatManager == null ? transform : CombatManager.transform;

            DamageInfo.AttackObjectName = AttackObjectName;
            DamageInfo.HitPosition = other.ClosestPoint(transform.position);
            DamageInfo.AttackPosition = DamageInfo.Sender.position;

            if (DamageInfo.WallCheck(other))
            {
                return;
            }

            DamageInfo.ReceiverStateDamageIncrease = hitCollider.Owner.CharacterState.HasFlag(TypeCharacterState.DAMAGE_INCREASE) ? 1f : 0;
            DamageInfo.ReduceDamage();

            if (inDamage && CombatManager != null)
            {
                CombatManager.OnHit(hitBox, hitCollider, this);
            }
            else
            {
                IgnoreGameObjects.Add(hitCollider.Owner.gameObject);
            }

            var targetCombatManager = hitCollider.Owner.GetComponent<CombatManager>();

            if (targetCombatManager != null)
            {
                targetCombatManager.OnTakeDamage(this);
            }

        }

        /// Tony Notice
        /// ���� �ʿ�
        /// ������ �ٶ� �̺�Ʈ ó��, ���� ����Ʈ�� ����Ѵ�
        public void HitAfterEvent()
        {
            if (MaterialType != TypeHitMaterial.NONE)
            {
                var collisionNormal = (transform.position - DamageInfo.AttackPosition).normalized;
                EffectsManager.Get().SetEffect((int)MaterialType, DamageInfo.AttackPosition, collisionNormal, null, 1f);
            }
        }
    }
}

namespace Zeus
{
    [System.Serializable]
    public class HitProperties
    {
        public HitProperties(HitProperties old)
        {
            HitDamageTags = old.HitDamageTags;
            UseRecoil = old.UseRecoil;
            DrawRecoilGizmos = old.DrawRecoilGizmos;
            RecoilRange = old.RecoilRange;
            HitRecolLayer = old.HitRecolLayer;
            HitMaterialType = old.HitMaterialType;
            SenderInfo = old.SenderInfo;
            AttackNum = old.AttackNum;
            HitBackInfo = old.HitBackInfo;
            StopDuration = old.StopDuration;
            DefaultDamageVaule = old.DefaultDamageVaule;
            RecoilID = old.RecoilID;
            ReactionID = old.ReactionID;
            AttackNum = old.AttackNum;
            DamageTypeID = old.DamageTypeID;
            CombatAttackType = old.CombatAttackType;
        }

        //�������� ���� ����� �±�
        public List<string> HitDamageTags = new List<string> { "Enemy" };
        //�÷��̾ ��ֹ��� �������� �ÿ� Recoil�ִϸ��̼��� ��������
        public bool UseRecoil = true;
        public bool DrawRecoilGizmos;
        [Range(0, 180f)]
        public float RecoilRange = 90f;
        public LayerMask HitRecolLayer = 1 << 0;
        public TypeHitMaterial HitMaterialType;
        internal AttackerInfo SenderInfo;
        public int AttackNum;
        public int RecoilID;
        public int ReactionID;
        public AttackType CombatAttackType;
        public DamageType DamageTypeID;

        public HitBackInfo HitBackInfo;
        public float StopDuration;
        public int DefaultDamageVaule;

        internal void SetAttackInfo(AttackInfo attackInfo)
        {
            RecoilID = attackInfo.RecoilID;
            ReactionID = attackInfo.ReactionID;
            DamageTypeID = attackInfo.DamageType;
            AttackNum = attackInfo.AttackNum;
            StopDuration = attackInfo.StopDuration;
            HitBackInfo = attackInfo.HitBackInfo;
            SenderInfo.AddDamageMultiplier = attackInfo.DamageMultiplier;
            CombatAttackType = attackInfo.CombatAttackType;
        }
    }
}