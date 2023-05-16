namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check Ability Presence", UnityEditor.MessageType.Info)]
#endif
    public class CheckAbilityPresence : StateDecision
    {
        public override string CategoryName => "Ability/";

        public override string DefaultName => "Check Character Ability Presence";

        public string AbilityName;

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            var ability = fsmBehaviour.ZeusAIController.GetAbility(AbilityName);
            var result = ability != null;
            if (result && !ability.enabled) return false;
            return result;
        }
    }
}
