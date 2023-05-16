using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Creates a Random Chance to go for the next state", UnityEditor.MessageType.Info)]
#endif
    public class RandomDecision : StateDecision
    {
        public override string CategoryName
        {
            get { return "Generic/"; }
        }
        public override string DefaultName
        {
            get { return "Random Decision"; }
        }

        [Range(0, 100)]
        [Tooltip("Percentage Chance between true and false")]
        public float RandomTrueFalse;
        public float Frequency;
        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            if (Frequency > 0)
            {
                if (InTimer(fsmBehaviour, Frequency))
                    return Random.Range(0, 100) < RandomTrueFalse;
                else return false;
            }

            return Random.Range(0, 100) < RandomTrueFalse;
        }
    }
}
