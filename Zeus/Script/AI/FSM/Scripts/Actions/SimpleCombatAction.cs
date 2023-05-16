namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Requires a ControlAICombat - Simple Combat based on this AI Controller Combat Settings", UnityEditor.MessageType.Info)]
#endif
    public class SimpleCombatAction : StateAction
    {
        public bool EngageInStrafe = false;
        public AIMovementSpeed engageSpeed = AIMovementSpeed.Running;
        public AIMovementSpeed combatSpeed = AIMovementSpeed.Walking;
        public override string CategoryName
        {
            get { return "Combat/"; }
        }
        public override string DefaultName
        {
            get { return "Melee Combat"; }
        }

        public SimpleCombatAction()
        {
            this.ExecutionType = FSMComponentExecutionType.OnStateEnter | FSMComponentExecutionType.OnStateExit | FSMComponentExecutionType.OnStateUpdate;
        }        

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.IAIController is IControlAICombat)
            {
                var combatController = (fsmBehaviour.IAIController as IControlAICombat);
                switch (executionType)
                {
                    case FSMComponentExecutionType.OnStateEnter:
                        OnEnterCombat(combatController);
                        break;

                    case FSMComponentExecutionType.OnStateExit:
                        OnExitCombat(combatController);
                        break;

                    case FSMComponentExecutionType.OnStateUpdate:
                        OnUpdateCombat(combatController);
                        break;
                }
            }   
        }

        protected virtual void OnEnterCombat(IControlAICombat controller)
        {
            controller.InitAttackTime();
            controller.IsInCombat = true;
        }

        protected virtual void OnExitCombat(IControlAICombat controller)
        {

            if (controller.CurrentTarget.Transform == null || controller.CurrentTarget.IsDead || !controller.TargetInLineOfSight) controller.ResetAttackTime();
            controller.IsInCombat = false;
        }

        protected virtual void OnUpdateCombat(IControlAICombat controller)
        {
            if (controller.CurrentTarget.Transform == null || controller.CurrentTarget.IsLost)
            {
                return;
            }

            if (controller != null)
            {
                if (controller.CanAttack)
                    EngageTarget(controller);
                else CombatMovement(controller);
                ControlLookPoint(controller);
            }
        }

        protected virtual void EngageTarget(IControlAICombat controller)
        {
            if (controller.CurrentTarget.Transform == null) return;

            if (controller.TargetDistance <= controller.AttackDistance)
            {
                controller.Stop();
                controller.Attack();
            }
            else if (!controller.AnimatorStateInfos.HasAnyTag("Attack", "LockMovement", "CustomAction"))
            {
                if (EngageInStrafe)
                    controller.StrafeMoveTo(controller.CurrentTarget.Transform.position, (controller.CurrentTarget.Transform.position - controller.transform.position).normalized, engageSpeed);
                else controller.MoveTo(controller.CurrentTarget.Transform.position, engageSpeed);
            }
            else controller.Stop();
        }

        protected virtual void CombatMovement(IControlAICombat controller)
        {            
            if (controller.StrafeCombatMovement)
                StrafeCombatMovement(controller);
            else
                SimpleCombatMovement(controller);
            if (controller.CanBlockInCombat)
            {
                controller.Blocking();
            }
        }

        protected virtual void ControlLookPoint(IControlAICombat controller)
        {
            if (controller.CurrentTarget.Transform == null || !controller.CurrentTarget.HasCollider)
                return;

            var movepoint = (controller.LastTargetPosition);
            controller.LookTo(movepoint);
        }

        protected virtual void SimpleCombatMovement(IControlAICombat controller)
        {
            bool moveForward = controller.TargetDistance > controller.CombatRange * 0.8f;
            bool moveBackWard = controller.TargetDistance < controller.MinDistanceOfTheTarget;
            var forwardMovement = (controller.CurrentTarget.Transform.position - controller.transform.position).normalized * (moveForward ? 1 + controller.StopingDistance : (moveBackWard ? -(1 + controller.StopingDistance) : 0));
            controller.StrafeMoveTo(controller.transform.position + forwardMovement, (controller.CurrentTarget.Transform.position - controller.transform.position).normalized,combatSpeed);

        }

        protected virtual void StrafeCombatMovement(IControlAICombat controller)
        {
            bool moveForward = controller.TargetDistance > controller.CombatRange * 0.8f;
            bool moveBackward = controller.TargetDistance < controller.MinDistanceOfTheTarget;
            var movepoint = (controller.LastTargetPosition);
            var forwardMovement = (movepoint - controller.transform.position).normalized * (moveForward ? 1 + controller.StopingDistance : (moveBackward ? -(1 + controller.StopingDistance) : 0));
            controller.StrafeMoveTo(controller.transform.position + (controller.transform.right * ((controller.StopingDistance + 1f)) * controller.StrafeCombatSide) + forwardMovement, (movepoint - controller.transform.position).normalized,combatSpeed);
        }        
    }
}