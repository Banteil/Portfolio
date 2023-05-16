using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check the distance between the AI Controller and the Current Target", UnityEditor.MessageType.Info)]
    [CreateAssetMenu(fileName = "CheckTargetDistanceDecision", menuName = "Zeus/Scriptable Object/Decision/CheckTargetDistance", order = int.MaxValue)]
#endif
    public class CheckTargetDistance : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Check Target Distance"; }
        }

        protected enum CompareValueMethod
        {
            Greater, Less, Equal, Between
        }
        [SerializeField]
        protected CompareValueMethod _compareMethod;
        public float Distance;
        public float CorrectionValue;

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            if (!fsmBehaviour.IAIController.CurrentTarget.Transform) return false;
            var radius = fsmBehaviour.gameObject.GetComponent<Character>().BlockerRadius;
            var dist = fsmBehaviour.IAIController.TargetDistance;
            return CompareDistance(dist, Distance + radius);
        }

        private bool CompareDistance(float value, float distance)
        {
            switch (_compareMethod)
            {
                case CompareValueMethod.Equal:
                    return value.Equals(distance) || (value + CorrectionValue).Equals(distance) || (value - CorrectionValue).Equals(distance);
                case CompareValueMethod.Greater:
                    return value - CorrectionValue > distance;
                case CompareValueMethod.Less:
                    return value + CorrectionValue < distance;
                case CompareValueMethod.Between:
                    return (value - CorrectionValue) < distance && distance < (value + CorrectionValue);
            }
            return false;
        }

    }
}
