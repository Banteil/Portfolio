
namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Requires a HeadTrack attached to your AI Controller", UnityEditor.MessageType.Info)]
#endif
    public class LookAroundAction : StateAction
    {
        public override string CategoryName
        {
            get { return "Controller/"; }
        }
        public override string DefaultName
        {
            get { return "Look Around (Headtrack)"; }
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            fsmBehaviour.IAIController.LookAround();
        }
    }
}