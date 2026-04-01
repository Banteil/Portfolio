using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Skill : MonoBehaviour
{
    [SerializeField]
    protected List<GameObject> _effectList = new List<GameObject>();

    public abstract void UseSkill(Transform _baseTr);
}
