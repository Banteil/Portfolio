using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : InteractionObject
{
    public override void InteractionAct(Character character)
    {
        base.InteractionAct(character);
        if (character.ControlType.Equals(CharacterControlType.PLAYER))
            StartCoroutine(InteractionProcess());
    }

    IEnumerator InteractionProcess()
    {
        InteractionStartFeedbacks?.PlayFeedbacks();
        InputManager.Instance.InputNotAllowed = false;

        yield return null;

        InteractionEndFeedbacks?.PlayFeedbacks();
    }
}
