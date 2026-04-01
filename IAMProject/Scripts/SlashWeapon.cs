using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;

public class SlashWeapon : MonoBehaviour
{
    ProjectileWeapon _projectileWeapon;
    public GameObject[] SlashEffects;
    public Vector3 Spread;

    private void Awake()
    {
        _projectileWeapon = GetComponent<ProjectileWeapon>();
    }

    public void SlashFeedback()
    {
        if (SlashEffects.Length.Equals(0)) return;

        int rand = Random.Range(0, SlashEffects.Length);
        Projectile projectile = Instantiate(SlashEffects[rand]).GetComponent<Projectile>();
        projectile.transform.position = _projectileWeapon.transform.position;
        Vector3 SpreadDirection = _projectileWeapon.RandomSpreadDirection;
        SpreadDirection.x = Random.Range(-Spread.x, Spread.x);
        SpreadDirection.y = Random.Range(-Spread.y, Spread.y);
        SpreadDirection.z = Random.Range(-Spread.z, Spread.z);
        Quaternion spread = Quaternion.Euler(SpreadDirection);
        projectile.SetDirection(spread * _projectileWeapon.transform.forward, _projectileWeapon.transform.rotation, true);
    }

    
}
