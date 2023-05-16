namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Clear the current target of your controller", UnityEditor.MessageType.Info)]
#endif
    public class AIClearTarget : StateAction
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Clear Target"; }
        }

        public AIClearTarget()
        {
            ExecutionType = FSMComponentExecutionType.OnStateEnter;
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == FSMComponentExecutionType.OnStateEnter) fsmBehaviour.IAIController.RemoveCurrentTarget();
        }
    }
}