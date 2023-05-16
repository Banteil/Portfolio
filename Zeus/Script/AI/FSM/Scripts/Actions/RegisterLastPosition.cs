using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("This is a vRegisterLastPosition Action", UnityEditor.MessageType.Info)]
#endif
    public class RegisterLastPosition : StateAction
    {       
       public override string CategoryName
        {
            get { return "Movement/"; }
        }
        public override string DefaultName
        {
            get { return "Set Start Position"; }
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            var ability = fsmBehaviour.IAIController.GetAbility<WaypointAbility>();
            if (ability == null) return;
            ability.SelfStartPosition = fsmBehaviour.IAIController.transform.position;
        }
    }
}