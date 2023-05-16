namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Verify if the AI Controller is currently moving towards a Destination", UnityEditor.MessageType.Info)]
#endif
    public class AIIsInDestination : StateDecision
    {
        public override string CategoryName
        {
            get { return "Movement/"; }
        }
        public override string DefaultName
        {
            get { return "Is In Destination"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            return fsmBehaviour.IAIController.IsInDestination;

        }
    }
}