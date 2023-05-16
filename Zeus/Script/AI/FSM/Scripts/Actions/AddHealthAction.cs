using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Add health to the current HealthController", UnityEditor.MessageType.Info)]
#endif
    public class AddHealthAction : StateAction
    {
        public bool recoverWhenIsDead = false;

        public override string CategoryName
        {
            get { return "Controller/"; }
        }

        public override string DefaultName
        {
            get { return "Add Health"; }
        }

        [Header("This action won't work with the DecisionTimer")]
        public float timeToAdd = 1f;
        public int healthToRecovery = 1;

        public override void DoAction(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType = FSMComponentExecutionType.OnStateUpdate)
        {
            if (fsmBehaviour.IAIController.IsDead && !recoverWhenIsDead) return;
            AddHealth(fsmBehaviour);
        }

        protected virtual void AddHealth(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour == null) return;

            if (InTimer(fsmBehaviour, timeToAdd))
            {
                fsmBehaviour.IAIController.ChangeHealth(healthToRecovery);
            }
        }
    }
}