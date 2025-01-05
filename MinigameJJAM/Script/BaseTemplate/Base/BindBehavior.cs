using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class BindBehavior : MonoBehaviour
    {
        protected Dictionary<Type, UnityEngine.Object[]> _objectsDic = new Dictionary<Type, UnityEngine.Object[]>();

        protected void Bind<T>(Type type) where T : UnityEngine.Object
        {
            string[] names = Enum.GetNames(type);

            UnityEngine.Object[] objects;
            if (!_objectsDic.ContainsKey(typeof(T)))
            {
                objects = new UnityEngine.Object[name.Length];
                _objectsDic.Add(typeof(T), objects);
            }
            else
                objects = _objectsDic[typeof(T)];

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

        public T Get<T>(int idx) where T : UnityEngine.Object
        {
            UnityEngine.Object[] objects = null;
            if (!_objectsDic.TryGetValue(typeof(T), out objects))
                return null;

            return objects[idx] as T;
        }

        public static void BindEvent(GameObject obj, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
        {
            if(!Util.IsUIObject(obj))
            {
                Debug.LogWarning($"{obj.name} is not a UI object.");
                return;
            }

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
                case Define.UIEvent.Down:
                    evt.OnDownHandler -= action;
                    evt.OnDownHandler += action;
                    break;
                case Define.UIEvent.Up:
                    evt.OnUpHandler -= action;
                    evt.OnUpHandler += action;
                    break;
            }
        }

        public static void RemoveEvent(GameObject obj)
        {
            var evt = obj.GetComponent<UIEventHandler>();
            if (evt == null) return;

            evt.OnClickHandler = null;
            evt.OnDragHandler = null;
            evt.OnDownHandler = null;
            evt.OnUpHandler = null;
        }

        public GameObject GetGameObject(int idx) { return Get<GameObject>(idx); }
        public TextMeshProUGUI GetText(int idx) { return Get<TextMeshProUGUI>(idx); }
        public Button GetButton(int idx) { return Get<Button>(idx); }
        public Image GetImage(int idx) { return Get<Image>(idx); }
        public RawImage GetRawImage(int idx) { return Get<RawImage>(idx); }
        public TMP_Dropdown GetDropdown(int idx) { return Get<TMP_Dropdown>(idx); }
        public ScrollRect GetScrollRect(int idx) { return Get<ScrollRect>(idx); }
        public Slider GetSlider(int idx) { return Get<Slider>(idx); }
    }
}
