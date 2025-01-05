using UnityEngine.Events;

namespace starinc.io.kingnslave
{
    // 람다는 AddPersistentListener로 연결 불가
    public static class EventUtilities
    {
        public static void AddPersistentListener(this UnityEvent unityEvent, UnityAction actionCall)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, actionCall);
#else
        unityEvent.AddListener(actionCall);
#endif
        }

        public static void AddPersistentListener<T0>(this UnityEvent<T0> unityEvent, UnityAction<T0> actionCall)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, actionCall);
#else
        unityEvent.AddListener(actionCall);
#endif
        }

        public static void AddPersistentListener<T0, T1>(this UnityEvent<T0, T1> unityEvent, UnityAction<T0, T1> actionCall)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, actionCall);
#else
        unityEvent.AddListener(actionCall);
#endif
        }

        public static void AddPersistentListener<T0, T1, T2>(this UnityEvent<T0, T1, T2> unityEvent, UnityAction<T0, T1, T2> actionCall)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, actionCall);
#else
        unityEvent.AddListener(actionCall);
#endif
        }

        public static void AddPersistentListener<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> unityEvent, UnityAction<T0, T1, T2, T3> actionCall)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.AddPersistentListener(unityEvent, actionCall);
#else
        unityEvent.AddListener(actionCall);
#endif
        }
    }
}