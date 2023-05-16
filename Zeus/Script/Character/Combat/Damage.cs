using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public class HitBackInfo
    {
        public enum HitBackBase { HITPOINT, SENDER, SENDERFRONT, CAMERAFRONT }
        public HitBackBase Base;
        public float Speed = 1f;
        public float Duration = 0.1f;
        public AnimationCurve Curve = new AnimationCurve();
    }

    [System.Serializable]
    public struct AttackerInfo
    {
        public int AttackerID;  //Player == 0; monster == monsterTableID;
        public int WeaponID;    //monster = 0;
        public int SkillID;     //skill table id
        public float AddDamageMultiplier; //추가 데미지 정보.
    }

    [System.Serializable]
    public class Damage
    {
        //캐릭터 HP를 감소시키는 값
        public float DamageValue = 0;
        //가드시 스태미너 감소값
        public float StaminaBlockCost = 0;
        //스태미너가 회복될때 까지의 시간
        public float StaminaRecoveryDelay = 1f;
        public float ReceiverStateDamageIncrease;
        public bool IgnoreDefense;
        public bool ActiveRagdoll;
        public float SenselessTime;

        internal Transform Sender;

        internal Transform Receiver;

        internal Vector3 HitPosition;
        internal Vector3 AttackPosition;
        //히트 시 리액션
        public bool HitReaction = true;
        public string DamageType;
        public string AttackObjectName;

        internal HitProperties AttackerHitProperties { get; set; }

        public Damage()
        {
            StaminaBlockCost = 5;
            StaminaRecoveryDelay = 1;
            HitReaction = true;
        }

        public Damage(int value)
        {
            DamageValue = value;
            HitReaction = true;
        }

        public Damage(int value, bool ignoreReaction)
        {
            DamageValue = value;
            HitReaction = !ignoreReaction;
        }

        public Damage(Damage damage)
        {
            this.DamageValue = damage.DamageValue;
            this.StaminaBlockCost = damage.StaminaBlockCost;
            this.StaminaRecoveryDelay = damage.StaminaRecoveryDelay;
            this.IgnoreDefense = damage.IgnoreDefense;
            this.ActiveRagdoll = damage.ActiveRagdoll;
            this.Sender = damage.Sender;
            this.Receiver = damage.Receiver;
            this.DamageType = damage.DamageType;
            this.HitPosition = damage.HitPosition;
            this.SenselessTime = damage.SenselessTime;
            this.AttackerHitProperties = new HitProperties(damage.AttackerHitProperties);
        }

        //피해량 계산.
        public void ReduceDamage()
        {
            //Player의 경우 0으로 셋팅하고 다른 npc의 경우는 0이면 오류리턴. userecoil은 대미지없이도 움찔.
            if (AttackerHitProperties.DefaultDamageVaule == 0 && !AttackerHitProperties.UseRecoil)
            {
                var tableData = TableManager.GetWeaponTableData(AttackerHitProperties.SenderInfo.WeaponID);
                if (tableData == null)
                {
                    Debug.LogError("WeaponTableData is Null");
                    return;
                }

                AttackerHitProperties.DefaultDamageVaule = tableData.Damage;
            }

            DamageValue = AttackerHitProperties.DefaultDamageVaule * (1f + AttackerHitProperties.SenderInfo.AddDamageMultiplier);
            if (ReceiverStateDamageIncrease != 0)
            {
                DamageValue *= (1f + ReceiverStateDamageIncrease);
            }

            //Debug.Log("ReduceDamage : " + DamageValue);
        }

        internal bool WallCheck(Collider victim)
        {
            //Debug.Log($"hitOther.transform.tag : {hitOther.transform.tag}");
            var castingPostion = victim.transform.position;
            castingPostion.y += 1.8f;
            var startPosition = AttackPosition;
            startPosition.y += 1.8f;
            var dir = castingPostion - startPosition;
            var hitInfos = Physics.RaycastAll(startPosition, dir.normalized, dir.magnitude);
#if UNITY_EDITOR

            Debug.DrawLine(startPosition, castingPostion, Color.green, 4f);
#endif

            //타겟과 나 사이에 장애물 체크.

            foreach (var item in hitInfos)
            {
                if (item.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}