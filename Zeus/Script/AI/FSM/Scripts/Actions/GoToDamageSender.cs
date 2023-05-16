namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Go to the last damage sender position", UnityEditor.MessageType.Info)]
#endif
    public class GoToDamageSender : StateAction
    {
        public override string CategoryName
        {
            get { return "Movement/"; }
        }
        public override string DefaultName
        {
            get { return "Go To Damage Sender"; }
        }

        public bool goInStrafe = false;
        public AIMovementSpeed speed = AIMovementSpeed.Walking;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.IAIController == null || fsmBehaviour.IAIController.ReceivedDamage.lastSender==null) return;
            if (executionType == FSMComponentExecutionType.OnStateEnter) fsmBehaviour.IAIController.ForceUpdatePath(2f);
        
            if (goInStrafe)
                fsmBehaviour.IAIController.StrafeMoveTo(fsmBehaviour.IAIController.ReceivedDamage.lastSender.position, fsmBehaviour.IAIController.ReceivedDamage.lastSender.position - fsmBehaviour.transform.position,speed);
            else fsmBehaviour.IAIController.MoveTo(fsmBehaviour.IAIController.ReceivedDamage.lastSender.position,speed);
        }
    }
}