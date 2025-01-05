using System;
using UnityEngine;

namespace starinc.io.gallaryx
{    
    public enum InputState
    {
        None,
        Down,
        Stay,
        Up,
    }

    public delegate float GetFloatEvent();
    public delegate bool GetBooleanEvent();
    public delegate Quaternion GetQuaternionEvent();
    public delegate InputState GetInputStateEvent();

    public class InputEvent
    {
        public GetFloatEvent InputHorizontal, InputVertical, InputMouseWheel;
        public GetInputStateEvent InputRunKey, InputJumpKey, InputMouseLeftKey, InputReturnKey, InputKeypadEnterKey, InputEscapeKey, InputRecordKey, InputAutoMoveKey, InputHideUIKey, InputGuideUIKey, InputEditUIKey;

        public Action InputDownCallback;
        public Action InputStayCallback;
        public Action InputUpCallback;
    }
}
