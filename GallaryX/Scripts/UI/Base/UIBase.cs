using starinc.io.gallaryx;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace starinc.io
{
    public class UIBase : MonoBehaviour, IPointerDownHandler
    {
        protected bool _isLandscape;
        public Canvas UICanvas { get; private set; }
        protected Dictionary<Type, UnityEngine.Object[]> _objectsDic = new Dictionary<Type, UnityEngine.Object[]>();

        private void Awake()
        {
            _isLandscape = Util.IsLandscape;
            UICanvas = gameObject.GetComponent<Canvas>();            
            OnAwake();
            ChangeReferenceResolution(_isLandscape);
        }

        protected virtual void OnAwake() { }

        private void Start() => OnStart();

        protected virtual void OnStart() { }

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

        protected T Get<T>(int idx) where T : UnityEngine.Object
        {
            UnityEngine.Object[] objects = null;
            if (!_objectsDic.TryGetValue(typeof(T), out objects))
                return null;

            return objects[idx] as T;
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
            UIEventHandler evt = Util.GetOrAddComponent<UIEventHandler>(obj);
            evt.OnClickHandler = null;
            evt.OnDragHandler = null;
            evt.OnDownHandler = null;
            evt.OnUpHandler = null;
        }

        protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
        protected TextMeshProUGUI GetText(int idx) { return Get<TextMeshProUGUI>(idx); }
        protected Button GetButton(int idx) { return Get<Button>(idx); }
        protected Image GetImage(int idx) { return Get<Image>(idx); }
        protected RawImage GetRawImage(int idx) { return Get<RawImage>(idx); }
        protected TMP_Dropdown GetDropdown(int idx) { return Get<TMP_Dropdown>(idx); }
        public ScrollRect GetScrollRect(int idx) { return Get<ScrollRect>(idx); }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            UIManager.Instance.IsUIClick = true;
        }

        protected virtual void ScreenOrientationChanged(bool isLandscape)
        {            
            ChangeReferenceResolution(isLandscape);
            //자식 별로 기능 구현//
        }

        private void ChangeReferenceResolution(bool isLandscape)
        {
            var canvasScaler = gameObject.GetComponent<CanvasScaler>();
            if (canvasScaler == null) return;
            canvasScaler.referenceResolution = isLandscape ? new Vector2(1920, 1080) : new Vector2(1080, 1920);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (UICanvas == null || !Util.IsMobileWebPlatform) return;
            if (_isLandscape == Util.IsLandscape) return;
            _isLandscape = Util.IsLandscape;
            ScreenOrientationChanged(_isLandscape);
        }

        protected virtual void OnDestroy() { }
    }
}
