
namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check if the AI Controller is current in Combat mode", UnityEditor.MessageType.Info)]
#endif
    public class CheckAbilityProcess : StateDecision
    {
        public override string CategoryName => "Ability/";

        public override string DefaultName => "Check Ability Process";

        public string AbilityName;

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            var ability = fsmBehaviour.ZeusAIController.GetAbility(AbilityName);
            if (ability == null) return false;
            return ability.IsProcessing;
        }
    }
}