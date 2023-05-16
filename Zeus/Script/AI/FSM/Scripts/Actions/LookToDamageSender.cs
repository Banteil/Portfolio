namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Requires a Headtrack to Look to the last damage sender", UnityEditor.MessageType.Info)]
#endif
    public class LookToDamageSender : StateAction
    {       
       public override string CategoryName
        {
            get { return "Controller/"; }
        }
        public override string DefaultName
        {
            get { return "Look To Damage Sender (Headtrack)"; }
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.IAIController.ReceivedDamage.lastSender)
            {
                fsmBehaviour.IAIController.LookToTarget(fsmBehaviour.IAIController.ReceivedDamage.lastSender);
            }
        }
    }
}