using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBase : ScriptableObject
{
    [Header("Event Info")]
    public int EventID;
    public string EventName;
    [TextArea]
    public string EventDescription;
}
