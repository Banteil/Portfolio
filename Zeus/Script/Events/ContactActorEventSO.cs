using UnityEngine;

namespace Zeus
{
    /// <summary>
    /// InteractableActor와 InteractionController 접촉 이벤트
    /// </summary>
    [CreateAssetMenu(menuName = "Zeus/Events/Contact Actor Event")]
    public class ContactActorEventSO : GameEventSO<bool, InteractionTrigger> { }
}