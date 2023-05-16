namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check if the AI Controller received a specific amount of Damage or Hits", UnityEditor.MessageType.Info)]
#endif
    public class AICheckMassiveDamage : StateDecision
    {
        public override string CategoryName
        {
            get { return "Health/"; }
        }
        public override string DefaultName
        {
            get { return "Check Damage Amount"; }
        }

        [vToggleOption("Compare Value", "Less", "Greater or Equals")]
        public bool greater = false;        
        [vToggleOption("Compare Type", "Total Hits", "Total Damage")]
        public bool massiveValue = true;
        public int valueToCompare;
       
        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            return (HasDamage(fsmBehaviour));
        }

        protected virtual bool HasDamage(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour.IAIController.ReceivedDamage == null) return false;
            var value = massiveValue ? fsmBehaviour.IAIController.ReceivedDamage.massiveValue : fsmBehaviour.IAIController.ReceivedDamage.massiveCount;
            return (greater ? (value >= valueToCompare) : (value < valueToCompare));
        }
    }
}
