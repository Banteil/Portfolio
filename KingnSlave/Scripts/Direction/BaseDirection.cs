using System.Collections.Generic;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace starinc.io.kingnslave
{
    public class BaseDirection : MonoBehaviour
    {
        protected Canvas canvas;
        protected Animator animator;

        protected Dictionary<Type, UnityEngine.Object[]> objectsDic = new Dictionary<Type, UnityEngine.Object[]>();

        protected void Bind<T>(Type type) where T : UnityEngine.Object
        {
            string[] names = Enum.GetNames(type);

            UnityEngine.Object[] objects;
            if (!objectsDic.ContainsKey(typeof(T)))
            {
                objects = new UnityEngine.Object[name.Length];
                objectsDic.Add(typeof(T), objects);
            }
            else
                objects = objectsDic[typeof(T)];

            int index = 0;
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] == null) break;
                index++;
            }

            for (int i = 0; i < names.Length; i++)
            {
                if (typeof(T) == typeof(GameObject))
                    objects[i + index] = Util.FindChild(gameObject, names[i], true);
                else
                    objects[i + index] = Util.FindChild<T>(gameObject, names[i], true);

                if (objects[i + index] == null)
                    Debug.Log($"{names[i]} 객체의 바인드에 실패하였습니다.");
            }
        }

        protected T Get<T>(int idx) where T : UnityEngine.Object
        {
            UnityEngine.Object[] objects = null;
            if (!objectsDic.TryGetValue(typeof(T), out objects))
                return null;

            return objects[idx] as T;
        }

        protected T[] GetAll<T>() where T : UnityEngine.Object
        {
            UnityEngine.Object[] objects = null;
            if (!objectsDic.TryGetValue(typeof(T), out objects))
                return null;

            T[] result = new T[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                result[i] = (T)objects[i];
            }
            return result;
        }

        protected void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            canvas = gameObject.GetOrAddComponent<Canvas>();
            canvas.sortingOrder = Define.DIRECTION_SORT_ORDER;
            animator = gameObject.GetOrAddComponent<Animator>();
        }

        async public virtual UniTask StartDirection()
        {
            animator.SetTrigger("Start");
            await UniTask.WaitUntil(() => animator.IsAnimationPlaying());
            await UniTask.WaitUntil(() => !animator.IsAnimationPlaying());
        }

        public virtual void StartDirectionStartEvent() { }
        public virtual void StartDirectionEndEvent() { }

        public virtual void EndDirection()
        {
            animator.SetTrigger("End");
        }

        public virtual void EndDirectionStartEvent() { } 
        public virtual void EndDirectionEndEvent()
        {
            Destroy(gameObject);
        }

        public virtual void PlaySFXEvent(string clipName) => AudioManager.Instance.PlaySFX(ResourceManager.Instance.GetSFXClip(clipName));
    }
}
