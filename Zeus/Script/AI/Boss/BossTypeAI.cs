using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public class BossTypeAI : MonoBehaviour
    {
        protected ZeusAIController _zeusAI;

        public UnityEvent StartEvent;
        public UnityEvent ActiveEvent;
        public UnityEvent InactiveEvent;
        public UnityEvent DieEvent;
        public UnityEvent AfterDieEvent;

        void Start()
        {
            Initialized();            
        }

        protected virtual void Initialized()
        {
            _zeusAI = GetComponent<ZeusAIController>();
            if (_zeusAI == null)
            {
                Debug.LogError("AI Controller가 존재하지 않습니다.");
                enabled = false;
                return;
            }

            _zeusAI.OnDeadEvent.AddListener((x) => DieEvent?.Invoke());
            _zeusAI.OnAfterDieEvent.AddListener(() => AfterDieEvent?.Invoke());
            StartEvent?.Invoke();
        }

        public virtual void ActiveBossEvent() => ActiveEvent?.Invoke();

        public virtual void InactiveBossEvent() => InactiveEvent?.Invoke();
    }
}
