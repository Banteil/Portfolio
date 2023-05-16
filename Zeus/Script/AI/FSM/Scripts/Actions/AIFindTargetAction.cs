
namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Call this Action to Find a Target based on your AI Controller Detection Settings, make sure your target has a HealthController", UnityEditor.MessageType.Info)]
#endif
    public class AIFindTargetAction : StateAction
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Find Target"; }
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            FindTarget(fsmBehaviour);
        }

        public bool checkForObstacles = true;

        protected virtual void FindTarget(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour != null)
            {
                fsmBehaviour.IAIController.FindTarget(checkForObstacles);
            }
        }
    }
}
