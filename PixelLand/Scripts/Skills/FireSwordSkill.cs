using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSwordSkill : Skill
{
    int _stack = 0;

    public override void UseSkill(Transform _baseTr)
    {
        _stack++;
        if((_stack % 3).Equals(0))
        {
            if(_stack.Equals(9))
            {
                Instantiate(_effectList[0], _baseTr, false);
                GameObject obj = Instantiate(_effectList[1], _baseTr.position, _baseTr.rotation * Quaternion.Euler(0f, 0f, 90f));
                _stack = 0;
            }
            else
                Instantiate(_effectList[0], _baseTr, false);
        }
    }
}
