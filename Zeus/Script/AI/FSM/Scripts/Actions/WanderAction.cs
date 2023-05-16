using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Makes the AI Wander around randomly", UnityEditor.MessageType.Info)]
#endif
    public class WanderAction : StateAction
    {
        public bool wanderInStrafe = false;

        public override string CategoryName
        {
            get { return "Movement/"; }
        }
        public override string DefaultName
        {
            get { return "Wander"; }
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            DoWander(fsmBehaviour);
        }

        public AIMovementSpeed speed = AIMovementSpeed.Walking;

        protected virtual void DoWander(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour == null) return;
            if (fsmBehaviour.IAIController.IsDead) return;

            if (fsmBehaviour.IAIController.IsInDestination || Vector3.Distance(fsmBehaviour.IAIController.TargetDestination, fsmBehaviour.IAIController.transform.position) <= 0.5f + fsmBehaviour.IAIController.StopingDistance)
            {

                var angle = Random.Range(-90f, 90f);
                var dir = Quaternion.AngleAxis(angle, Vector3.up) * fsmBehaviour.IAIController.transform.forward;
                var movePoint = fsmBehaviour.IAIController.transform.position + dir.normalized * (Random.Range(1f, 4f) + fsmBehaviour.IAIController.StopingDistance);
                if (wanderInStrafe)
                    fsmBehaviour.IAIController.StrafeMoveTo(movePoint, dir.normalized,speed);
                else
                    fsmBehaviour.IAIController.MoveTo(movePoint,speed);
            }
        }
    }
}
