
namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Verify if your AI Controller lost the current target", UnityEditor.MessageType.Info)]
#endif
    public class TargetLost : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Lost the Target?"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour != null && fsmBehaviour.IAIController != null && fsmBehaviour.IAIController.CurrentTarget.Transform)
            {
                return fsmBehaviour.IAIController.CurrentTarget.IsLost;
            }
            return true;
        }
    }
}