namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Requires a CurrentTarget", UnityEditor.MessageType.Info)]
#endif
    public class RotateToTargetAction : StateAction
    {
        public override string CategoryName
        {
            get { return "Movement/"; }
        }
        public override string DefaultName
        {
            get { return "Rotate To Target"; }
        }
        public bool onlyIfIsInLineOfSight = true;
        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour != null && fsmBehaviour.IAIController.CurrentTarget.Transform && (!onlyIfIsInLineOfSight||fsmBehaviour.IAIController.TargetInLineOfSight))
                fsmBehaviour.IAIController.RotateTo(fsmBehaviour.IAIController.LastTargetPosition-fsmBehaviour.transform.position);
        }
    }
}