using Cysharp.Threading.Tasks;
using UnityEngine;

namespace starinc.io
{
    public abstract class SensorWrapperScriptable : ScriptableObject
    {
        public abstract string Name { get; }
        public abstract bool IsAvailable { get; }
        public abstract bool IsEnabled { get; }
        public abstract UniTask InitializeAsync(float timeout = 5f);
        public abstract void Enable(bool enable);
    }
}