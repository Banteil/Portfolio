using UnityEngine;

namespace Zeus
{
    public class ContactTrigger : InteractionTrigger
    {
        [SerializeField] private int _targedID;
        [SerializeField] private TypeQuestStep _stepType;

        public override void OnEnter(InteractionActor actor)
        {
            var dirVector = transform.position - actor.transform.position;
            var distance = Mathf.CeilToInt(dirVector.sqrMagnitude);
            QuestManager.Instance.UpdateQuest(_stepType, _targedID, distance);
        }
    }
}