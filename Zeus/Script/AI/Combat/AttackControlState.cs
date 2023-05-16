using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public enum DamageType { NORMAL, PARRY, DODGE }

    [System.Serializable]
    public class AttackInfo
    {
        [Header("Base Attack Info")]
        public int AttackNum;
        public float DamageMultiplier; //해당 모션의 추가 데미지 배율;
        public int RecoilID; //방어 모션 애니메이션 ID
        [Tooltip("Small = 0, Big = 1, Down = 2")]
        public int ReactionID; //피격 모션 애니메이션 ID
        public AttackType CombatAttackType = AttackType.Unarmed; //해당 모션의 공격 타입
        public DamageType DamageType = DamageType.NORMAL; //해당 모션의 데미지 판정 타입
        public float SenselessTime;
        [Tooltip("normalizedTime of Active Damage")]
        public float StartDamage = 0.05f;
        [Tooltip("normalizedTime of Disable Damage")]
        public float EndDamage = 0.9f;
        public HitBackInfo HitBackInfo;
        public float StopDuration;

        [Header("Attack Object Active Info")]
        public int RangeAttackIndex = -1;
        [Tooltip("기본적으로 좌,우 무기에 연결된 바디 파츠는 LeftLowerArm 또는 RightLowerArm")]
        public List<string> BodyParts = new List<string> { "RightLowerArm" };

        [Header("Attack Sign Info")]
        public bool DisplaySignDuringAttack = false;
        public float SignDuration = 0.5f;
        public HumanBodyBones SignPoint = HumanBodyBones.RightHand;
        public float SignIntensity = 8f;
    }

    public class AttackControlState : AdvancedStateMachineBehaviour
    {
        public virtual AttackInfo[] AttackInfoArray { get; }
        public bool ResetAttackTrigger;
        public bool DisplaySignBeforeAttack = false;

        protected bool _isActive;
        protected IAttackListener _mFighter;
        protected bool _isAttacking;
        protected int _currentIndex = 0;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
        }

        protected void ActiveAttackSign(Animator animator, AttackInfo currentInfo)
        {
            var zeusAIController = animator.GetComponent<ZeusAIController>();
            if (zeusAIController == null)
            {
                Debug.LogWarning("AttackSignState : ZeusMonsterControlAI Null");
                return;
            }
            else if (zeusAIController.AttackSign == null)
            {
                Debug.LogWarning("AttackSignState : AttackSign Object Null");
                return;
            }

            int soundID;
            Color uiColor;
            switch (currentInfo.DamageType)
            {
                case DamageType.NORMAL:
                    uiColor = Color.white;
                    soundID = 92;
                    break;
                case DamageType.PARRY:
                    uiColor = Color.green;
                    soundID = 93;
                    break;
                case DamageType.DODGE:
                    uiColor = Color.red;
                    soundID = 94;
                    break;
                default:
                    uiColor = Color.white;
                    soundID = 92;
                    break;
            }
            //HDR Color Intensity 수치 적용
            float factor = Mathf.Pow(2, currentInfo.SignIntensity);
            Color hdrColor = new Color(uiColor.r * factor, uiColor.g * factor, uiColor.b * factor);
            //공격 알림 객체 생성 후 세팅
            zeusAIController.AttackSign.StartDisplay(currentInfo.SignPoint, hdrColor, soundID, currentInfo.SignDuration);
        }
    }
}