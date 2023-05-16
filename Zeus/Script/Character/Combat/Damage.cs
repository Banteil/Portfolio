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
        public float AddDamageMultiplier; //�߰� ������ ����.
    }

    [System.Serializable]
    public class Damage
    {
        //ĳ���� HP�� ���ҽ�Ű�� ��
        public float DamageValue = 0;
        //����� ���¹̳� ���Ұ�
        public float StaminaBlockCost = 0;
        //���¹̳ʰ� ȸ���ɶ� ������ �ð�
        public float StaminaRecoveryDelay = 1f;
        public float ReceiverStateDamageIncrease;
        public bool IgnoreDefense;
        public bool ActiveRagdoll;
        public float SenselessTime;

        internal Transform Sender;

        internal Transform Receiver;

        internal Vector3 HitPosition;
        internal Vector3 AttackPosition;
        //��Ʈ �� ���׼�
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

        //���ط� ���.
        public void ReduceDamage()
        {
            //Player�� ��� 0���� �����ϰ� �ٸ� npc�� ���� 0�̸� ��������. userecoil�� ��������̵� ����.
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

            //Ÿ�ٰ� �� ���̿� ��ֹ� üũ.

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