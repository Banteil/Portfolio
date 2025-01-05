using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io
{
    public static class Extension
    {
        public static T GetOrAddComponent<T>(this GameObject obj) where T : UnityEngine.Component
        {
            return Util.GetOrAddComponent<T>(obj);
        }

        public static void BindEvent(this GameObject obj, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
        {
            UIBase.BindEvent(obj, action, type);
        }

        public static void RemoveEvent(this GameObject obj)
        {
            UIBase.RemoveEvent(obj);
        }

        public static Transform FindChildRecursive(this Transform parent, string targetName)
        {
            // 현재 트랜스폼의 이름이 목표와 일치하는지 확인
            if (parent.name == targetName)
                return parent;

            // 자식 트랜스폼들을 순회
            foreach (Transform child in parent)
            {
                // 자식에서 재귀적으로 탐색
                Transform result = FindChildRecursive(child, targetName);
                if (result != null)
                    return result;
            }

            // 찾지 못한 경우 null 반환
            return null;
        }
    }
}
