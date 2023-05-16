using System.Linq;
using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check Throw Item Collision", UnityEditor.MessageType.Info)]
#endif
    public class CollisionThrowItem : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "Check Throw Item Collision"; }
        }

        public LayerMask CheckLayer;
        public string ThrowItemName;
        public float Distance = 2f;

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            var colls = Physics.OverlapSphere(fsmBehaviour.transform.position, Distance, CheckLayer).ToList().FindAll((x) => x.gameObject.GetComponent<ProjectileAttackObject>() != null);
            var items = colls.FindAll((x) => x.GetComponent<ProjectileAttackObject>().AttackObjectName.Equals(ThrowItemName));
            if (items.Count <= 0) return false;

            items = items.OrderBy(x => Vector3.Distance(fsmBehaviour.transform.position, x.transform.position)).ToList();
            var dis = Vector3.Distance(fsmBehaviour.transform.position, items[0].transform.position);
            return dis <= Distance;
        }
    }
}
