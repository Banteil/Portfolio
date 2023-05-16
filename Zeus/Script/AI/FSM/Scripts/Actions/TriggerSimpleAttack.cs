using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Trigger an Attack Animation", UnityEditor.MessageType.Info)]
#endif
    public class TriggerSimpleAttack : StateAction
    {
        public string AnimatorStateName;
        public float AttackDistance = 1f;
        public float RotateToTargetSmooth = 5;
        public AIMovementSpeed AttackSpeed = AIMovementSpeed.Walking;

        public override string CategoryName
        {
            get { return "Combat/"; }
        }

        public override string DefaultName
        {
            get { return "Trigger Generic Attack"; }
        }

        public TriggerSimpleAttack()
        {
            ExecutionType = FSMComponentExecutionType.OnStateUpdate | FSMComponentExecutionType.OnStateEnter | FSMComponentExecutionType.OnStateExit;
        }

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {

            Attack(fsmBehaviour.IAIController as IControlAICombat, executionType);
        }

        public virtual void Attack(IControlAICombat aICombat, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (executionType == FSMComponentExecutionType.OnStateEnter)
            {
                aICombat.InitAttackTime();
            }

            if (aICombat != null && aICombat.CurrentTarget.Transform)
            {
                var distance = aICombat.TargetDistance;
                if (distance <= (AttackDistance))
                {
                    if (aICombat.IsMoving) aICombat.Stop();
                    aICombat.RotateTo(aICombat.CurrentTarget.Transform.position - aICombat.transform.position);
                    Vector3 targetDirection = aICombat.CurrentTarget.Transform.position - aICombat.transform.position;
                    targetDirection.y = 0;
                    aICombat.transform.rotation = Quaternion.Lerp(aICombat.transform.rotation, Quaternion.LookRotation(targetDirection, Vector3.up), Time.deltaTime * RotateToTargetSmooth);
                    if (!aICombat.IsAttacking && aICombat.CanAttack)
                    {
                        aICombat.Animator.PlayInFixedTime(AnimatorStateName);
                        aICombat.InitAttackTime();
                    }
                }
                else
                {
                    aICombat.MoveTo(aICombat.CurrentTarget.Transform.position, AttackSpeed);
                }
            }
            if (executionType == FSMComponentExecutionType.OnStateExit)
            {
                aICombat.ResetAttackTime();
            }
        }
    }
}