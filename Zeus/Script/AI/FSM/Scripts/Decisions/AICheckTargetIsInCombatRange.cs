namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check if the target is within the CombatRange of the AI Controller", UnityEditor.MessageType.Info)]
#endif
    public class AICheckTargetIsInCombatRange : StateDecision
    {
        public override string CategoryName
        {
            get { return "Combat/"; }
        }
        public override string DefaultName
        {
            get { return "Check Target Combat Range"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            return TargetIsInCombatRange(fsmBehaviour.IAIController as IControlAICombat);
        }

        protected virtual bool TargetIsInCombatRange(IControlAICombat ctrlCombat)
        {
            if (ctrlCombat == null) return false;
            if (ctrlCombat.CurrentTarget.Transform == null) return false;
            if (!ctrlCombat.CurrentTarget.Transform.gameObject.activeSelf) return false;
            return ctrlCombat.TargetDistance <= ctrlCombat.CombatRange;
        }
    }
}

