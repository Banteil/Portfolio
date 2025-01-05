using UnityEngine;

namespace starinc.io.gallaryx
{
    public class BaseAbility : MonoBehaviour
    {
        protected CharacterController _characterController;

        public Transform CharacterTransform { get { return _characterController.transform; } }
        public Animator CharacterAnimator { get { return _characterController.AnimatorComponent; } }

        public bool IsAI { get { return _characterController.Type != CharacterType.Player; } }
        
        protected InputEvent AbilityInputEvent { get { return !_characterController.IsInteraction ? _characterController.Brain.BrainInputEvent : new InputEvent(); } }

        private void Awake() => OnAwake();

        protected virtual void OnAwake()
        {
            _characterController = GetComponentInParent<CharacterController>();
            if (_characterController == null)
            {
                Destroy(gameObject);
                return;
            }            
        }

        private void Start() => Initialize();

        protected virtual void Initialize()
        {
            _characterController.InputStateCallback += AbilityInputState;
            _characterController.InputActionCallback += AbilityInputAction;
            _characterController.CheckInteractionsCallback += Call_CharacterInteractionState;
        }

        protected virtual void AbilityInputState() { }

        protected virtual void AbilityInputAction() { }

        protected virtual void Call_CharacterInteractionState(bool isInteraction) { }

        public virtual void RemoveAbility()
        {
            _characterController.InputStateCallback -= AbilityInputState;
            _characterController.InputActionCallback -= AbilityInputAction;
            _characterController.CheckInteractionsCallback -= Call_CharacterInteractionState;
            Destroy(gameObject);
        }
    }
}
