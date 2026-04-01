using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionObject : MonoBehaviour
{
    [Header("Interaction Info")]
    public string ActInfo = "Interaction";

    [Header("Interaction Feedbacks")]
    public MMFeedbacks InteractionStartFeedbacks;
    public MMFeedbacks InteractionEndFeedbacks;

    public virtual void InteractionAct(Character character)
    {
        SetOutline(false);
        InteractionAbility interactionAbility = character.GetAbility<InteractionAbility>();
        interactionAbility.RemoveInteractionObject(this);
    }

    protected virtual void SetOutline(bool isActive) { }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            if (character.ControlType.Equals(CharacterControlType.PLAYER)) SetOutline(true);
            InteractionAbility interactionAbility = character.GetAbility<InteractionAbility>();
            interactionAbility?.AddInteractionObject(this);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        Character character = collision.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            if (character.ControlType.Equals(CharacterControlType.PLAYER)) SetOutline(false);
            InteractionAbility interactionAbility = character.GetAbility<InteractionAbility>();
            interactionAbility?.RemoveInteractionObject(this);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Character character = other.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            if (character.ControlType.Equals(CharacterControlType.PLAYER)) SetOutline(true);
            InteractionAbility interactionAbility = character.GetAbility<InteractionAbility>();
            interactionAbility?.AddInteractionObject(this);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        Character character = other.gameObject.GetComponentInParent<Character>();
        if (character != null)
        {
            if (character.ControlType.Equals(CharacterControlType.PLAYER)) SetOutline(false);
            InteractionAbility interactionAbility = character.GetAbility<InteractionAbility>();
            interactionAbility?.RemoveInteractionObject(this);
        }
    }
}
