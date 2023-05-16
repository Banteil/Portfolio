using System.Collections.Generic;

namespace Zeus
{
    public class FSMComponent
    {

        public List<StateAction> ActionsEnter;
        public List<StateAction> ActionsExit;
        public List<StateAction> ActionsUpdate;

        public FSMComponent(List<StateAction> actions)
        {
            ActionsEnter = actions.FindAll(act => act && (act.ExecutionType & FSMComponentExecutionType.OnStateEnter) != 0);
            ActionsExit = actions.FindAll(act => act && (act.ExecutionType & FSMComponentExecutionType.OnStateExit) != 0);
            ActionsUpdate = actions.FindAll(act => act && (act.ExecutionType & FSMComponentExecutionType.OnStateUpdate) != 0);
        }

        public void DoActions(IFSMBehaviourController fsmBehaviour, FSMComponentExecutionType executionType)
        {

            switch (executionType)
            {
                case FSMComponentExecutionType.OnStateEnter:
                    for (int i = 0; i < ActionsEnter.Count; i++)
                    {
                        ActionsEnter[i].DoAction(fsmBehaviour, FSMComponentExecutionType.OnStateEnter);
                    }
                    break;
                case FSMComponentExecutionType.OnStateExit:
                    for (int i = 0; i < ActionsExit.Count; i++)
                    {
                        ActionsExit[i].DoAction(fsmBehaviour, FSMComponentExecutionType.OnStateExit);
                    }
                    break;
                case FSMComponentExecutionType.OnStateUpdate:

                    for (int i = 0; i < ActionsUpdate.Count; i++)
                    {
                        ActionsUpdate[i].DoAction(fsmBehaviour, FSMComponentExecutionType.OnStateUpdate);
                    }
                    break;
            }
        }
    }
}