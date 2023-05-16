using UnityEngine;

namespace Zeus
{
#if UNITY_EDITOR
    [FSMHelpbox("Check if the current target with a HealthController is Dead", UnityEditor.MessageType.Info)]
    [CreateAssetMenu(fileName = "CheckMonsterDeath", menuName = "Zeus/Scriptable Object/Decision/CheckMonsterDeath", order = int.MaxValue)]
#endif
    public class CheckMonsterDeath : StateDecision
    {
        public override string CategoryName
        {
            get { return "Detection/"; }
        }
        public override string DefaultName
        {
            get { return "CheckMonsterDeath"; }
        }

        public string GroupID;

        public override bool Decide(IFSMBehaviourController fsmBehaviour)
        {
            //이것도 수정 필요
            return CharacterObjectManager.Get().IsEmptyGroup(GroupID);
        }
    }
}