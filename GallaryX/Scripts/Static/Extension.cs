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
            // ���� Ʈ�������� �̸��� ��ǥ�� ��ġ�ϴ��� Ȯ��
            if (parent.name == targetName)
                return parent;

            // �ڽ� Ʈ���������� ��ȸ
            foreach (Transform child in parent)
            {
                // �ڽĿ��� ��������� Ž��
                Transform result = FindChildRecursive(child, targetName);
                if (result != null)
                    return result;
            }

            // ã�� ���� ��� null ��ȯ
            return null;
        }
    }
}
