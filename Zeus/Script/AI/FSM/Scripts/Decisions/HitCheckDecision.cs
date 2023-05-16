using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check if the current target with a HealthController is Dead", UnityEditor.MessageType.Info)]
    [CreateAssetMenu(fileName = "HitCheckDecision", menuName = "Zeus/Scriptable Object/Decision/HitCheckDecision", order = int.MaxValue)]
#endif
    public class HitCheckDecision : StateDecision
    {
        public override string CategoryName
        {
            get { return "Combat/"; }
        }
        public override string DefaultName
        {
            get { return "Check What Hit"; }
        }

        public List<string> ObjectNameList = new List<string>();

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            var healthController = fsmBehaviour.gameObject.GetComponent<HealthController>();
            if (ObjectNameList.Count == 0 || string.IsNullOrEmpty(healthController.LastHitObjectName)) return false;
            return ObjectNameList.Contains(healthController.LastHitObjectName);
        }        
    }
}