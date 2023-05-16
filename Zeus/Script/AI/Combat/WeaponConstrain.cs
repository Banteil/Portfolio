using UnityEngine;
using Zeus;

[ClassHeader(" Weapon Constrain ", "Weapon Constrain Helper: call true OnEquip and false OnDrop by CollectableStandalone events to avoid handler movement on 2018.x", IconName = "meleeIcon")]
public class WeaponConstrain : zMonoBehaviour
{
    Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Call OnEquipe (true) / OnDrop (false)
    public void Inv_Weapon_FreezeAll(bool status)
    {        
        if (status)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            _rigidbody.constraints = RigidbodyConstraints.None;
        }        
    }
}