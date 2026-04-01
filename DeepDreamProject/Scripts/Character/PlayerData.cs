using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData : DontDestorySingleton<PlayerData>
{
    [System.Serializable]
    public class LocationInformation
    {
        public Vector3 PlayerPos;
        public Vector3 PlayerRot;
    }
    public LocationInformation LocationInfo;

    public float SleepPower = 500f;
    public float MaxSleepPower = 1000f;
}
