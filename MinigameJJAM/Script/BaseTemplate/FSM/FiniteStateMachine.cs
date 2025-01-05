using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(menuName = "FSM/FiniteStateMachine")]
    public class FiniteStateMachine : ScriptableObject
    {
        public List<State> States;
        private State _currentState;

        public void OnEnable()
        {
            if (States.Count > 0)
            {
                _currentState = States[0];
            }
        }

        public State GetCurrentState() => _currentState;

        public void ChangeState(State nextState)
        {
            if (nextState == _currentState) return;
            _currentState?.OnStateExit();
            _currentState = nextState;
            _currentState.OnStateEnter();
        }
        
        public void UpdateState()
        {
            _currentState?.OnStateUpdate();
            foreach (var transition in _currentState.Transitions)
            {
                if (transition.Condition.Decide())
                {
                    ChangeState(transition.TrueState);
                    return;
                }
            }
        }

        public void OnDisable()
        {
            
        }
    }
}
