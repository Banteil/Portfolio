using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace starinc.io.gallaryx
{
    public class VirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent<InputState> InputStateEvent;
        private bool _isPressed = false;        
        private bool _stateProcessed = false;
        private InputState _currentState = InputState.None;

        void Update()
        {
            if (_isPressed)
            {
                if (!_stateProcessed)
                {
                    UIManager.Instance.InteractUI = true;
                    _currentState = InputState.Down;
                    _stateProcessed = true;
                }
                else
                {
                    UIManager.Instance.InteractUI = true;
                    _currentState = InputState.Stay;
                }
            }
            else if (_currentState == InputState.Down || _currentState == InputState.Stay)
            {
                UIManager.Instance.InteractUI = false;
                _currentState = InputState.Up;
                _stateProcessed = true;
            }
            else
            {
                _currentState = InputState.None;
            }

            InputStateEvent?.Invoke(_currentState);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _stateProcessed = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            _stateProcessed = false;
        }
    }
}
