using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace starinc.io.gallaryx
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private Image _fadeImage;
        [SerializeField] private Canvas _hoveringCanvas;
        private GameObject _zoomInUI;

        private int _order = 10;
        public int Order { get { return _order; } }
        private List<UIPopup> _popupList = new List<UIPopup>();
        private UIScene _sceneUI;
        public UIScene SceneUI { get { return _sceneUI; } }
        private UIGlobal _globalUI;
        public UIGlobal GlobalUI { get { return _globalUI; } }

        public bool InteractUI { get; set; }

        public bool IsUIClick { get; set; }

        private AfterHideUI _afterHideUI;
        private bool _isHide = false;

        protected override void OnAwake()
        {
            base.OnAwake();
            _afterHideUI = gameObject.GetComponentInChildren<AfterHideUI>();
        }

        private void Start()
        {  
            InputManager.Instance.KeyInputEvent.InputUpCallback += () =>
            {
                if (InputManager.Instance.KeyInputEvent.InputEscapeKey() == InputState.Up)
                    GetLastPopup()?.ClosePopup();
            };

            InputManager.Instance.KeyInputEvent.InputUpCallback += HideUI;
        }

        public GameObject Root
        {
            get
            {
                GameObject root = GameObject.Find("UIRoot");
                if (root == null)
                    root = new GameObject { name = "UIRoot" };
                return root;
            }
        }

        public void SetCanvas(GameObject obj, bool sort = true)
        {
            Canvas canvas = Util.GetOrAddComponent<Canvas>(obj);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;

            if (sort)
            {
                canvas.sortingOrder = canvas.sortingOrder + _order;
                _order++;
            }
            else
            {
                canvas.sortingOrder = 0;
            }

            CanvasScaler canvasScaler = Util.GetOrAddComponent<CanvasScaler>(obj);
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

        public T ShowSceneUI<T>(string name = null, Transform parent = null) where T : UIScene
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            if (Root.transform.Find(name) != null) return null;

            Transform root = parent == null ? Root.transform : parent;
            GameObject obj = ResourceManager.Instance.Instantiate($"UI/Scene/{name}", root);
            if (obj == null) return null;

            T sceneUI = Util.GetOrAddComponent<T>(obj);
            this._sceneUI = sceneUI;
            return sceneUI;
        }

        public T ShowGlobalUI<T>(string name = null, Transform parent = null) where T : UIGlobal
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            Transform root = parent == null ? Root.transform : parent;
            GameObject obj = ResourceManager.Instance.Instantiate($"UI/Global/{name}", root);
            if (obj == null) return null;

            T globalUI = Util.GetOrAddComponent<T>(obj);
            this._globalUI = globalUI;
            return globalUI;
        }

        public T ShowPopupUI<T>(string name = null, Action callback = null, Transform parent = null) where T : UIPopup
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            Transform root = parent == null ? Root.transform : parent;
            var matchTr = root.Find(name);
            if (matchTr != null)
            {
                var matchPopup = matchTr.GetComponent<T>();
                return matchPopup;
            }

            GameObject obj = ResourceManager.Instance.Instantiate($"UI/Popup/{name}", root);
            if (obj == null) return null;

            T popup = Util.GetOrAddComponent<T>(obj);
            _popupList.Add(popup);
            popup.SetCloseCallback(callback);
            return popup;
        }

        public void CloseSpecificPopupUI<T>(Action callback = null) where T : UIPopup
        {
            if (_popupList.Count == 0) return;
            var popupIndex = _popupList.FindIndex(x => x is T);
            if (popupIndex == -1) return;
            var popup = _popupList[popupIndex];
            ResourceManager.Instance.Destroy(popup.gameObject);
            _order--;
            _popupList.RemoveAt(popupIndex);

            callback?.Invoke();
            Util.UnfocusUI();
        }

        public void CloseLastPopupUI(Action callback = null)
        {
            if (_popupList.Count == 0) return;
            var popup = _popupList[_popupList.Count - 1];
            ResourceManager.Instance.Destroy(popup.gameObject);
            _order--;
            _popupList.RemoveAt(_popupList.Count - 1);

            callback?.Invoke();
            Util.UnfocusUI();
        }

        public void AddPopupUI<T>(T popup) where T : UIPopup
        {
            _popupList.Add(popup);
        }

        public bool RemovePopupUI<T>() where T : UIPopup
        {
            if (_popupList.Count == 0) return false;
            var popupIndex = _popupList.FindIndex(x => x is T);
            if (popupIndex == -1) return false;

            _popupList.RemoveAt(popupIndex);
            Util.UnfocusUI();
            return true;
        }

        public void CloseAllPopupUI()
        {
            while (_popupList.Count > 0) CloseLastPopupUI();
        }

        public UIPopup GetLastPopup() => _popupList.Count > 0 ? _popupList[_popupList.Count - 1] : null;

        public List<UIPopup> GetPopupList() => _popupList;

        public T AddListUI<T>(int index, Transform parent, string name = null) where T : UIList
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            GameObject obj = ResourceManager.Instance.Instantiate($"UI/List/{name}", parent, false);
            if (obj == null) return null;

            T list = Util.GetOrAddComponent<T>(obj);
            list.SetIndex(index);
            return list;
        }

        public void FadeOut(float time, Action callback = null)
        {
            InteractUI = true;
            _fadeImage.raycastTarget = true;
            _fadeImage.DOFade(1f, time)
            .SetEase(Ease.InQuad) // 어두워짐. 알파 값 조정.
            .OnComplete(() => // 실행 후.
            {
                callback?.Invoke();
            });
        }

        public void FadeIn(float time, Action callback = null)
        {
            _fadeImage.DOFade(0f, time)
            .SetEase(Ease.InQuad)
            .OnComplete(() => // 실행 후.
            {
                _fadeImage.raycastTarget = false;
                InteractUI = false;
                callback?.Invoke();
            });
        }

        public void ActiveZoomInUI(bool isActive, Transform exhibitsTr = null)
        {
            if ((isActive && exhibitsTr == null) || Util.IsMobileWebPlatform) return;

            if(isActive)
            {
                if (_zoomInUI != null) return;
                _zoomInUI = Instantiate(Resources.Load<GameObject>($"Prefabs/UI/ZoomInSprite"));
                var pos = exhibitsTr.position;
                pos += exhibitsTr.forward * 0.05f;
                _zoomInUI.transform.position = pos;
                _zoomInUI.transform.rotation = exhibitsTr.rotation;
            }
            else
            {
                if (_zoomInUI != null)
                {
                    Destroy(_zoomInUI);
                    _zoomInUI = null;
                }
            }
        }

        public GameObject CreateHoveringUI()
        {
            var obj = Instantiate(ResourceManager.Instance.Load<GameObject>($"Prefabs/UI/HoveringUI"), _hoveringCanvas.transform, false);
            return obj;
        }

        private void HideUI()
        {
            if (Util.IsUIFocusing) return;
            if (InputManager.Instance.KeyInputEvent.InputHideUIKey() == InputState.Up)
            {
                HideUIProcess();
            }
        }

        public void HideUIProcess()
        {
            _isHide = !_isHide;
            _globalUI.UICanvas.enabled = !_isHide;
            _sceneUI.UICanvas.enabled = !_isHide;
            for (int i = 0; i < _popupList.Count; i++)
            {
                _popupList[i].UICanvas.enabled = !_isHide;
            }

            if(Util.IsMobileWebPlatform)
            {
                InputManager.Instance.MobileVirtualController.InactiveRunButton();
                InputManager.Instance.MobileVirtualController.UICanvas.enabled = !_isHide;
            }

            _afterHideUI.HideUIProcess(_isHide);
        }        
    }
}
