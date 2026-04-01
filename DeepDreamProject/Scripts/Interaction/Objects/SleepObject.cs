using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SleepObject : InteractionObject
{
    public SceneData MoveSceneData;
    protected Volume _volume;
    protected SpriteRenderer _spriteRenderer;

    protected virtual void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _volume = Camera.main.GetComponentInChildren<Volume>();
    }

    public override void InteractionAct(Character character)
    {
        base.InteractionAct(character);
        if (character.ControlType.Equals(CharacterControlType.PLAYER))
            StartCoroutine(SleepProcess(character));
    }

    protected override void SetOutline(bool isActive)
    {
        float interactionFloat = isActive ? 1f : 0f;
        _spriteRenderer.material.SetFloat("_OutlineThickness", interactionFloat);
    }

    IEnumerator SleepProcess(Character character)
    {
        InputManager.Instance.InputNotAllowed = true;

        ItemHandleAbility itemHandleAbility = character.GetAbility<ItemHandleAbility>();
        itemHandleAbility.SyncHand = false;
        character.Animator.SetTrigger("Sleep");

        yield return new WaitForSeconds(2f);        
        float timer = 0f;
        while (timer < 3f)
        {
            if (_volume.profile.TryGet<ColorAdjustments>(out var color))
            {
                color.postExposure.value -= 3f * Time.deltaTime;
            }
            timer += Time.deltaTime;
            yield return null;
        }        
        SceneLoadManager.Instance.LoadScene(MoveSceneData);
        InputManager.Instance.InputNotAllowed = false;
    }
}
