using System;
using UnityEngine;

namespace Zeus
{    public enum TypeAnimState { NONE = 0, ENTER, UPDATE, EXIT }

    [CreateAssetMenu(fileName = "AnimStateEventSO", menuName = "Zeus/Events/Anim State Event")]
    public class AnimStateEventSO : GameEventSO<TypeAnimState> { } 
}
