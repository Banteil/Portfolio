using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAct : MonoBehaviour
{
    public int doorIndex = 0;
    public int direction;
    public bool doorLock = false;    
    public List<SwitchAct> connectSwitchActList = new List<SwitchAct>();    
}
