namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("This is a Avoid Action", UnityEditor.MessageType.Info)]
#endif
    public class AvoidAction : StateAction
    {       
       public override string CategoryName
        {
            get { return "Controller/"; }
        }
        public override string DefaultName
        {
            get { return "Avoid"; }
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            var ability = fsmBehaviour.IAIController.GetAbility<AvoidAbility>();
            if (ability == null) return;
            ability.Avoid();
        }
    }
}