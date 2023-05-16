using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class TimelineTrigger : InteractionTrigger
    {
        [SerializeField] private int _id;
        [SerializeField] private UnityEvent<int> _callEnterEvent;

        public override void OnEnter(InteractionActor actor)
        {
            base.OnEnter(actor);

            _callEnterEvent.Invoke(_id);
        }
    }

}