using System.Collections.Generic;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check if the AI Controller has received any Damage or a specific DamageType", UnityEditor.MessageType.Info)]
#endif
    public class AICheckDamage : StateDecision
    {
        public override string CategoryName
        {
            get { return "Health/"; }
        }
        public override string DefaultName
        {
            get { return "Check Damage Type"; }
        }

        public List<string> damageTypeToCheck;


        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            return (HasDamage(fsmBehaviour));
        }

        protected virtual bool HasDamage(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour.IAIController == null) return false;
            var hasDamage = (fsmBehaviour.IAIController.ReceivedDamage.isValid) && (damageTypeToCheck.Count == 0 || damageTypeToCheck.Contains(fsmBehaviour.IAIController.ReceivedDamage.lasType));
           
            if (fsmBehaviour.DebugMode)
            {
                fsmBehaviour.SendDebug(Name + " " + (fsmBehaviour.IAIController.ReceivedDamage.isValid) + " " + fsmBehaviour.IAIController.ReceivedDamage.lastSender, this);
            }

            return hasDamage;
        }
    }
}
