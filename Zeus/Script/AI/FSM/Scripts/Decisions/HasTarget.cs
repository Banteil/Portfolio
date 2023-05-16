namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Return true or false if the AI Controller has a CurrentTarget or not", UnityEditor.MessageType.Info)]
#endif
    public class HasTarget : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Has a CurrentTarget?"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour.IAIController == null)
                return false;
            return fsmBehaviour.IAIController.CurrentTarget.Transform != null;
        }
    }
}