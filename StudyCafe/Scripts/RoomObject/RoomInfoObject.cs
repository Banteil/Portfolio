using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInfoObject : MonoBehaviour
{
    private void OnMouseDown()
    {
        RoomManager.Instance.OnRoomInfo();
    }
}
