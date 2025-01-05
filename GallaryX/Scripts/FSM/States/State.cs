
using System.Collections.Generic;
using starinc.io.gallaryx;
using UnityEngine;

//[CreateAssetMenu(menuName = "FSM/State")]
public abstract class State : ScriptableObject
{
    public string StateName;

    public List<Transition> Transitions = new List<Transition>();

    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public abstract void OnStateExit();
}

[System.Serializable]
public class Transition
{
    public Decision Condition;
    public State TrueState;
}