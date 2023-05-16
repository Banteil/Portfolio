using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Verify the current Health of your AI Controller", UnityEditor.MessageType.Info)]
    [CreateAssetMenu(fileName = "CheckHealthDecision", menuName = "Zeus/Scriptable Object/Decision/CheckHealth", order = int.MaxValue)]
#endif
    public class AICheckHealth : StateDecision
    {
        public override string CategoryName
        {
            get { return "Health/"; }
        }
        public override string DefaultName
        {
            get { return "Check Health"; }
        }

        public enum zCheckValue
        {
            EQUALS, LESS, GREATER, NOEQUALS
        }

        public zCheckValue CheckValue = zCheckValue.NOEQUALS;

        public float Value;

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            return CheckValueFuncion(fsmBehaviour);
        }

        protected virtual bool CheckValueFuncion(IFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour == null) return false;

            float healthPercentage = (fsmBehaviour.IAIController.CurrentHealth / fsmBehaviour.IAIController.MaxHealth) * 100f;

            switch (CheckValue)
            {
                case zCheckValue.EQUALS:
                    return healthPercentage == Value;
                case zCheckValue.LESS:
                    return healthPercentage < Value;
                case zCheckValue.GREATER:
                    return healthPercentage > Value;
                case zCheckValue.NOEQUALS:
                    return healthPercentage != Value;
            }

            return false;
        }
    }
}