namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Overwrite the Line of Sight of your AI Controller", UnityEditor.MessageType.Info)]
#endif
    public class AISetLineOfSight : StateAction
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Set Line Of Sight"; }
        }

        public AISetLineOfSight()
        {
            ExecutionType = FSMComponentExecutionType.OnStateEnter;
            FieldOfView = -1;
            MinDistanceToDetect = -1;
            MaxDistanceToDetect = -1f;
            LostTargetDistance = -1f;
        }

        [vHelpBox("If you don't want to overwrite a value leave it to -1")]
        public float FieldOfView;
        public float MinDistanceToDetect;
        public float MaxDistanceToDetect;
        public float LostTargetDistance;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == FSMComponentExecutionType.OnStateEnter) fsmBehaviour.IAIController.SetLineOfSight(FieldOfView, MinDistanceToDetect, MaxDistanceToDetect, LostTargetDistance);
        }
    }
}