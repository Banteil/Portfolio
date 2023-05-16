namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Requires a AIMessageReceiver attached to your AI Controller - This will send a message to your Controller, so you can triggers custom Events", UnityEditor.MessageType.Info)]
#endif
    public class AISendMessage : StateAction
    {
        public override string CategoryName => "Controller/";
        public override string DefaultName => "SendMessage";

        public AISendMessage()
        {
            ExecutionType = FSMComponentExecutionType.OnStateEnter;
        }
        public string listenerName;
        public string message;
        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.MessageReceiver) fsmBehaviour.MessageReceiver.Send(listenerName, message);
        }
    }
}