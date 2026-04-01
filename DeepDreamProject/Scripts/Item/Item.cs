using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemStates { DROP, HANDLE }
public enum ItemActStates { IDLE, ACTIVEACTION, STARTTHROWING, BEINGTHROWN, ENDTHROWING }

public class Item : InteractionObject
{
    [Header("Item Info")]
    public ItemStates State = ItemStates.DROP;
    public ItemActStates ActState = ItemActStates.IDLE;
    public DimensionTypes DimensionType;
    public Character Owner;
    public string GUID;

    [Header("Binding Info")]
    public Transform BindActions;
    public DamageOnTouch BindAttackBound;

    [Header("Item Data")]
    public ItemData Data;
    public LayerMask TargetLayer;

    [Header("Feedbacks")]
    public MMFeedbacks StartAttackFeedback;
    public MMFeedbacks EndAttackFeedback;
    public MMFeedbacks StartActionFeedback;
    public MMFeedbacks EndActionFeedback;
    public MMFeedbacks StartThrowingFeedback;
    public MMFeedbacks EndThrowingFeedback;

    protected Transform _bindModel;
    protected Transform _leftHandTr;
    public Transform LeftHandTr { get { return _leftHandTr; } }
    protected Transform _rightHandTr;
    public Transform RightHandTr { get { return _rightHandTr; } }

    protected Animator _animator;
    public Animator Animator { get { return _animator; } }
    protected Rigidbody2D _rigidbody2D;
    public Rigidbody2D RigidBody2D { get { return _rigidbody2D; } }
    protected Rigidbody _rigidbody;
    public Rigidbody Rigidbody { get { return _rigidbody; } }

    protected SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return _spriteRenderer; } }

    protected List<ItemAction> _actions;
    public List<ItemAction> Actions { get { return _actions; } }

    protected Health _health;
    protected Vector3 _targetPos;
    protected Vector3 _direction;
    protected float _attackDelay;
    protected float _timer;

    protected virtual void Awake()
    {
        _bindModel = transform.Find("__Model");
        _leftHandTr = _bindModel.Find("LeftHandTr");
        _rightHandTr = _bindModel.Find("RightHandTr");
        _animator = _bindModel.GetComponent<Animator>();
        Transform itemSprite = _bindModel.Find("ItemSprite");
        if (itemSprite != null)
            _spriteRenderer = itemSprite.GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        if (_health != null)
            _health.OnDeath += OnDeath;
    }

    protected virtual void Start()
    {
        SetItemData();
        InitializeActionList();

        //아이템이 드랍 상태라면 애니메이션 정지 및 데미지 미적용 상태로 전환
        if (State.Equals(ItemStates.DROP))
        {
            _animator.enabled = false;
            BindAttackBound?.ActiveCollider(false);
        }
    }

    /// <summary>
    /// 아이템의 스탯, GUID 등 필요 정보를 세팅하는 함수
    /// </summary>
    void SetItemData()
    {
        if (Data == null) return;

        if (_spriteRenderer != null)
            _spriteRenderer.sprite = Data.ItemSprite;

        GUID = Guid.NewGuid().ToString();
        _health.MaximumHP = Data.Durability;
        _health.SetInitialHealth();
        _health.Invulnerable = Data.IsUnbreakable;
    }

    /// <summary>
    /// 아이템 액션 정보를 정리하는 함수
    /// </summary>
    protected virtual void InitializeActionList()
    {
        _actions = BindActions != null ? BindActions.GetComponentsInChildren<ItemAction>().ToList() : transform.GetComponentsInChildren<ItemAction>().ToList();
    }

    protected override void SetOutline(bool isActive)
    {
        float interactionFloat = isActive ? 1f : 0f;
        _spriteRenderer.material.SetFloat("_OutlineThickness", interactionFloat);
    }

    public override void InteractionAct(Character character)
    {
        base.InteractionAct(character);
        ItemHandleAbility itemHandleAbility = character.GetAbility<ItemHandleAbility>();
        if (itemHandleAbility == null) return;

        itemHandleAbility.SetHandleItem(this);
        _animator.enabled = true;
        BindAttackBound.IgnoreGameObject(Owner.BindModel.gameObject);
    }

    #region 아이템 사용 관련 함수

    protected virtual void Update()
    {
        if (Data == null || Owner == null) return;
        switch (ActState)
        {
            case ItemActStates.ACTIVEACTION:
                ActiveAction();
                break;
            case ItemActStates.STARTTHROWING:
                StartThrowing();
                break;
            case ItemActStates.ENDTHROWING:
                EndThrowing();
                break;
            default:

                break;
        }
    }

    private void FixedUpdate()
    {
        if (Data == null || Owner == null) return;
        switch (ActState)
        {
            case ItemActStates.BEINGTHROWN:
                BeingThrown();
                break;
        }
    }

    protected virtual void ActiveAction()
    {
        foreach (ItemAction action in _actions)
        {
            action.State = ActionStates.STARTACTION;
        }

        ActState = ItemActStates.IDLE;
        StartActionFeedback?.PlayFeedbacks(transform.position);
    }

    protected virtual void StartThrowing()
    {
        transform.SetParent(null);
        _animator.Rebind();
        _animator.Update(0f);
        _animator.enabled = false;
        BindAttackBound?.ActiveCollider(true);

        if (DimensionType.Equals(DimensionTypes.DIMENSION2D))
        {
            _rigidbody2D.isKinematic = false;

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10;
            _targetPos = Camera.main.ScreenToWorldPoint(mousePosition);
            _targetPos.z = transform.position.z;
            _direction = (_targetPos - transform.position).normalized;

            float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        }

        ActState = ItemActStates.BEINGTHROWN;
        StartThrowingFeedback?.PlayFeedbacks(this.transform.position);
    }

    protected virtual void BeingThrown()
    {
        if (DimensionType.Equals(DimensionTypes.DIMENSION2D))
        {
            transform.localRotation *= Quaternion.Euler(0f, 0f, Data.ThrowPower);
            _rigidbody2D.velocity = _direction * Data.ThrowPower;
            float targetDistance = Vector2.Distance(_targetPos, transform.position);
            if (targetDistance <= float.Epsilon + 1f)
            {
                _rigidbody2D.velocity = Vector2.zero;
                ActState = ItemActStates.ENDTHROWING;
                return;
            }
        }
    }

    protected virtual void EndThrowing()
    {
        State = ItemStates.DROP;
        ActState = ItemActStates.IDLE;
        Owner = null;
        BindAttackBound.ClearIgnoreList();
        BindAttackBound?.ActiveCollider(false);
        EndThrowingFeedback?.PlayFeedbacks(this.transform.position);
    }

    #endregion

    public virtual ItemAction GetAction(string typeName)
    {
        foreach (ItemAction action in _actions)
        {
            string name = action.GetType().ToString();
            if (name.Equals(typeName))
            {
                return action;
            }
        }
        return null;
    }

    public virtual void Reflect()
    {
        if (ActState.Equals(ItemActStates.BEINGTHROWN))
        {
            ActState = ItemActStates.ENDTHROWING;

            if (DimensionType.Equals(DimensionTypes.DIMENSION2D))
                _rigidbody2D.velocity = Vector2.Reflect(_rigidbody2D.velocity, _direction.normalized) * 0.5f;
            else
                _rigidbody.velocity = Vector3.Reflect(_rigidbody.velocity, _direction.normalized) * 0.5f;
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        Reflect();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!State.Equals(ItemStates.DROP)) return;
        base.OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (!State.Equals(ItemStates.DROP)) return;
        base.OnTriggerExit2D(collision);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        Reflect();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!State.Equals(ItemStates.DROP)) return;
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (!State.Equals(ItemStates.DROP)) return;
        base.OnTriggerExit(other);
    }

    protected virtual void OnDeath()
    {
        ItemHandleAbility itemHandleAbility = Owner.GetAbility<ItemHandleAbility>();
        itemHandleAbility.ItemBreak();
    }
}
