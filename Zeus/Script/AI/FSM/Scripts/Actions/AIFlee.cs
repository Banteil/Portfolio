using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Call this action to make the AI Flee from his current target, you can set the movement speed and the distance to flee", UnityEditor.MessageType.Info)]
#endif
    public class AIFlee : StateAction
    {
        public override string CategoryName
        {
            get { return "Movement/"; }
        }
        public override string DefaultName
        {
            get { return "Flee"; }
        }

        public AIMovementSpeed fleeSpeed = AIMovementSpeed.Running;
        public float fleeDistance = 10f;
        public bool debugMode;
        public bool debugFleeDirection;
        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == FSMComponentExecutionType.OnStateUpdate)
            {
                Flee(fsmBehaviour);
            }
            else if (executionType == FSMComponentExecutionType.OnStateEnter)
            {
                Flee(fsmBehaviour);
                // force update path to be really fast for quick time, so the AI can react quickly and flee
                fsmBehaviour.IAIController.ForceUpdatePath();
            }
        }

        protected virtual void Flee(IFSMBehaviourController fsmBehaviour)
        {
            // FLEEING FROM DAMAGE SENDER
            if (fsmBehaviour != null && fsmBehaviour.IAIController.ReceivedDamage.isValid && fsmBehaviour.IAIController.ReceivedDamage.lastSender != null)
            {
                if (fsmBehaviour.IAIController.RemainingDistance < 1)
                {
                    for (int i = 1; i < 36; i++)
                    {
                        if (InTimer(fsmBehaviour, 1, "FleeTimer"))
                        {
                            if (Vector3.Distance(fsmBehaviour.IAIController.TargetDestination, fsmBehaviour.IAIController.transform.position) < fleeDistance * 0.25f + fsmBehaviour.IAIController.StopingDistance || fsmBehaviour.IAIController.IsInDestination)
                            {
                                if (debugMode) Debug.Log("Fleeing from damage sender");
                                var threatPoint = fsmBehaviour.IAIController.ReceivedDamage.lastSender.position;
                                var fleeDir = fsmBehaviour.IAIController.transform.position - threatPoint;
                                fleeDir = Quaternion.Euler(0, Random.Range(-(5 * i), 5 * i), 0) * fleeDir.normalized;
                                fleeDir.y = 0f;
                                if (debugFleeDirection) Debug.DrawRay(fsmBehaviour.IAIController.transform.position, fleeDir * fleeDistance, Color.yellow, 10f);
                        
                                fsmBehaviour.IAIController.MoveTo(fsmBehaviour.IAIController.transform.position + fleeDir * fleeDistance, fleeSpeed);
                                fsmBehaviour.IAIController.ForceUpdatePath();
                            }
                        }
                        else i--;
                    }
                }

            }
            // FLEEING FROM A TARGET
            else if (fsmBehaviour != null && fsmBehaviour.IAIController.CurrentTarget.Transform != null)
            {
                for (int i = 1; i < 36; i++)
                {
                    if (InTimer(fsmBehaviour, 1, "FleeTimer"))
                    {
                        if (Vector3.Distance(fsmBehaviour.IAIController.TargetDestination, fsmBehaviour.IAIController.transform.position) < fleeDistance * 0.25f + fsmBehaviour.IAIController.StopingDistance || fsmBehaviour.IAIController.IsInDestination)
                        {
                            if (debugMode) Debug.Log("Fleeing from a target");
                            var threatPoint = fsmBehaviour.IAIController.CurrentTarget.Transform.position;
                            var fleeDir = fsmBehaviour.IAIController.transform.position - threatPoint;
                            fleeDir = Quaternion.Euler(0, Random.Range(-(5 * i), 5 * i), 0) * fleeDir.normalized;
                            if (debugFleeDirection) Debug.DrawRay(fsmBehaviour.IAIController.transform.position, fleeDir * fleeDistance, Color.yellow, 10f);
                            fsmBehaviour.IAIController.MoveTo(fsmBehaviour.IAIController.transform.position + fleeDir * fleeDistance,fleeSpeed);
                            fsmBehaviour.IAIController.ForceUpdatePath();
                        }
                    }
                    else i--;
                }

            }
            // FLEEING WITHOUT TARGET OR DAMAGE SENDER 
            else if (fsmBehaviour != null)
            {
                for (int i = 1; i < 36; i++)
                {
                    if (InTimer(fsmBehaviour, 1, "FleeTimer"))
                    {
                        if (Vector3.Distance(fsmBehaviour.IAIController.TargetDestination, fsmBehaviour.IAIController.transform.position) < fleeDistance * 0.25f + fsmBehaviour.IAIController.StopingDistance || fsmBehaviour.IAIController.IsInDestination)
                        {
                            if (debugMode) Debug.Log("Fleeing without target or damage sender");
                            var fleeDir = fsmBehaviour.IAIController.transform.forward;
                            fleeDir = Quaternion.Euler(0, Random.Range(-(10 * i), 10 * (i)), 0) * fleeDir.normalized;
                            if (debugFleeDirection) Debug.DrawRay(fsmBehaviour.IAIController.transform.position, fleeDir * fleeDistance, Color.yellow, 10f);                   
                            fsmBehaviour.IAIController.MoveTo(fsmBehaviour.IAIController.transform.position + fleeDir * fleeDistance,fleeSpeed);
                            fsmBehaviour.IAIController.ForceUpdatePath();
                        }
                    }
                    else i--;
                }
            }
        }
    }
}