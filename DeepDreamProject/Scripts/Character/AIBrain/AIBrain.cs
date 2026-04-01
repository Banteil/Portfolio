using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    [Header("AIBrain Info")]
    /// AI 행동 제어 여부
    public bool BrainActive = true;
    /// AI가 탑재되는 캐릭터
    public Character Owner;
    /// 상태 리스트
    public List<AIState> States;
    /// AI의 현재 상태
    public AIState CurrentState { get; protected set; }
    /// 현재 상태에서 보낸 시간
    public float TimeInThisState;
    /// 현재 타겟
    public Transform Target;
    /// 타겟의 마지막으로 체크된 월드 포지션
    public Vector3 LastKnownTargetPosition = Vector3.zero;

    [Header("Frequencies")]
    /// 작업을 수행할 빈도(초) (낮은 값: 높은 빈도, 높은 값: 낮은 빈도이지만 더 나은 성능)
    public float ActionsFrequency = 0f;
    /// 결정을 평가할 빈도(초)
    public float DecisionFrequency = 0f;
    ///행동 및 결정 빈도를 무작위화할지 여부
    public bool RandomizeFrequencies = false;
    /// 동작 빈도를 무작위화할 최소값과 최대값
    public Vector2 RandomActionFrequency = new Vector2(0.5f, 1f);
    /// 결정 빈도를 무작위화할 최소값과 최대값
    public Vector2 RandomDecisionFrequency = new Vector2(0.5f, 1f);

    protected AIDecision[] _decisions;
    protected AIAction[] _actions;
    protected float _lastActionsUpdate = 0f;
    protected float _lastDecisionsUpdate = 0f;
    protected AIState _initialState;

    public virtual AIAction[] GetAttachedActions()
    {
        AIAction[] actions = this.gameObject.GetComponentsInChildren<AIAction>();
        return actions;
    }

    public virtual AIDecision[] GetAttachedDecisions()
    {
        AIDecision[] decisions = this.gameObject.GetComponentsInChildren<AIDecision>();
        return decisions;
    }

    /// <summary>
    /// On awake we set our brain for all states
    /// </summary>
    protected virtual void Awake()
    {
        foreach (AIState state in States)
        {
            state.SetBrain(this);
        }
        _decisions = GetAttachedDecisions();
        _actions = GetAttachedActions();
        if (RandomizeFrequencies)
        {
            ActionsFrequency = Random.Range(RandomActionFrequency.x, RandomActionFrequency.y);
            DecisionFrequency = Random.Range(RandomDecisionFrequency.x, RandomDecisionFrequency.y);
        }
    }

    /// <summary>
    /// On Start we set our first state
    /// </summary>
    protected virtual void Start()
    {
        ResetBrain();
    }

    /// <summary>
    /// Every frame we update our current state
    /// </summary>
    protected virtual void Update()
    {
        if (!BrainActive || (CurrentState == null) || (Time.timeScale == 0f))
        {
            return;
        }

        if (Time.time - _lastActionsUpdate > ActionsFrequency)
        {
            CurrentState.PerformActions();
            _lastActionsUpdate = Time.time;
        }

        if (!BrainActive)
        {
            return;
        }

        if (Time.time - _lastDecisionsUpdate > DecisionFrequency)
        {
            CurrentState.EvaluateTransitions();
            _lastDecisionsUpdate = Time.time;
        }

        TimeInThisState += Time.deltaTime;

        StoreLastKnownPosition();
    }

    /// <summary>
    /// Transitions to the specified state, trigger exit and enter states events
    /// </summary>
    /// <param name="newStateName"></param>
    public virtual void TransitionToState(string newStateName)
    {
        if (CurrentState == null)
        {
            CurrentState = FindState(newStateName);
            if (CurrentState != null)
            {
                CurrentState.EnterState();
            }
            return;
        }
        if (newStateName != CurrentState.StateName)
        {
            CurrentState.ExitState();
            OnExitState();

            CurrentState = FindState(newStateName);
            if (CurrentState != null)
            {
                CurrentState.EnterState();
            }
        }
    }

    /// <summary>
    /// When exiting a state we reset our time counter
    /// </summary>
    protected virtual void OnExitState()
    {
        TimeInThisState = 0f;
    }

    /// <summary>
    /// Initializes all decisions
    /// </summary>
    protected virtual void InitializeDecisions()
    {
        if (_decisions == null)
        {
            _decisions = GetAttachedDecisions();
        }
        foreach (AIDecision decision in _decisions)
        {
            decision.Initialization();
        }
    }

    /// <summary>
    /// Initializes all actions
    /// </summary>
    protected virtual void InitializeActions()
    {
        if (_actions == null)
        {
            _actions = GetAttachedActions();
        }
        foreach (AIAction action in _actions)
        {
            action.Initialization();
        }
    }

    /// <summary>
    /// Returns a state based on the specified state name
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    protected AIState FindState(string stateName)
    {
        foreach (AIState state in States)
        {
            if (state.StateName == stateName)
            {
                return state;
            }
        }
        if (stateName != "")
        {
            Debug.LogError("You're trying to transition to state '" + stateName + "' in " + this.gameObject.name + "'s AI Brain, but no state of this name exists. Make sure your states are named properly, and that your transitions states match existing states.");
        }
        return null;
    }

    /// <summary>
    /// Stores the last known position of the target
    /// </summary>
    protected virtual void StoreLastKnownPosition()
    {
        if (Target != null)
        {
            LastKnownTargetPosition = Target.transform.position;
        }
    }

    /// <summary>
    /// Resets the brain, forcing it to enter its first state
    /// </summary>
    public virtual void ResetBrain()
    {
        InitializeDecisions();
        InitializeActions();
        BrainActive = true;
        this.enabled = true;

        if (CurrentState != null)
        {
            CurrentState.ExitState();
            OnExitState();
        }

        if (States.Count > 0)
        {
            CurrentState = States[0];
            CurrentState?.EnterState();
        }
    }
}
