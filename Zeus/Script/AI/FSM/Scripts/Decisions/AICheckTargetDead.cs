using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check if the current target with a HealthController is Dead", UnityEditor.MessageType.Info)]
    [CreateAssetMenu(fileName = "TargetIsDeadDecision", menuName = "Zeus/Scriptable Object/Decision/TargetIsDead", order = int.MaxValue)]
#endif
    public class AICheckTargetDead : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Check if Target is Dead"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            return TargetIsDead(fsmBehaviour);
        }

        protected virtual bool TargetIsDead(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour == null) return true;
            Transform target = fsmBehaviour.IAIController.CurrentTarget;
            if (target == null) return true;            
            return fsmBehaviour.IAIController.CurrentTarget.IsDead;
        }
    }
}