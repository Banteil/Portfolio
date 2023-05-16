using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    [ClassHeader(" FSM BEHAVIOUR CONTROLLER", HelpBoxText = "Required a AI Controller Component", UseHelpBox = true,
        IconName = "Textures/Editor/FSMIcon2")]
    public partial class FSMBehaviourController : zMonoBehaviour, IFSMBehaviourController
    {
        public MessageReceiver MessageReceiver
        {
            get
            {
                if (_messageReceiver == null && !tryGetMessageReceiver) _messageReceiver = GetComponent<MessageReceiver>();
                if (_messageReceiver == null && !tryGetMessageReceiver) tryGetMessageReceiver = true;

                return _messageReceiver;
            }
        }
        private MessageReceiver _messageReceiver;
        private bool tryGetMessageReceiver;

        [EditorToolbar("FSM")] [SerializeField]
        protected FSMBehaviour _fsmBehaviour;

        [SerializeField] protected bool _stop;
        [SerializeField] protected bool _debugMode;
        public UnityEngine.Events.UnityEvent OnStartFSM;
        public UnityEngine.Events.UnityEvent OnPauseFSM;
        public UnityEngine.Events.UnityEvent OnResetFSM;
        public UnityEngine.Events.UnityEvent<FSMState> OnStateEnter;
        public UnityEngine.Events.UnityEvent<FSMState> OnStateExit;
        public UnityEngine.Events.UnityEvent<FSMBehaviour> OnChangeBehaviour;
        private Dictionary<string, float> _timers = new();
        FSMState _currentState;
        FSMState _lastState;
        bool _inChangeState;

        protected virtual void Start()
        {
            IAIController = GetComponent<IControlAIZeus>();
            ZeusAIController = GetComponent<ZeusAIController>();
        }

        protected virtual void Update()
        {
            if (IAIController != null && !IAIController.IsDead && !IsStopped) UpdateStates();
        }

        protected virtual void UpdateStates()
        {
            if (_currentState)
            {
                if (!_inChangeState)
                {
                    _currentState.UpdateState(this);
                    UpdateAnyState();
                }
            }
            else
            {
                Entry();
            }
        }

        public virtual void ResetFSM()
        {
            if (_currentState)
                _currentState.OnStateExit(this);
            OnResetFSM.Invoke();
            _currentState = null;
        }

        protected virtual void Entry()
        {
            if (!_fsmBehaviour) return;
            if (_fsmBehaviour.States.Count > 1)
            {
                _currentState = _fsmBehaviour.States[0];
                _currentState.OnStateEnter(this);
            }
            else if (_currentState != null) _currentState = null;
        }

        protected virtual void UpdateAnyState()
        {
            // AnyState
            if (_currentState && _fsmBehaviour && _fsmBehaviour.States.Count > 1)
            {
                _fsmBehaviour.States[1].UpdateState(this);
            }
        }

        #region FSM Interface

        public virtual FSMBehaviour FsmBehaviour
        {
            get => _fsmBehaviour;
            set => _fsmBehaviour = value;
        }

        public virtual bool DebugMode
        {
            get => _debugMode;
            set => _debugMode = value;
        }

        public virtual bool IsStopped
        {
            get => _stop;
            set => _stop = value;
        }

        public virtual IControlAIZeus IAIController { get; set; }
        public virtual ZeusAIController ZeusAIController { get; set; }

        public virtual int IndexOffCurrentState =>
            _currentState && _fsmBehaviour ? _fsmBehaviour.States.IndexOf(_currentState) : -1;

        public virtual string NameOffCurrentState => _currentState ? _currentState.Name : string.Empty;

        public virtual void SendDebug(string message, Object sender = null)
        {
            if (DebugList == null) DebugList = new List<FSMDebugObject>();
            if (DebugList.Exists(d => d.Sender == sender))
            {
                var debug = DebugList.Find(d => d.Sender == sender);
                debug.Message = message;
            }
            else
            {
                DebugList.Add(new FSMDebugObject(message, sender));
            }
        }

        public virtual List<FSMDebugObject> DebugList { get; protected set; }

        public virtual FSMState AnyState
        {
            get { return _fsmBehaviour.States.Count > 1 ? _fsmBehaviour.States[1] : null; }
        }

        public virtual FSMState CurrentState
        {
            get { return _currentState; }
            protected set { _currentState = value; }
        }

        public virtual FSMState LastState
        {
            get { return _lastState; }
            protected set { _lastState = value; }
        }

        public virtual bool HasTimer(string key)
        {
            return _timers.ContainsKey(key);
        }

        public virtual void RemoveTimer(string key)
        {
            if (_timers.ContainsKey(key)) _timers.Remove(key);
        }

        public virtual float GetTimer(string key)
        {
            if (!_timers.ContainsKey(key))
            {
                _timers.Add(key, 0f);
            }

            if (_timers.ContainsKey(key))
            {
                if (DebugMode)
                    SendDebug("<color=yellow>Get Timer " + key + " = " + _timers[key].ToString("0.0") + " </color> ",
                        gameObject);
                return _timers[key];
            }

            return 0;
        }

        public virtual void SetTimer(string key, float value)
        {
            if (!_timers.ContainsKey(key))
            {
                _timers.Add(key, value);
            }
            else if (_timers.ContainsKey(key))
            {
                _timers[key] = value;
            }

            if (DebugMode)
                SendDebug("<color=yellow>Set " + key + " Timer to " + value.ToString("0.0") + " </color> ", gameObject);
        }

        public virtual void ChangeState(FSMState state)
        {
            if (!state || state == _currentState || _inChangeState) return;
            _inChangeState = true;
            _lastState = _currentState;
            _currentState = null;
            if (_lastState)
            {
                if (DebugMode)
                    SendDebug(
                        "<color=red>EXIT:" + _lastState.name + "</color>" + "  " + "<color=yellow> ENTER :" +
                        state.Name + " </color> ", gameObject);
                _lastState.OnStateExit(this);
                OnStateExit.Invoke(_lastState);
            }

            _currentState = state;
            state.OnStateEnter(this);
            _inChangeState = false;
            OnStateExit.Invoke(state);
        }

        public virtual void ChangeBehaviour(FSMBehaviour behaviour)
        {
            if (_fsmBehaviour == behaviour) return;
            _inChangeState = true;
            _fsmBehaviour = behaviour;
            _currentState = null;
            ResetFSM();
            if (DebugMode)
                SendDebug("CHANGE BEHAVIOUR TO " + behaviour.name);
            _inChangeState = false;
            OnChangeBehaviour.Invoke(_fsmBehaviour);
        }

        public virtual void StartFSM()
        {
            IsStopped = false;
            OnStartFSM.Invoke();
        }

        public virtual void StopFSM()
        {
            IsStopped = true;
            OnPauseFSM.Invoke();
        }

        //[System.Serializable]
        //public class FSMStateEvent : UnityEngine.Events.UnityEvent<FSMState> { }

        //[System.Serializable]
        //public class FSMBehaviourEvent : UnityEngine.Events.UnityEvent<FSMBehaviour> { }

        #endregion
    }
}