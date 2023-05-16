using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("This will overwrite the current Tag, Layer and MaxDistanceToDetect of your controller", UnityEditor.MessageType.Info)]
#endif
    public class FindSpecificTarget : StateAction
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Find Specific Target"; }
        }

        public LayerMask DetectLayer;
        public TagMask DetectTags;
        public bool CheckForObstacles = true;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            FindTarget(fsmBehaviour.IAIController);
        }

        public virtual void FindTarget(IControlAI vIControl)
        {
            vIControl.FindSpecificTarget(DetectTags, DetectLayer, CheckForObstacles);
        }
    }
}