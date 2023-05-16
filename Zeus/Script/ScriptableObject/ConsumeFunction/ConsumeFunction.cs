using UnityEngine;

namespace Zeus
{
    public abstract class ConsumeFunction : ScriptableObject
    {
        public virtual void OnEffect(int value) { }
        public virtual void OnRelease() { }
    }
}