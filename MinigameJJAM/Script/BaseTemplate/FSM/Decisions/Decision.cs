using UnityEngine;

namespace starinc.io
{
    [CreateAssetMenu(menuName = "FSM/Decision")]
    public abstract class Decision : ScriptableObject
    {
        public abstract bool Decide();
    }
}
