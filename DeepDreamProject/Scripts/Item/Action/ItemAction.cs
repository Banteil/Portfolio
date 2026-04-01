using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionStates { NONE, STARTACTION, INACTION, ENDACTION }

[RequireComponent(typeof(Item))]
public class ItemAction : MonoBehaviour
{
    public ActionStates State;

    //액션의 주체
    protected Item _item;
    //어빌리티 초기화가 완료되었는지 여부 체크
    protected bool _actionInitialized;
    public bool ActionInitialized { get { return _actionInitialized; } }

    [Header("Feedbacks")]
    //액션 시작, 종료 되었을 때 피드백
    public MMFeedbacks ActionStartFeedbacks;
    public MMFeedbacks ActionStopFeedbacks;

    protected virtual void Awake()
    {
        PreInitialization();
    }

    /// <summary>
    /// On Start(), we call the ability's intialization
    /// </summary>
    protected virtual void Start()
    {
        Initialization();
    }

    /// <summary>
    /// A method you can override to have an initialization before the actual initialization
    /// </summary>
    protected virtual void PreInitialization()
    {
        _item = gameObject.GetComponentInParent<Item>();        
    }

    /// <summary>
    /// Gets and stores components for further use
    /// </summary>
    protected virtual void Initialization()
    {
        //초기화 내용
        _actionInitialized = true;
    }

    protected virtual void Update()
    {
        if (_item == null || State.Equals(ActionStates.NONE)) return;

        switch(State)
        {
            case ActionStates.STARTACTION:
                StartAction();
                break;
            case ActionStates.INACTION:
                InAction();
                break;
            case ActionStates.ENDACTION:
                EndAction();
                break;
        }
    }

    public virtual void StartAction() { }

    public virtual void InAction() { }

    public virtual void EndAction() { }

    public virtual void UpdateAnimator() { }

    public virtual void Feedback() { }
}
