using UnityEngine;
namespace Zeus
{
    public class AttackSignState : StateMachineBehaviour
    {
        public float Duration = 0.5f;
        public HumanBodyBones SignPoint = HumanBodyBones.Chest;
        [SerializeField] DamageType _damageType;
        [SerializeField] float _intensity = 8f;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
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

            var state = Utilities.GetBehaviour<AttackControlState>(animator, stateInfo);
            if (state != null)
            {
                var info = state.AttackInfoArray[0];                
                SignPoint = info.SignPoint;
                Duration = info.SignDuration;
                _damageType = info.DamageType;
                _intensity = info.SignIntensity;
            }

            int soundID;
            Color uiColor;
            switch (_damageType)
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
            float factor = Mathf.Pow(2, _intensity);
            Color hdrColor = new Color(uiColor.r * factor, uiColor.g * factor, uiColor.b * factor);
            //공격 알림 객체 생성 후 세팅
            zeusAIController.AttackSign.StartDisplay(SignPoint, hdrColor, soundID, Duration);
        }
    }
}