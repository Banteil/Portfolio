namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Requires a ControlAIMelee", UnityEditor.MessageType.Info)]
#endif
    public class AIAttack : StateAction
    {
        public bool OverrideAttackDistance;
        [zHideInInspector("overrideAttackDistance")]
        public float AttackDistance;
        public bool OverrideAttackID;
        [zHideInInspector("overrideAttackID")]
        public int AttackID;
        public bool OverrideStrongAttack;
        [zHideInInspector("overrideStrongAttack")]
        public bool StrongAttack;
        [vHelpBox("Force attack when attack distance reached")]
        public bool ForceFirstAttack;
        [vHelpBox("Speed Movement to Attack distance")]
        public AIMovementSpeed AttackSpeed = AIMovementSpeed.Walking;

        public override string CategoryName
        {
            get { return "Combat/"; }
        }
        public override string DefaultName
        {
            get { return "Trigger MeleeAttack"; }
        }

        public AIAttack()
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
                if (ForceFirstAttack) aICombat.Attack(true);                
            }

            if (aICombat != null && aICombat.CurrentTarget.Transform)
            {
                var distance = aICombat.TargetDistance;
                if (distance <= (OverrideAttackDistance ? AttackDistance : aICombat.AttackDistance))
                {
                    aICombat.RotateTo(aICombat.CurrentTarget.Transform.position - aICombat.transform.position);
                    if (!aICombat.IsAttacking && aICombat.CanAttack)
                    {
                        aICombat.Attack();
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