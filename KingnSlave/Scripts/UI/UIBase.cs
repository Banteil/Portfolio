using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public abstract class UIBase : MonoBehaviour
    {
        protected Dictionary<Type, UnityEngine.Object[]> objectsDic = new Dictionary<Type, UnityEngine.Object[]>();
        protected bool isInitialized = false;

        public void Initialized()
        {
            if (isInitialized) return;
            InitializedProcess();
            isInitialized = true;
        }

        protected abstract void InitializedProcess();

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

        protected T Get<T>(int idx, bool checkInheritance = false) where T : UnityEngine.Object
        {
            UnityEngine.Object[] objects = null;
            if (checkInheritance)
            {

                if (!objectsDic.TryGetValue(typeof(T), out objects))
                    return null;
            }
            else
            {
                if (!objectsDic.TryGetValue(typeof(T), out objects))
                    return null;
            }
            return objects[idx] as T;
        }

        protected T[] GetAll<T>(bool checkInheritance = false) where T : UnityEngine.Object
        {
            UnityEngine.Object[] objects = null;
            var type = typeof(T);
            if (checkInheritance)
            {

                if (!objectsDic.TryGetValue(type, out objects))
                    return null;
            }
            else
            {
                if (!objectsDic.TryGetValue(typeof(T), out objects))
                    return null;
            }

            T[] result = new T[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                result[i] = (T)objects[i];
            }
            return result;
        }

        public static void BindEvent(GameObject obj, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
        {
            UIEventHandler evt = Util.GetOrAddComponent<UIEventHandler>(obj);
            switch (type)
            {
                case Define.UIEvent.Click:
                    evt.OnClickHandler -= action;
                    evt.OnClickHandler += action;
                    break;
                case Define.UIEvent.Drag:
                    evt.OnDragHandler -= action;
                    evt.OnDragHandler += action;
                    break;
                case Define.UIEvent.EndDrag:
                    evt.OnEndDragHandler -= action;
                    evt.OnEndDragHandler += action;
                    break;
            }
        }

        public static void ClearEvent(GameObject obj, Define.UIEvent type = Define.UIEvent.Click)
        {
            UIEventHandler evt = Util.GetOrAddComponent<UIEventHandler>(obj);
            switch (type)
            {
                case Define.UIEvent.Click:
                    evt.OnClickHandler = null;
                    break;
                case Define.UIEvent.Drag:
                    evt.OnDragHandler = null;
                    break;
                case Define.UIEvent.EndDrag:
                    evt.OnEndDragHandler = null;
                    break;
            }
        }

        public virtual void InputEscape() { }

        public virtual void SetListData(UIList list) { }

        public virtual void SetCallback(Action callback) { }

        public ScrollRect GetScrollRect(int idx) { return Get<ScrollRect>(idx); }
        protected TextMeshProUGUI GetText(int idx) { return Get<TextMeshProUGUI>(idx); }
        protected Button GetButton(int idx) { return Get<Button>(idx); }
        protected Image GetImage(int idx) { return Get<Image>(idx); }
        protected RawImage GetRawImage(int idx) { return Get<RawImage>(idx); }
    }
}
