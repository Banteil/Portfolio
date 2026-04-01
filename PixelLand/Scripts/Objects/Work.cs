using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Work : InteractionObject
{
    protected float maxTime;
    protected ItemType interactableType;
    [SerializeField]
    protected MyItemDropInfo _dropInfo;
    public MyItemDropInfo DropInfo { get { return _dropInfo; } }

    public override void Interaction(CharacterBasic character)
    {
        character.HitBox.InteractOff(this);
        character.State = CharacterState.WORK;
        character.WC.Working(interactableType);
        StartCoroutine(WorkProcess(character));
    }

    public abstract IEnumerator WorkProcess(CharacterBasic character);

    public abstract void WorkComplete();
}
