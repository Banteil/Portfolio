using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    int hp;

    void Start()
    {
        hp = 8 + (DataPassing.stageNum * 2);
    }

    public bool attackWall(int damage)
    {
        hp -= damage;

        if (hp <= 0) return true;

        return false;
    }
}
