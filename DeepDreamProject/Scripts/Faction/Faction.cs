using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Faction", menuName = "Data/Faction")]
public class Faction : ScriptableObject
{
    public List<Faction> FriendlyFactions;
    public List<Faction> HostileFactions;

    public bool CheckHostile(Faction faction)
    {
        return HostileFactions.Contains(faction);
    }

    public bool CheckFriendly(Faction faction)
    {
        return FriendlyFactions.Contains(faction);
    }
}
