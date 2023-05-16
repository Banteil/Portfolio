using UnityEngine;

namespace Zeus
{
    public class IgnoreHitReaction : StateMachineBehaviour
    {
        public enum IgnoreReactionEventTriggerType
        {
            AllByNormalizedTime, EnterStateExitByNormalized, EnterByNormalizedExitState, EnterStateExitState
        }

        public IgnoreReactionEventTriggerType IgnoreHitReactionType = IgnoreReactionEventTriggerType.AllByNormalizedTime;
        public Vector2 NormalizedTime = new Vector2(0.1f, 0.8f);
       
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {            
            switch(IgnoreHitReactionType)
            {
                case IgnoreReactionEventTriggerType.EnterStateExitByNormalized:
                    NormalizedTime.x = 0f;
                    break;
                case IgnoreReactionEventTriggerType.EnterByNormalizedExitState:
                    NormalizedTime.y = 1f;
                    break;
                case IgnoreReactionEventTriggerType.EnterStateExitState:
                    NormalizedTime.x = 0f;
                    NormalizedTime.y = 1f;
                    break;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var normalizedTimeClamped = stateInfo.normalizedTime % 1;
            bool isIgnoreTime = normalizedTimeClamped >= NormalizedTime.x && normalizedTimeClamped <= NormalizedTime.y;
            AIMotor aiMotor = animator.GetComponent<AIMotor>();
            if (aiMotor != null) aiMotor.SuperArmorState = isIgnoreTime;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AIMotor aiMotor = animator.GetComponent<AIMotor>();
            if (aiMotor != null) aiMotor.SuperArmorState = false;
        }
    }
}