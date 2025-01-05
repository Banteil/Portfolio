using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace starinc.io.kingnslave
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
        
        public static void ClearEvent(this GameObject obj, Define.UIEvent type = Define.UIEvent.Click)
        {
            UIBase.ClearEvent(obj, type);
        }

        /// <summary>
        /// 매개변수 obj의 (Clone)이름 제거하는 함수
        /// </summary>
        /// <param name="obj"></param>
        public static void ExcludingClone(this GameObject obj)
        {
            int index = obj.name.IndexOf("(Clone)");
            if (index > 0)
                obj.name = obj.name.Substring(0, index);
        }

        public static bool IsAnimationPlaying(this Animator animator)
        {
            AnimatorStateInfo currentAnimation = animator.GetCurrentAnimatorStateInfo(0);
            var isPlaying = currentAnimation.normalizedTime < 1.0f;
            return isPlaying;
        }
    }
}
