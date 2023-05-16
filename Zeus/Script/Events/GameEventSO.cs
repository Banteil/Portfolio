using UnityEngine;
using UnityEngine.Events;

namespace Zeus
{
    public abstract class GameEventSO : ScriptableObject
    {
        private UnityAction _callEvent;
        public void Raise() => _callEvent?.Invoke();
        public void Regist(UnityAction callback) => _callEvent += callback;
        public void Unregist(UnityAction callback) => _callEvent -= callback;
    }
    public abstract class GameEventSO<T> : ScriptableObject
    {
        private UnityAction<T> _callEvent;
        public void Raise(T args) => _callEvent?.Invoke(args);
        public void Regist(UnityAction<T> callback) => _callEvent += callback;
        public void Unregist(UnityAction<T> callback) => _callEvent -= callback;
    }
    public abstract class GameEventSO<T0, T1> : ScriptableObject
    {
        private UnityAction<T0, T1> _callEvent;
        public void Raise(T0 args1, T1 args2) => _callEvent?.Invoke(args1, args2);
        public void Regist(UnityAction<T0, T1> callback) => _callEvent += callback;
        public void Unregist(UnityAction<T0, T1> callback) => _callEvent -= callback;
    }
}