using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("This is a vHasWayPointArea decision", UnityEditor.MessageType.Info)]
#endif
    public class HasWayPointArea : StateDecision
    {
        public override string CategoryName
        {
            get { return "Movement/"; }
        }

        public override string DefaultName
        {
            get { return "Has WayPointArea?"; }
        }

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            var ability = fsmBehaviour.IAIController.GetAbility<WaypointAbility>();
            if (ability == null) return false;
            return ability.WaypointArea != null;
        }
    }
}
