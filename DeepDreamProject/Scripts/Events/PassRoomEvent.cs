using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassRoomEvent", menuName = "Data/RoomEventData/PassRoomEvent")]
public class PassRoomEvent : RoomEventData
{
    [System.Serializable]
    public class PassRoomData
    {
        public GameObject Object;
        public Vector3 Pos;

        public void ObjectSetting(Room room)
        {
            GameObject roomObj = Instantiate(Object, room.BindObjects, false);
            roomObj.transform.localPosition = Pos;
        }
    }
    public List<PassRoomData> PassRoomDataList;

    public override void InitializationEvent(Room room)
    {
        for (int i = 0; i < PassRoomDataList.Count; i++)
        {
            PassRoomDataList[i].ObjectSetting(room);
        }
    }

    public override void MeetingEvent(Room room)
    {
        room.OpenDoors();
        room.IsClear = true;
    }
}
