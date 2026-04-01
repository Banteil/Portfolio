using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "new ItemData", menuName = "PixelLand/Data/Skill", order = 0)]
public class SkillData : ScriptableObject
{
    [SerializeField]
    Skill _skill;
    public Skill Skill => _skill;

    public void ApplyDirty()
    {
        EditorUtility.SetDirty(this);
    }
}

