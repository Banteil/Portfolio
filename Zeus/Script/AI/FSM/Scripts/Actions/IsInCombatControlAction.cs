namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("This is a Combat State Control Action", UnityEditor.MessageType.Info)]
#endif
    public class IsInCombatControlAction : StateAction
    {       
       public override string CategoryName
        {
            get { return "Controller/"; }
        }
        public override string DefaultName
        {
            get { return "IsInCombatControl"; }
        }

        public bool IsCombat;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (!(fsmBehaviour.IAIController is IControlAICombat)) return;
            (fsmBehaviour.IAIController as IControlAICombat).IsInCombat = IsCombat;
        }
    }
}