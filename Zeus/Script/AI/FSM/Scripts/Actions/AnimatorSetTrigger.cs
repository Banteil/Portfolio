using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("This is a AnimatorSetTrigger Action", UnityEditor.MessageType.Info)]
#endif
    public class AnimatorSetTrigger : StateAction
    {       
       public override string CategoryName
        {
            get { return "Animator/"; }
        }
        public override string DefaultName
        {
            get { return "AnimatorSetTrigger"; }
        }
        public string Trigger;
        [vToggleOption("Method","Set","Reset")]
        public bool Reset;
        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
           if(Reset) fsmBehaviour.IAIController.Animator.ResetTrigger(Trigger);else fsmBehaviour.IAIController.Animator.SetTrigger(Trigger);
            //TO DO
        }
    }
}