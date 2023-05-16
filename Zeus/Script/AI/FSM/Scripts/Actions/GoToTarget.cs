namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Use it to make the AI Controller chase a target * Requires a CurrentTarget *", UnityEditor.MessageType.Info)]
#endif
    public class GoToTarget : StateAction
    {
        public override string CategoryName
        {
            get { return "Movement/"; }
        }
        public override string DefaultName
        {
            get { return "Chase Target"; }
        }

        public bool useStrafeMovement = false;
        [zHideInInspector("useStrafeMovement")]
        public bool updateRotationInStrafe = false;
        public AIMovementSpeed speed = AIMovementSpeed.Walking;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.IAIController == null) return;
            if (executionType == FSMComponentExecutionType.OnStateEnter) fsmBehaviour.IAIController.ForceUpdatePath(2f);

            if (useStrafeMovement)
            {
                if (updateRotationInStrafe)
                {
                    var dir = fsmBehaviour.IAIController.TargetInLineOfSight ? fsmBehaviour.IAIController.LastTargetPosition - fsmBehaviour.transform.position : fsmBehaviour.IAIController.DesiredVelocity;
                    fsmBehaviour.IAIController.StrafeMoveTo(fsmBehaviour.IAIController.LastTargetPosition, dir, speed);
                }
                else fsmBehaviour.IAIController.StrafeMoveTo(fsmBehaviour.IAIController.LastTargetPosition,  speed);
            }
            else fsmBehaviour.IAIController.MoveTo(fsmBehaviour.IAIController.LastTargetPosition, speed);
        }
    }
}