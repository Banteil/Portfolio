using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEventData : EventBase
{
    public static int MaxEventGrade = 5;
    [Range(0, 4)]
    public int Grade;

    public virtual void InitializationEvent(Room room) { }

    public virtual void MeetingEvent(Room room) { }
}
