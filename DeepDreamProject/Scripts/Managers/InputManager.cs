using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp }

public class InputManager : DontDestorySingleton<InputManager>
{
    public class InputData
    {
        public InputData(string buttonID, Action buttonDown = null, Action buttonPressed = null, Action buttonUp = null)
        {
            ButtonID = buttonID;
            ButtonState = ButtonStates.Off;
            ButtonDownAction = buttonDown;
            ButtonPressedAction = buttonPressed;
            ButtonUpAction = buttonUp;
        }

        public string ButtonID;
        public ButtonStates ButtonState;
        public Action ButtonDownAction;
        public Action ButtonPressedAction;
        public Action ButtonUpAction;

        float _lastButtonDownAt;
        float _lastButtonUpAt;

        public float TimeSinceLastButtonDown { get { return Time.unscaledTime - _lastButtonDownAt; } }
        public float TimeSinceLastButtonUp { get { return Time.unscaledTime - _lastButtonUpAt; } }
        public bool ButtonDownRecently(float time) { return (Time.unscaledTime - TimeSinceLastButtonDown <= time); }
        public bool ButtonUpRecently(float time) { return (Time.unscaledTime - TimeSinceLastButtonUp <= time); }

        public virtual void TriggerButtonDown()
        {
            _lastButtonDownAt = Time.unscaledTime;
            if (ButtonDownAction == null)
                ButtonState = ButtonStates.ButtonDown;
            else
                ButtonDownAction();
        }

        public virtual void TriggerButtonPressed()
        {
            if (ButtonPressedAction == null)
                ButtonState = ButtonStates.ButtonPressed;
            else
                ButtonPressedAction();
        }

        public virtual void TriggerButtonUp()
        {
            _lastButtonUpAt = Time.unscaledTime;
            if (ButtonUpAction == null)
                ButtonState = ButtonStates.ButtonUp;
            else
            {
                ButtonUpAction();
            }
        }
    }

    List<InputData> _inputDataList = new List<InputData>(); 
    public InputData FireInput { get; private set; }
    public InputData ThrowInput { get; private set; }
    public InputData ActionInput { get; private set; }
    public InputData JumpInput { get; private set; }
    public InputData DashInput { get; private set; }
    public InputData InteractionInput { get; private set; }
    public InputData SubmitInput { get; private set; }
    public InputData CancelInput { get; private set; }

    public Vector2 MovementDirection;
    //입력 불가 여부
    bool _inputNotAllowed;
    public bool InputNotAllowed
    {
        get { return _inputNotAllowed; }
        set
        {
            _inputNotAllowed = value;
            if(_inputNotAllowed)
            {
                foreach (InputData inputData in _inputDataList)
                {
                    inputData.ButtonState = ButtonStates.Off;
                }
                MovementDirection = Vector2.zero;                
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        InitializeInputData();
    }

    void InitializeInputData()
    {
        _inputDataList.Add(FireInput = new InputData("Fire"));
        _inputDataList.Add(ThrowInput = new InputData("Throw"));
        _inputDataList.Add(ActionInput = new InputData("Action"));
        _inputDataList.Add(JumpInput = new InputData("Jump"));
        _inputDataList.Add(DashInput = new InputData("Dash"));
        _inputDataList.Add(InteractionInput = new InputData("Interaction"));
        _inputDataList.Add(SubmitInput = new InputData("Submit"));
        _inputDataList.Add(CancelInput = new InputData("Cancel"));
        MovementDirection = Vector2.zero;
    }

    void Update()
    {
        if (_inputNotAllowed) return;
        GetInputData();
        SetMovement();
    }

    void GetInputData()
    {        
        foreach (InputData inputData in _inputDataList)
        {
            if (Input.GetButton(inputData.ButtonID))
                inputData.TriggerButtonPressed();

            if (Input.GetButtonDown(inputData.ButtonID))
                inputData.TriggerButtonDown();

            if (Input.GetButtonUp(inputData.ButtonID))
                inputData.TriggerButtonUp();
        }
    }

    void SetMovement()
    {
        MovementDirection.x = Input.GetAxisRaw("Horizontal");
        MovementDirection.y = Input.GetAxisRaw("Vertical");
    }

    void LateUpdate()
    {
        ProcessInputStates();
    }

    public void ProcessInputStates()
    {
        foreach (InputData inputData in _inputDataList)
        {
            if (inputData.ButtonState.Equals(ButtonStates.ButtonDown))
                inputData.ButtonState = ButtonStates.ButtonPressed;

            if (inputData.ButtonState.Equals(ButtonStates.ButtonUp))
                inputData.ButtonState = ButtonStates.Off;
        }
    }
}
