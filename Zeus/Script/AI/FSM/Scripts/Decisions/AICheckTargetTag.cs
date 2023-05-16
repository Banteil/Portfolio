namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Verify the CurrentTarget Tag", UnityEditor.MessageType.Info)]
#endif
    public class AICheckTargetTag : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Check Target Tag"; }
        }

        public TagMask TargetTags;
        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour.IAIController.CurrentTarget.Transform != null)
                return TargetTags.Contains(fsmBehaviour.IAIController.CurrentTarget.Transform.gameObject.tag);
            return true;
        }
    }
}
