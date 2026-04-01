using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAbility : MonoBehaviour
{
    //어빌리티의 주체
    protected Character _character;
    //캐릭터 스탯이 있는지 여부 체크
    protected bool _characterStatExist;
    //어빌리티 초기화가 완료되었는지 여부 체크
    protected bool _abilityInitialized;

    public bool AbilityInitialized { get { return _abilityInitialized; } }
    //AI 행동 등으로 인풋이 스크립트를 통해 진행될때 bool true
    public bool ScriptDrivenInput = false;
    //어빌리티 시작, 종료 되었을 때 피드백
    public MMFeedbacks AbilityStartFeedbacks;
    public MMFeedbacks AbilityStopFeedbacks;

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
        _character = gameObject.GetComponentInParent<Character>();        
    }

    /// <summary>
    /// Gets and stores components for further use
    /// </summary>
    protected virtual void Initialization()
    {
        if (_character.CharacterStat != null) _characterStatExist = true;
        if (_character.ControlType.Equals(CharacterControlType.AI)) ScriptDrivenInput = true;

        _abilityInitialized = true;
    }

    public virtual void EarlyProcessAbility()
    {
        InternalHandleInput();
    }

    protected virtual void InternalHandleInput() 
    {
        //handleInput 내용 추가
        HandleInput();
    }
    protected virtual void HandleInput() { }

    public virtual void ProcessAbility() { }

    public virtual void LateProcessAbility() { }

    public virtual void UpdateAnimator() { }

    public virtual void Feedback() { }
}
