using System;
using System.Collections.Generic;
using UnityEngine;

namespace starinc.io
{
    public static class Manager
    {
        private static Dictionary<Type, BaseManager> _managers = new Dictionary<Type, BaseManager>();
        public static int IsTestBuild
        {
            get
            {
#if UNITY_EDITOR
                return Define.INT_TRUE;
#elif TEST_BUILD
                return Define.INT_TRUE;
#else
                return Define.INT_FALSE;
#endif
            }
        }

        public static T Get<T>() where T : BaseManager
        {
            var type = typeof(T);
            if (_managers.TryGetValue(type, out var manager))
            {
                return manager as T;  // T 타입으로 캐스팅 후 반환
            }
            else
            {                
                GameObject obj = new GameObject(typeof(T).Name);  // 새로운 GameObject 생성
                T newManager = obj.AddComponent<T>();  // 컴포넌트로 T 타입의 매니저 추가
                return newManager;
            }
        }

        public static bool Set(BaseManager manager)
        {
            var type = manager.GetType();
            if (_managers.ContainsKey(type))
            {
                return false;  // 이미 존재하면 false 반환
            }

            // 사전에 추가
            _managers.Add(type, manager);
            return true;
        }

        #region Shortcut
        public static GameManager Game => Get<GameManager>();
        public static SoundManager Sound => Get<SoundManager>();
        public static UserManager User => Get<UserManager>();
        public static UIManager UI => Get<UIManager>();
        public static SceneLoadManager Load => Get<SceneLoadManager>();
        public static InputManager Input => Get<InputManager>();
        public static AdManager Ad => Get<AdManager>();
        public static IAPManager IAP => Get<IAPManager>();
        public static EncryptionManager Enc => Get<EncryptionManager>();
        #endregion
    }
}
