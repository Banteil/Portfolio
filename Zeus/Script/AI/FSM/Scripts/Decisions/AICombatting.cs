
namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check if the AI Controller is current in Combat mode", UnityEditor.MessageType.Info)]
#endif
    public class AICombatting : StateDecision
    {
        public override string CategoryName
        {
            get { return "Combat/"; }
        }
        public override string DefaultName
        {
            get { return "Is in Combat"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            if (!(fsmBehaviour.IAIController is IControlAICombat)) return false;
            return (fsmBehaviour.IAIController as IControlAICombat).IsInCombat;
        }
    }
}