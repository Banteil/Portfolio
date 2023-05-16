using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check what FSM State is running", UnityEditor.MessageType.Info)]
#endif
    public class AICheckState : StateDecision
    {
        public override string CategoryName
        {
            get { return "Behaviour/"; }
        }        
        
        public override string DefaultName
        {
            get { return "Check FSM State"; }
        }

        [SerializeField, HideInInspector] protected int _stateIndex;

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            return fsmBehaviour.IndexOffCurrentState == _stateIndex + 2;
        }
    }
}