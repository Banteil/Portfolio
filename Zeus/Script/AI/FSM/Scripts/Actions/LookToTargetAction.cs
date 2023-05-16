namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Requires a HeadTrack attached to your AI Controller", UnityEditor.MessageType.Info)]
#endif
    public class LookToTargetAction : StateAction
    {
        public override string CategoryName
        {
            get { return "Controller/"; }
        }
        public override string DefaultName
        {
            get { return "Look To Target (Headtrack)"; }
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour != null && fsmBehaviour.IAIController.CurrentTarget.Transform && fsmBehaviour.IAIController.TargetInLineOfSight)
            {
                fsmBehaviour.IAIController.LookTo(fsmBehaviour.IAIController.LastTargetPosition, 3f);
            }
        }
    }

}
