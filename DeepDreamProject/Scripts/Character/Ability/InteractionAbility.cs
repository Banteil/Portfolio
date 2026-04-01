using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionAbility : CharacterAbility
{
    public InteractionUI InteractionUI;
    public InteractionObject MainInteractionObject;

    protected List<InteractionObject> _interactionObjects = new List<InteractionObject>();
    protected ButtonStates _interactionButtonInput;
    protected ButtonStates _interactionButtonState;

    protected bool _interactionEventPlaying;

    protected override void Initialization()
    {
        InteractionUI?.SetActive(false);
        base.Initialization();
    }

    protected override void InternalHandleInput()
    {
        if (!InputManager.HasInstance) return;
        _interactionButtonInput = InputManager.Instance.InteractionInput.ButtonState;
        base.InternalHandleInput();
    }

    protected override void HandleInput()
    {
        base.HandleInput();
        _interactionButtonState = _interactionButtonInput;
    }

    public virtual void SetInteraction()
    {
        _interactionButtonInput = ButtonStates.ButtonUp;
    }

    public override void ProcessAbility()
    {
        if (_interactionObjects.Count.Equals(0) || _interactionEventPlaying)
        {
            InteractionUI.SetActive(false);
            return;
        }

        CheckMainInteraction();
        if (MainInteractionObject != null)
        {
            if (_interactionButtonState.Equals(ButtonStates.ButtonUp))
            {
                MainInteractionObject.InteractionAct(_character);
            }
        }
    }

    protected virtual void CheckMainInteraction()
    {
        if (MainInteractionObject != null) return;
        InteractionUI?.SetActive(false);

        if(_interactionObjects.Count > 0)
        {
            MainInteractionObject = _interactionObjects[0];
            InteractionUI?.SetActInfo(MainInteractionObject.ActInfo);
            InteractionUI?.SetActive(true);
        }
    }

    public void AddInteractionObject(InteractionObject interactionObject)
    {
        if (!_interactionObjects.Contains(interactionObject))
        {
            _interactionObjects.Add(interactionObject);

            //타겟과의 거리를 기준으로 정렬
            _interactionObjects.Sort(delegate (InteractionObject a, InteractionObject b)
            {
                if (a == null || b == null)
                {
                    return 0;
                }

                return Vector2.Distance(_character.transform.position, a.transform.position)
                .CompareTo(
                    Vector2.Distance(_character.transform.position, b.transform.position));
            });
        }
    }

    public void RemoveInteractionObject(InteractionObject interactionObject)
    {
        if (_interactionObjects.Contains(interactionObject))
            _interactionObjects.Remove(interactionObject);

        MainInteractionObject = null;
    }

    public override void UpdateAnimator()
    {

    }
}
