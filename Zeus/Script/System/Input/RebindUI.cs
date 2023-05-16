using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Zeus
{
    [Flags]
    public enum TypeRebind
    { 
        NONE = 0,

        DPAD_UP = 1 << 0,
        DPAD_DOWN = 1 << 1,
        DPAD_LEFT = 1 << 2,
        DPAD_RIGHT = 1 << 3,
        DPAD = DPAD_UP | DPAD_DOWN | DPAD_LEFT | DPAD_RIGHT,

        BUTTON_NORTH = 1 << 4,
        BUTTON_SOUTH = 1 << 5,
        BUTTON_WEST = 1 << 6,
        BUTTON_EAST = 1 << 7,
        BUTTON = BUTTON_NORTH | BUTTON_SOUTH | BUTTON_WEST | BUTTON_EAST,

        LS_UP = 1 << 8,
        LS_DOWN = 1 << 9,
        LS_LEFT = 1 << 10,
        LS_RIGHT = 1 << 11,
        LS_BUTTON = 1 << 12,
        LS = LS_UP | LS_DOWN | LS_LEFT | LS_RIGHT,
        LS_ALL = LS | LS_BUTTON,

        RS_UP = 1 << 13,
        RS_DOWN = 1 << 14,
        RS_LEFT = 1 << 15,
        RS_RIGHT = 1 << 16,
        RS_BUTTON = 1 << 17,
        RS = RS_UP | RS_DOWN | RS_LEFT | RS_RIGHT,
        RS_ALL = RS | RS_BUTTON,

        START = 1 << 18,
        L1 = 1 << 19,
        L2 = 1 << 20,
        R1 = 1 << 21,
        R2 = 1 << 22,

        ALL = ~NONE,
    }
    public class RebindUI : MonoBehaviour
    {
        [SerializeField] private InputActionReference _inputActionReference;
        [Range(0, 10)]
        [SerializeField] private int _selectedBinding;
        [SerializeField] private InputBinding.DisplayStringOptions _displayStringOptions;
        
        [Header("Binding Info")]
        [ReadOnly, SerializeField] private InputBinding _inputBinding;
        [ReadOnly, SerializeField] private int _bindingIndex;
        [ReadOnly, SerializeField] private string _actionName;

        [Header("UI Fields")]
        [SerializeField] private TextMeshProUGUI _actionText;
        [SerializeField] private TextMeshProUGUI _bindingText;
        [SerializeField] private GameObject _rebindOverlay;
        [SerializeField] private TextMeshProUGUI _rebindText;

        private void Start()
        {
            InputReader.Instance.CallRebindStarted += RebindStarted;
            InputReader.Instance.CallRebindUpdated += UpdateBindingDisplay;
            InputReader.Instance.CallRebindStopped += RebindStoped;

            if (_inputActionReference != null)
            {
                InputReader.Instance.LoadBindingOverride(_inputActionReference.action.name);
                GetBindingInfo();
                UpdateBindingDisplay();
            }
        }
        private void OnDestroy()
        {
            if (InputReader.HasInstance)
            {
                InputReader.Instance.CallRebindStarted -= RebindStarted;
                InputReader.Instance.CallRebindUpdated -= UpdateBindingDisplay;
                InputReader.Instance.CallRebindStopped -= RebindStoped;
            }
        }
        private void OnValidate()
        {
            if (_inputActionReference != null)
            {
                GetBindingInfo();
                UpdateBindingDisplay();
            }
        }

        private void RebindStarted(string rebindText)
        {
            _rebindOverlay.SetActive(true);
            _rebindText.SetText(rebindText);
        }
        private void RebindStoped(bool complete)
        {
            _rebindOverlay.SetActive(false);
        }

        private void GetBindingInfo()
        {
            if (_inputActionReference.action != null)
                _actionName = _inputActionReference.action.name;

            if (_inputActionReference.action.bindings.Count > _selectedBinding)
            {
                _inputBinding = _inputActionReference.action.bindings[_selectedBinding];
                _bindingIndex = _selectedBinding;
            }
        }
        private void UpdateBindingDisplay()
        {
            //if (_actionText != null)
            //    _actionText.SetText(_actionName);

            if (_bindingText != null)
            {
                if (Application.isPlaying)
                {
                    var action = InputReader.Instance.GetAction(_actionName);
                    _bindingText.SetText(action.GetBindingDisplayString(_bindingIndex));
                }
                else
                {
                    _bindingText.SetText(_inputActionReference.action.GetBindingDisplayString(_bindingIndex));
                }
            }
        }

        public void StartRebind()
        {
            InputReader.Instance.StartRebind(_actionName, _bindingIndex, _rebindText);
        }
        public void ResetBinding()
        {
            InputReader.Instance.ResetBinding(_actionName, _bindingIndex);
        }
    }
}