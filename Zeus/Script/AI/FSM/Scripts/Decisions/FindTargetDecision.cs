namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Find a Target and return true or false, can be used in the AnyState to find a target and make a transition to other state if a target was founded", UnityEditor.MessageType.Info)]
#endif
    public class FindTargetDecision : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "FindTarget Decision"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour.IAIController != null)
            {
                fsmBehaviour.IAIController.FindTarget();
                return fsmBehaviour.IAIController.CurrentTarget.Transform != null;
            }
            return true;
        }
    }
}