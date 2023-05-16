using UnityEngine;

namespace Zeus
{
    public class CharacterAbility : MonoBehaviour
    {
        protected Character _owner;
        private bool _initialized;
        public bool AbilityInitialized => _initialized;
        public bool IsProcessing;

        private void Awake()
        {
            AwakeInitialize();
        }

        protected virtual void AwakeInitialize()
        {
            _owner = GetComponentInParent<Character>();
            if (_owner == null)
            {
                Debug.LogError("Character 클래스가 확인되지 않습니다.");
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            Initialize();
            _initialized = true;
        }

        protected virtual void Initialize() { }
        public virtual void EarlyProcessAbility() { }
        public virtual void ProcessAbility() { }
        public virtual void LateProcessAbility() { }
    }
}
