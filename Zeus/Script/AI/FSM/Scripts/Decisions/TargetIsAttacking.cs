using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Verify if your CurrentTarget is Attacking you", UnityEditor.MessageType.Info)]
#endif
    public class TargetIsAttacking : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Target Is Attacking?"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour.IAIController == null || !fsmBehaviour.IAIController.CurrentTarget.IsFighter) return false;
            return fsmBehaviour.IAIController.CurrentTarget.IsAttacking;
        }
    }
}