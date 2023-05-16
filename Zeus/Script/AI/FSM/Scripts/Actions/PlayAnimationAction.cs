using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("This is a vPlayAnimationAction Action", UnityEditor.MessageType.Info)]
#endif
    public class PlayAnimationAction : StateAction
    {       
       public override string CategoryName
        {
            get { return "Animator/"; }
        }
        public override string DefaultName
        {
            get { return "Play Animation"; }
        }

        public string _animationState;
        public int _layer;
        public float crossfade=0.2f;
        public PlayAnimationAction()
        {
            ExecutionType = FSMComponentExecutionType.OnStateEnter;
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            fsmBehaviour.IAIController.Animator.CrossFadeInFixedTime(_animationState,crossfade, _layer);
        }
    }
}