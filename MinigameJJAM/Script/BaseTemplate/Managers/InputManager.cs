using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace starinc.io
{
    public class InputManager : BaseManager
    {
        #region Cache
        private InputActionAsset _inputActionAsset;
        private InputAction _cancelAction, _clickAction, _dragAction;        
        private EventSystem _eventSystem;
        #endregion

        #region Callback
        public event Action OnEscapeButton;
        public event Action OnInputDetection;
        public event Action<InputAction.CallbackContext> OnClickEvent;
        public event Action<InputAction.CallbackContext> OnDragEvent;
        #endregion

        protected override void OnAwake()
        {
            _inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
            ActionInitialization();
            CreateEventSystem();
        }

        private void ActionInitialization()
        {
#if !UNITY_IOS
            _cancelAction = _inputActionAsset.FindActionMap("UI").FindAction("Cancel");
            _cancelAction.Enable();
            _cancelAction.performed += EscapeAction;
#endif
            _clickAction = _inputActionAsset.FindActionMap("UI").FindAction("Click");
            _clickAction.Enable();
            _clickAction.performed += ClickEvent;
            _clickAction.performed += InputDetectionAction;

            _dragAction = _inputActionAsset.FindActionMap("UI").FindAction("Drag");
            _dragAction.Enable();
            _dragAction.performed += DragEvent;            
            _dragAction.performed += InputDetectionAction;            
        }

        public InputAction GetInputAction(string actionMap, string actionName)
        {
            var map = _inputActionAsset.FindActionMap(actionMap);
            if (map == null)
            {
                Debug.LogWarning($"ActionMap {actionMap}을 찾을 수 없습니다.");
                return null;
            }

            var action = map.FindAction(actionName);
            if (action == null) Debug.LogWarning($"InputAction {actionName}은 존재하지 않습니다.");
            return action;
        }

        private void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            _eventSystem = eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            eventSystem.transform.SetParent(transform);

            Manager.Load.OnSceneLoadProcessStarted += ClearSelectedUIBeforeSceneLoad;
            Manager.Load.OnNextSceneLoadCompleted += DestroyExtraEventSystems;
            DestroyExtraEventSystems();
        }

        private void ClickEvent(InputAction.CallbackContext context) => OnClickEvent?.Invoke(context);
        private void DragEvent(InputAction.CallbackContext context) => OnDragEvent?.Invoke(context);
        private void EscapeAction(InputAction.CallbackContext context) => OnEscapeButton?.Invoke();
        private void InputDetectionAction(InputAction.CallbackContext context) => OnInputDetection?.Invoke();

        private void ClearSelectedUIBeforeSceneLoad()
        {
            ClearInputEvent();
            if (_eventSystem != null)
            {
                _eventSystem.SetSelectedGameObject(null);
                var inputModule = _eventSystem.currentInputModule as InputSystemUIInputModule;
                if (inputModule != null)
                {
                    inputModule.DeactivateModule();
                    inputModule.ActivateModule();
                }
            }
        }

        private void DestroyExtraEventSystems()
        {
            var eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var es in eventSystems)
            {
                if (es != _eventSystem)
                {
                    Destroy(es.gameObject);
                }
            }
        }

        private void ClearInputEvent()
        {
            OnClickEvent = null;
            OnDragEvent = null;
        }
    }
}
