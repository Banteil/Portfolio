using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Set The Damage sender as AI Target", UnityEditor.MessageType.Info)]
#endif
    public class SetDamageSenderAsTarget : StateAction
    {       
       public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Set DamageSender as Target"; }
        }
        public SetDamageSenderAsTarget()
        {
            ExecutionType = FSMComponentExecutionType.OnStateEnter;
        }
        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {

            if(fsmBehaviour.IAIController.ReceivedDamage.lastSender)
            {
                fsmBehaviour.IAIController.SetCurrentTarget(fsmBehaviour.IAIController.ReceivedDamage.lastSender);
            }          
        }
    }
}