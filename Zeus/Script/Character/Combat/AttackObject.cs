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
        public TypeHitMaterial MaterialType; //때리는 오브젝트의 제질.
        internal HashSet<GameObject> IgnoreGameObjects;
        [SerializeField]
        private HitProperties _hitProperties;

        [SerializeField]
        internal HitProperties HitProperties
        {
            get { return _hitProperties; }
        }

        //null을상정해야한다.
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

        //등록 되있는 히트박스들의 액티브 상태를 전환
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
        /// 수정 필요
        //ObjectHitBox에서 호출됨, 데미지를 주는 쪽 함수 //hitbox는 무기에 달린 collider 고, collider ohter은 맞은놈.
        public virtual void OnHit(HitBox hitBox, Collider other)
        {
            //other 오브젝트 컴포넌트 체크
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

            //데미지를 못 주는 상태일 시
            if (!CanApplyDamage && inDamage)
            {
                Debug.LogError($"CanApplyDamage false DamageInfo.DamageValue : {DamageInfo.DamageValue} / AttackObject Name : {gameObject.name} / other : {other.name} / hitCollider owner : {hitCollider.Owner.name} / hitBox trigger type : {hitBox.TriggerType}");
                return;
            }

            //충돌을 무시하는 오브젝트인 경우
            if (IgnoreGameObjects.Contains(hitCollider.Owner.gameObject))
            {
                return;
            }

            //어택 오브젝트 DamageInfo 값 초기화

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
        /// 수정 필요
        /// 데미지 줄때 이벤트 처리, 무기 이펙트를 출력한다
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

        //데미지를 받을 대상의 태그
        public List<string> HitDamageTags = new List<string> { "Enemy" };
        //플레이어가 장애물을 공격했을 시에 Recoil애니메이션이 나오는지
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