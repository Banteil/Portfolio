using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackBox : MonoBehaviour
{
    MonsterBasic monster;
    BoxCollider2D attackBoxCollider;
    public BoxCollider2D AttackBoxCollider { get { return attackBoxCollider; } }

    void Start()
    {
        monster = GetComponentInParent<MonsterBasic>();
        attackBoxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.GetComponent<CharacterHitBox>() != null)
        {
            CollisionCheck(collision.GetComponent<CharacterHitBox>());
        }
    }

    void CollisionCheck(CharacterHitBox hitBox)
    {
        if (hitBox.InvincibilityCheck) return;
        int knockbackPower = (int)(monster.Info.Power * 100);
        if (knockbackPower > 500) knockbackPower = 500;
        hitBox.Hit(monster.Info.Power, knockbackPower, transform.position, monster);
        if (hitBox.CB.State.Equals(CharacterState.DIE))
            monster.Target = null;
    }
}
