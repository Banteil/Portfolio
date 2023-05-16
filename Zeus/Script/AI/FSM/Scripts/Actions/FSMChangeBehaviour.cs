namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Use it to change the current FSM Behavior of your Controller", UnityEditor.MessageType.Info)]
#endif
    public class FSMChangeBehaviour : StateAction
    {
        public override string CategoryName
        {
            get { return "Controller/"; }
        }
        public override string DefaultName
        {
            get { return "Change FSM Behaviour"; }
        }

        public FSMBehaviour NewBehaviour;
        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            fsmBehaviour.ChangeBehaviour(NewBehaviour);
        }
    }
}