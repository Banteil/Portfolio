using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "Data/RoomData")]
public class RoomData : ScriptableObject
{
    public Sprite GroundSprite;
    public Sprite WallSprite;
    public RuntimeAnimatorController DoorAnimator;
}
