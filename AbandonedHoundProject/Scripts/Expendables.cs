using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Expendables
{
    const int potionValue = 10;
    const int foodValue = 20;
    const int bombValue = 50;

    public static int PotionValue
    {
        get { return potionValue * PlayerState.Instance.potionGrade; }
    }

    public static int FoodValue
    {
        get { return foodValue * PlayerState.Instance.foodGrade; }
    }

    public static int BombValue
    {
        get { return bombValue * PlayerState.Instance.bombGrade; }
    }

    public static int UsePotion
    {
        get
        {
            int value = (int)(PlayerState.Instance.MaxHp / 100f * (potionValue * PlayerState.Instance.potionGrade));
            return value;
        }
    }

    public static int UseFood
    {
        get
        {
            int value = (int)(PlayerState.Instance.MaxStamina / 100f * (foodValue * PlayerState.Instance.foodGrade));
            return value;
        }
    }

    public static int BattleUseFood
    {
        get
        {
            int value = PlayerState.Instance.foodGrade * 3;
            return value;
        }
    }

    public static int AttackBomb
    {
        get
        {
            int value = bombValue * PlayerState.Instance.bombGrade;
            return value;
        }
    }
}
