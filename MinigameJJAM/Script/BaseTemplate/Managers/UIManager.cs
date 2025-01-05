using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io
{
    public class UIManager : BaseManager
    {
        #region Cache
        private const int BASE_ORDER = 10;

        public int Order { get; private set; } = BASE_ORDER;
        public SceneUI SceneUI { get; private set; }
        public GlobalUI GlobalUI { get; private set; }

        private List<PopupUI> _popupList = new List<PopupUI>();

        private PrefabObjectTable _uiTable;
        public bool DisableEscape { get; set; }
        #endregion

        #region Callback
        public event Action OnClosePopup;
        private event Action _onEscape;
        public event Action OnEscape
        {
            add
            {
                _onEscape = value;
            }
            remove
            {
                if (_onEscape == value)
                    _onEscape = null;
            }
        }
        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();
            _uiTable = Resources.Load<PrefabObjectTable>("UIPrefabTable");
            Manager.Load.OnSceneLoadProcessStarted += ClearCachedUI;
        }

        protected override void OnStart()
        {
            base.OnStart();
#if !UNITY_IOS
            Manager.Input.OnEscapeButton += EscapeProcess;
#endif
        }

        private GameObject _root;
        public GameObject Root
        {
            get
            {
                _root = GameObject.Find("UIRoot");
                if (_root == null)
                    _root = new GameObject { name = "UIRoot" };
                return _root;
            }
        }

        public void SetCanvas(GameObject obj, bool sort = true)
        {
            Canvas canvas = Util.GetOrAddComponent<Canvas>(obj);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;

            if (sort)
            {
                canvas.sortingOrder = Order;
                Order++;
            }
            else
            {
                canvas.sortingOrder = 0;
            }
            CanvasScaler canvasScaler = Util.GetOrAddComponent<CanvasScaler>(obj);
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(Define.REFERENCE_WIDTH, Define.REFERENCE_HEIGHT);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

        public void SetGlobalUI(GlobalUI globalUI) => GlobalUI = globalUI;

        public void SetSceneUI(SceneUI sceneUI) => SceneUI = sceneUI;

        private void ClearCachedUI()
        {
            _popupList.Clear();
            Order = BASE_ORDER;
        }

        #region Popup UI
        public T ShowPopupUI<T>(Transform parent = null, string popupSoundName = "popupSound") where T : PopupUI
        {
            var name = typeof(T).Name;
            Debug.Log($"Show Popup UI : {name}");

            if (!string.IsNullOrEmpty(popupSoundName))
                Manager.Sound.PlaySFX(popupSoundName);
            var matchTr = Root.transform.Find(name);
            if (matchTr != null)
            {
                T matchUI = matchTr.GetComponent<T>();
                return matchUI;
            }

            Transform root = parent == null ? Root.transform : parent;
            GameObject obj = _uiTable.GetPrefabObject(name, root);
            if (obj == null) return null;

            T popup = Util.GetOrAddComponent<T>(obj);
            _popupList.Add(popup);
            return popup;
        }

        public void CloseLastPopup()
        {
            if (_popupList.Count == 0) return;
            var popup = _popupList[_popupList.Count - 1];
            Destroy(popup.gameObject);
            _popupList.RemoveAt(_popupList.Count - 1);
            Order--;

            OnClosePopup?.Invoke();
            Util.UnfocusUI();
            Resources.UnloadUnusedAssets();
        }

        public void FindClosePopup<T>(T popupUI) where T : PopupUI
        {
            if (_popupList.Count == 0) return;
            var type = popupUI.GetType();
            var uiToRemove = _popupList.LastOrDefault(ui => ui.GetType() == type);
            if (uiToRemove != null)
            {
                Destroy(uiToRemove.gameObject);
                _popupList.Remove(uiToRemove);
                Debug.Log($"{uiToRemove.gameObject.name} of type {type.Name} is closed and removed.");
            }
            else
            {
                Debug.Log($"{popupUI.gameObject.name} is not present in the list");
            }
            Order--;

            OnClosePopup?.Invoke();
            Util.UnfocusUI();
            Resources.UnloadUnusedAssets();
        }

        public void ShowMessage(string message, Action callback = null, Vector2 frameSize = default)
        {
            var messageUI = ShowPopupUI<MessagePopupUI>();
            messageUI.SetMessageInfo(message, callback, frameSize);
        }
        #endregion

        #region ListUI
        public T GetListUI<T>(Transform parent) where T : ListUI
        {
            var name = typeof(T).Name;
            GameObject obj = _uiTable.GetPrefabObject(name, parent, false);
            if (obj == null) return null;

            T list = Util.GetOrAddComponent<T>(obj);
            return list;
        }
        #endregion

        #region VirtualController
        public VirtualController ShowOrGetVirtualController()
        {
            var name = typeof(VirtualController).Name;
            var matchTr = Root.transform.Find(name);
            if (matchTr != null)
            {
                var matchController = matchTr.GetComponent<VirtualController>();
                return matchController;
            }

            GameObject obj = _uiTable.GetPrefabObject(name, Root.transform);
            if (obj == null) return null;

            var controller = obj.GetComponent<VirtualController>();
            return controller;
        }
        #endregion

        private void EscapeProcess()
        {
            if (DisableEscape) return;

            if (_popupList.Count > 0)
                CloseLastPopup();
            else
                _onEscape?.Invoke();
        }
    }
}
