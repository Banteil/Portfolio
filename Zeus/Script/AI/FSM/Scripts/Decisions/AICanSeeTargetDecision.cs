
namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Verify if you can see the target based on the Detection Settings of the AI Controller", UnityEditor.MessageType.Info)]
#endif
    public class AICanSeeTargetDecision : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Can See Target"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            var cansee = CanSeeTarget(fsmBehaviour);
            return cansee;
        }

        protected virtual bool CanSeeTarget(IFSMBehaviourController fsmBehaviour)
        {
            return fsmBehaviour.IAIController.TargetInLineOfSight;
        }
    }
}