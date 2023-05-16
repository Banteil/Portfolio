namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("This is a vIsInTrigger decision", UnityEditor.MessageType.Info)]
#endif
    public class IsInTrigger : StateDecision
    {
		public override string CategoryName
        {
            get { return "Trigger/"; }
        }

        public override string DefaultName
        {
            get { return "IsInTrigger"; }
        }
        [vToggleOption("Method","Compare tag","Compare name")]
        public bool UseName = false;
        public string CompareTrigger;
        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {           
            return UseName ? fsmBehaviour.IAIController.IsInTriggerWithName(CompareTrigger) : fsmBehaviour.IAIController.IsInTriggerWithTag(CompareTrigger);
        }
    }
}
