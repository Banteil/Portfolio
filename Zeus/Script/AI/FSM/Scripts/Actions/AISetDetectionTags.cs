namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Overwrite the Detection Tags of your AI Controller", UnityEditor.MessageType.Info)]
#endif
    public class AISetDetectionTags : StateAction
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Set Detections Tags"; }
        }

        public AISetDetectionTags()
        {
            ExecutionType = FSMComponentExecutionType.OnStateEnter;
        }

        public TagMask Tags;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == FSMComponentExecutionType.OnStateEnter) fsmBehaviour.IAIController.SetDetectionTags(Tags);
        }
    }
}