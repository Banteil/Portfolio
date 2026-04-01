using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUI : MonoBehaviour
{
    [SerializeField]
    protected CharacterBasic target;
    public CharacterBasic Target { get { return target; } set { target = value; } }
}
