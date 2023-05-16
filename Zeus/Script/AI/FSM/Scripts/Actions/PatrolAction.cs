using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Requires a WaypointArea assign to the AI Controller", UnityEditor.MessageType.Info)]
#endif
    public class PatrolAction : StateAction
    {
        public bool DebugMode;

        public override string CategoryName
        {
            get { return "Movement/"; }
        }
        public override string DefaultName
        {
            get { return "Patrol"; }
        }

        public AIMovementSpeed PatrolSpeed = AIMovementSpeed.Walking;

        public bool PatrolInStrafe;
        [zHideInInspector("PatrolInStrafe")]
        public bool UpdateRotationInStrafe=true;
   
        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            DoPatrolWaypoints(fsmBehaviour);
        }

        protected virtual void DoPatrolWaypoints(IFSMBehaviourController fsmBehaviour)
        {
            var ability = fsmBehaviour.IAIController.GetAbility<WaypointAbility>();
            if (fsmBehaviour == null || ability == null) return;
            if (fsmBehaviour.IAIController.IsDead) return;

            if (ability.WaypointArea != null && ability.WaypointArea.waypoints.Count > 0)
            {
                if (ability.TargetWaypoint == null || !ability.TargetWaypoint.isValid)
                {
                    ability.NextWayPoint();
                }
                else
                {
                    if (Vector3.Distance(fsmBehaviour.IAIController.transform.position, ability.TargetWaypoint.position) <
                        fsmBehaviour.IAIController.StopingDistance + ability.TargetWaypoint.areaRadius + ability.ChangeWaypointDistance &&
                        ability.TargetWaypoint.CanEnter(fsmBehaviour.IAIController.transform) &&
                        !ability.TargetWaypoint.IsOnWay(fsmBehaviour.IAIController.transform))
                    {
                        ability.TargetWaypoint.Enter(fsmBehaviour.IAIController.transform);

                    }
                    else if (Vector3.Distance(fsmBehaviour.IAIController.transform.position, ability.TargetWaypoint.position) <
                        fsmBehaviour.IAIController.StopingDistance + ability.TargetWaypoint.areaRadius &&
                        (!ability.TargetWaypoint.CanEnter(fsmBehaviour.IAIController.transform) ||
                        !ability.TargetWaypoint.isValid))
                    {
                        ability.NextWayPoint();
                    }

                    if (ability.TargetWaypoint != null &&
                        ability.TargetWaypoint.IsOnWay(fsmBehaviour.IAIController.transform) &&
                        Vector3.Distance(fsmBehaviour.IAIController.transform.position, ability.TargetWaypoint.position) <=
                        ability.TargetWaypoint.areaRadius + ability.ChangeWaypointDistance)
                    {
                        if (fsmBehaviour.IAIController.RemainingDistance <= (fsmBehaviour.IAIController.StopingDistance + ability.ChangeWaypointDistance) || fsmBehaviour.IAIController.IsInDestination)
                        {
                            var timer = fsmBehaviour.GetTimer("Patrol");
                            if (timer >= ability.TargetWaypoint.timeToStay || !ability.TargetWaypoint.isValid)
                            {
                                ability.TargetWaypoint.Exit(fsmBehaviour.IAIController.transform);
                                ability.VisitedWaypoints.Clear();
                                ability.NextWayPoint();
                                if (DebugMode) Debug.Log("Sort new Waypoint");
                                fsmBehaviour.IAIController.Stop();
                                fsmBehaviour.SetTimer("Patrol", 0);
                            }
                            else if (timer < ability.TargetWaypoint.timeToStay)
                            {
                                if (DebugMode) Debug.Log("Stay");
                                if (ability.TargetWaypoint.rotateTo)
                                {
                                    fsmBehaviour.IAIController.Stop();
                                    fsmBehaviour.IAIController.RotateTo(ability.TargetWaypoint.transform.forward);
                                }
                                else
                                    fsmBehaviour.IAIController.Stop();

                                fsmBehaviour.SetTimer("Patrol", timer + Time.deltaTime);
                            }
                        }
                    }
                    else
                    {
                        if (PatrolInStrafe)
                        {
                            if(UpdateRotationInStrafe)  fsmBehaviour.IAIController.StrafeMoveTo(ability.TargetWaypoint.position, fsmBehaviour.IAIController.DesiredVelocity, PatrolSpeed);
                            else fsmBehaviour.IAIController.StrafeMoveTo(ability.TargetWaypoint.position,  PatrolSpeed);
                        }
                           
                        else
                            fsmBehaviour.IAIController.MoveTo(ability.TargetWaypoint.position, PatrolSpeed);
                        if (DebugMode) Debug.Log("Go to new Waypoint");
                    }
                }
            }
            else if (ability.SelfStartingPoint)
            {
                if (fsmBehaviour.DebugMode)
                    fsmBehaviour.SendDebug("MoveTo SelfStartPosition", this);
                if (PatrolInStrafe)
                {
                    if (UpdateRotationInStrafe) fsmBehaviour.IAIController.StrafeMoveTo(ability.SelfStartPosition, fsmBehaviour.IAIController.DesiredVelocity, PatrolSpeed);
                    else fsmBehaviour.IAIController.StrafeMoveTo(ability.SelfStartPosition, PatrolSpeed);
                }
                else
                    fsmBehaviour.IAIController.MoveTo(ability.SelfStartPosition, PatrolSpeed);
            }
            else if (ability.CustomStartPoint)
            {
                if (fsmBehaviour.DebugMode)
                    fsmBehaviour.SendDebug("MoveTo CustomStartPosition", this);
                if (PatrolInStrafe)
                {
                    if (UpdateRotationInStrafe) fsmBehaviour.IAIController.StrafeMoveTo(ability.CustomStartPosition, fsmBehaviour.IAIController.DesiredVelocity, PatrolSpeed);
                    else fsmBehaviour.IAIController.StrafeMoveTo(ability.CustomStartPosition, PatrolSpeed);
                }
                else
                    fsmBehaviour.IAIController.MoveTo(ability.CustomStartPosition, PatrolSpeed);
            }
            else
            {
                if (fsmBehaviour.DebugMode)
                    fsmBehaviour.SendDebug("Stop Patrolling", this);
                fsmBehaviour.IAIController.Stop();
            }
        }
    }
}