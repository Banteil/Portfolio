using UnityEngine;

namespace Zeus
{
    public class QuestSpawnTrigger : InteractionTrigger
    {
        [SerializeField] private int _questID;
        
        public override void OnEnter(InteractionActor actor)
        {
            QuestManager.Instance.AddQuest(_questID);
        }
    }
}