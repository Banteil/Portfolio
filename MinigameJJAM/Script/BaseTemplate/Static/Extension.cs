using System;
using System.Collections.Generic;
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
            BaseUI.BindEvent(obj, action, type);
        }

        public static void RemoveEvent(this GameObject obj)
        {
            BaseUI.RemoveEvent(obj);
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

        public static bool IsAnimationPlaying(this Animator animator)
        {
            AnimatorStateInfo currentAnimation = animator.GetCurrentAnimatorStateInfo(0);
            var isPlaying = currentAnimation.normalizedTime < 1.0f;
            return isPlaying;
        }

        public static bool IsPause(this AudioSource audioSource)
        {
            return !audioSource.isPlaying && audioSource.time > 0 && audioSource.time < audioSource.clip.length;
        }

        public static T Dequeue<T>(this List<T> list)
        {
            if (list.Count == 0)
                throw new System.InvalidOperationException("The list is empty.");

            T item = list[0];
            list.RemoveAt(0);
            return item;
        }
    }
}
