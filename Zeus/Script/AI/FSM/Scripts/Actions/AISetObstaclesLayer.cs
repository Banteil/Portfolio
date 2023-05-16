using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Overwrite the Detection Obstacle Layer of your AI Controller", UnityEditor.MessageType.Info)]
#endif
    public class AISetObstaclesLayer : StateAction
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Set Obstacles Layer"; }
        }

        public AISetObstaclesLayer()
        {
            ExecutionType = FSMComponentExecutionType.OnStateEnter;
        }

        public LayerMask NewLayer;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == FSMComponentExecutionType.OnStateEnter) fsmBehaviour.IAIController.SetObstaclesLayer(NewLayer);
        }
    }
}