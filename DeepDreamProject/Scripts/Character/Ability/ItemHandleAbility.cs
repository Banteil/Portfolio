using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandleAbility : CharacterAbility
{
    public Item HandleItem;
    protected bool _syncHand = true;
    public bool SyncHand
    {
        get { return _syncHand; }
        set
        {
            _syncHand = value;
            if (_leftHand != null) _leftHand.gameObject.SetActive(_syncHand);
            if (_rightHand != null) _rightHand.gameObject.SetActive(_syncHand);
        }
    }
    public Action<ItemData> OnHandleCallback;

    protected Item _baseHand;
    protected Transform _leftHand;
    protected Transform _rightHand;
    protected Vector3 _leftHandBasePos;
    protected Vector3 _rightHandBasePos;

    protected ButtonStates _actionButtonInput;
    protected ButtonStates _throwButtonInput;

    protected ButtonStates _actionButtonState;
    protected ButtonStates _throwButtonState;

    protected override void PreInitialization()
    {
        base.PreInitialization();
        _baseHand = _character.BindWeaponAttachment.Find("Hand").GetComponent<Item>();
        if (_baseHand != null)
            _baseHand.BindAttackBound.IgnoreGameObject(_character.BindModel.gameObject);
        _leftHand = _character.BindWeaponAttachment.Find("LeftHand").transform;
        _rightHand = _character.BindWeaponAttachment.Find("RightHand").transform;
    }

    protected override void Initialization()
    {
        _leftHandBasePos = _leftHand.localPosition;
        _rightHandBasePos = _rightHand.localPosition;
        SetBaseHand();
        base.Initialization();
    }

    protected override void InternalHandleInput()
    {
        if (!InputManager.HasInstance) return;

        _actionButtonInput = InputManager.Instance.ActionInput.ButtonState;
        _throwButtonInput = InputManager.Instance.ThrowInput.ButtonState;
        base.InternalHandleInput();
    }

    protected override void HandleInput()
    {
        base.HandleInput();

        _actionButtonState = _actionButtonInput;
        _throwButtonState = _throwButtonInput;
    }

    public virtual void SetAction()
    {
        _actionButtonState = ButtonStates.ButtonPressed;
    }

    public virtual void SetThrow()
    {
        _throwButtonState = ButtonStates.ButtonUp;
    }

    public virtual void SetHandleItem(Item item)
    {
        _baseHand?.gameObject.SetActive(false);
        HandleItem = item;
        HandleItem.SpriteRenderer.sortingOrder = -2;
        item.Owner = _character;
        item.State = ItemStates.HANDLE;
        item.transform.SetParent(_character.BindWeaponAttachment, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        item.RigidBody2D.isKinematic = true;
        item.RigidBody2D.velocity = Vector2.zero;
        item.RigidBody2D.angularVelocity = 0f;
        OnHandleCallback?.Invoke(item.Data);
    }

    public override void ProcessAbility()
    {
        if (HandleItem == null) return;
        SyncHandPos();
        ChangeItemState();
    }

    void SyncHandPos()
    {
        if (!SyncHand) return;

        if (_leftHand != null)
        {
            if (HandleItem.LeftHandTr != null)
                _leftHand.position = HandleItem.LeftHandTr.position;
            else
                _leftHand.localPosition = _leftHandBasePos;
        }
        if (_rightHand != null)
        {
            if (HandleItem.RightHandTr != null)
                _rightHand.position = HandleItem.RightHandTr.position;
            else
                _rightHand.localPosition = _rightHandBasePos;
        }
    }

    void ChangeItemState()
    {
        if (_actionButtonState.Equals(ButtonStates.ButtonPressed))
        {
            if (HandleItem.ActState.Equals(ItemActStates.IDLE))
                HandleItem.ActState = ItemActStates.ACTIVEACTION;
        }

        if (_throwButtonState.Equals(ButtonStates.ButtonUp))
        {
            if (HandleItem.Data.IsBind || _character.State.Equals(CharacterStates.ACT)) return;
            HandleItem.ActState = ItemActStates.STARTTHROWING;
            HandleItem.SpriteRenderer.sortingOrder = -1;
            Reset();
            SetBaseHand();
        }
    }

    public void ItemBreak()
    {
        Destroy(HandleItem.gameObject);
        SetBaseHand();
    }

    void SetBaseHand()
    {
        _baseHand?.gameObject.SetActive(true);
        HandleItem = _baseHand;
        OnHandleCallback?.Invoke(HandleItem.Data);
    }

    protected virtual void Reset()
    {
        _leftHand.localPosition = _leftHandBasePos;
        _rightHand.localPosition = _rightHandBasePos;
    }
}
