using UnityEngine;
namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Overwrite the Detection Layer of your AI Controller", UnityEditor.MessageType.Info)]
#endif
    public class AISetDetectionLayer : StateAction
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Set Detections Layer"; }
        }

        public AISetDetectionLayer()
        {
            ExecutionType = FSMComponentExecutionType.OnStateEnter;
        }

        public LayerMask newLayer;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == FSMComponentExecutionType.OnStateEnter) fsmBehaviour.IAIController.SetDetectionLayer(newLayer);
        }
    }
}