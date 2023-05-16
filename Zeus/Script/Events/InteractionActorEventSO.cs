using UnityEngine;

namespace Zeus
{
    [CreateAssetMenu(menuName = "Zeus/Events/Interaction Actor Event")]

    public class InteractionActorEventSO : GameEventSO<InteractionTrigger, bool> { }
}