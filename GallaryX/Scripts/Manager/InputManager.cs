using System;
using UnityEngine;

namespace starinc.io.gallaryx
{
    public class InputManager : Singleton<InputManager>
    {
        public VirtualController MobileVirtualController { get; private set; }
        public InputEvent KeyInputEvent = new InputEvent();
        public event Action ControlKeyInputCallback;
        public Action InputEnterCallback;
        public float TouchDeltaMag { get; set; }

        public bool PlayerControlInput
        {
            get
            {
                var inputMoveKey = (Mathf.Abs(KeyInputEvent.InputHorizontal()) + Mathf.Abs(KeyInputEvent.InputVertical())) != 0;
                var inputRunning = KeyInputEvent.InputRunKey() == InputState.Stay;
                var inputJump = KeyInputEvent.InputJumpKey() == InputState.Up;

                return inputMoveKey || inputRunning || inputJump;
            }
        }

        protected override void OnAwake()
        {
            MobileVirtualController = GetComponentInChildren<VirtualController>();
            SetInputEvent();
        }

        private void SetInputEvent()
        {
            KeyInputEvent.InputHorizontal = () => GetAxis("Horizontal") + MobileVirtualController.InputH;
            KeyInputEvent.InputVertical = () => GetAxis("Vertical") + MobileVirtualController.InputV;
            KeyInputEvent.InputRunKey = () => GetCombinedInputState(GetKeyState(KeyCode.LeftShift), MobileVirtualController.InputRunButton);
            KeyInputEvent.InputJumpKey = () => GetCombinedInputState(GetKeyState(KeyCode.Space), MobileVirtualController.InputJumpButton);
            KeyInputEvent.InputMouseLeftKey = () => GetMouseKeyState(0);
            KeyInputEvent.InputMouseWheel = () => GetAxis("Mouse ScrollWheel", false);
            KeyInputEvent.InputReturnKey = () => GetKeyState(KeyCode.Return);
            KeyInputEvent.InputKeypadEnterKey = () => GetKeyState(KeyCode.KeypadEnter);
            KeyInputEvent.InputEscapeKey = () => GetKeyState(KeyCode.Escape);
            KeyInputEvent.InputRecordKey = () => GetKeyState(KeyCode.R);
            KeyInputEvent.InputGuideUIKey = () => GetKeyState(KeyCode.G);
            KeyInputEvent.InputHideUIKey = () => GetKeyState(KeyCode.Z);
            KeyInputEvent.InputAutoMoveKey = () => GetKeyState(KeyCode.Q);
            KeyInputEvent.InputEditUIKey = () => GetKeyState(KeyCode.P);
        }

        private float GetAxis(string state, bool isRaw = true)
        {
            if(isRaw) return Input.GetAxisRaw(state);
            else return Input.GetAxis(state);
        }

        private InputState GetKeyState(KeyCode keyCode)
        {
            if (Input.GetKeyDown(keyCode)) return InputState.Down;
            else if (Input.GetKey(keyCode)) return InputState.Stay;
            else if (Input.GetKeyUp(keyCode)) return InputState.Up;
            else return InputState.None;
        }

        private InputState GetMouseKeyState(int key)
        {
            if (Input.GetMouseButtonDown(key)) return InputState.Down;
            else if (Input.GetMouseButton(key)) return InputState.Stay;
            else if (Input.GetMouseButtonUp(key)) return InputState.Up;
            else return InputState.None;
        }

        private float MobilePinchControl()
        {
            float deltaMagnitudeDiff = 0;
            if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                // 이전 터치 간의 거리와 현재 터치 간의 거리를 계산
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;
                float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
                float touchDeltaMag = (touch1.position - touch2.position).magnitude;

                // 터치 간의 거리가 증가하면 줌 아웃, 감소하면 줌 인
                deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
            }
            if (deltaMagnitudeDiff < 0)
                deltaMagnitudeDiff = -0.1f;
            else if (deltaMagnitudeDiff > 0)
                deltaMagnitudeDiff = 0.1f;

            return deltaMagnitudeDiff;
        }

        private void Update()
        {
            KeyInputEvent.InputDownCallback?.Invoke();
            KeyInputEvent.InputStayCallback?.Invoke();
            KeyInputEvent.InputUpCallback?.Invoke();

            TouchDeltaMag = MobilePinchControl();

            if (PlayerControlInput)
                ControlKeyInputCallback?.Invoke();
        }

        private InputState GetCombinedInputState(InputState state1, InputState state2)
        {
            var result = (InputState)Mathf.Max((int)state1, (int)state2);
            return result;
        }

        public void JSEnterPressed() => InputEnterCallback?.Invoke();
    }
}
