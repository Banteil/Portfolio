using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace starinc.io.kingnslave
{
    public class UIManager : Singleton<UIManager>
    {
        private int order = 10;
        public int Order { get { return order; } set { order = value; } }

        private List<UIBase> uiList = new List<UIBase>();

        private int LastIndex { get { return uiList.Count - 1; } }

        private UIGlobal globalUI;
        public UIGlobal GlobalUI { get { return globalUI; } }

        private GameObject connectingUI; 

        protected override void OnAwake()
        {
            base.OnAwake();
            SceneManager.sceneUnloaded += ClearUIStack;
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
                canvas.sortingOrder = order;
                order++;
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

        public void ShowWarningUI(string stringData, bool isLocaleKey = true, Action callback = null)
        {
            var warningUI = ShowUI<UIWarning>("WarningUI", callback);
            warningUI.SetInfomationText(isLocaleKey ? Util.GetLocalizationTableString(Define.InfomationLocalizationTable, stringData) : stringData);
        }

        public void SetGlobalUI(GameObject obj)
        {
            var globalUI = Util.GetOrAddComponent<UIGlobal>(obj);
            this.globalUI = globalUI;
        }

        public UIUserProfile ShowUserProfile(UserData userData)
        {
            if(userData == null)
            {
                Debug.LogError("유저 데이터가 null이어서 유저 프로필 창을 표시할 수 없습니다.");
                return null;
            }
            var userProfile = ShowUI<UIUserProfile>("UserProfileUI");
            userProfile.SetUserData(userData);
            return userProfile;
        }

        async public void ShowUserProfile(string sid)
        {
            if (string.IsNullOrEmpty(sid))
            {
                return;
            }

            var userData = new UserData();
            await CallAPI.APISelectUser(UserDataManager.Instance.MySid, sid, (data) =>
            {
                userData = data;
            });

            if (userData == null)
            {
                Debug.LogError("유저 데이터가 null이어서 유저 프로필 창을 표시할 수 없습니다.");
                return;
            }
            var userProfile = ShowUI<UIUserProfile>("UserProfileUI");
            userProfile.SetUserData(userData);
        }

        public T ShowUI<T>(string name = null, Action callback = null, Transform parent = null) where T : UIBase
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            var matchTr = Root.transform.Find(name);
            if (matchTr != null)
            {
                T matchUI = matchTr.GetComponent<T>();
                return matchUI;
            }

            Transform root = parent == null ? Root.transform : parent;
            GameObject obj = ResourceManager.Instance.Instantiate(name, root);
            if (obj == null) return null;

            T popup = Util.GetOrAddComponent<T>(obj);
            uiList.Add(popup);
            popup.SetCallback(callback);
            return popup;
        }

        public void FindCloseUI(UIBase uiBase, Action callback = null)
        {
            if (uiList.Count == 0) return;
            callback?.Invoke();

            var index = uiList.IndexOf(uiBase);
            if (index == -1)
            {
                Debug.Log($"{uiBase.gameObject.name} UI를 종료하는데 문제가 발생하였습니다.");
                return;
            }

            var ui = uiList[index];
            if (ui != null)
            {
                Destroy(ui.gameObject);
                uiList.RemoveAt(index);
            }
            order--;
            Resources.UnloadUnusedAssets();
        }

        public void CloseUI(Action callback = null)
        {
            if (uiList.Count <= 0) return;
            callback?.Invoke();

            var ui = uiList[LastIndex];
            if (ui != null)
            {
                Destroy(ui.gameObject);
                uiList.RemoveAt(LastIndex);
            }
            order--;
            Resources.UnloadUnusedAssets();
        }

        public void CloseRangeUI(int range)
        {
            for (int i = 0; i < range; i++)
            {
                if (uiList.Count == 0) break;
                CloseUI();
            }
        }

        public void CloseAllUI()
        {
            while (uiList.Count > 0) CloseUI();
        }

        private void ClearUIStack(Scene scene)
        {
            uiList.Clear();
            order = 10;
        }

        public T AddListUI<T>(Transform parent, string name = null, int index = 0) where T : UIList
        {
            if (string.IsNullOrEmpty(name))
                name = typeof(T).Name;

            GameObject obj = ResourceManager.Instance.Instantiate(name, parent, false);
            if (obj == null) return null;

            T list = Util.GetOrAddComponent<T>(obj);
            list.SetIndex(index);
            return list;
        }

        public void ShowConnectingUI()
        {
            CloseConnectingUI();
            connectingUI = Instantiate(ResourceManager.Instance.ConnectingUI, transform, false);
        }

        public void CloseConnectingUI()
        {
            if (connectingUI != null)
            {
                Destroy(connectingUI);
                connectingUI = null;
            }
        }

        public UIYesOrNo ShowYesOrNoUI(string info, Action callback = null)
        {              
            var ynUI = ShowUI<UIYesOrNo>("YesOrNoUI", callback);
            if(ynUI != null)
                ynUI.SetInfomationText(info);
            return ynUI;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (uiList.Count > 0)
                {
                    uiList[LastIndex].InputEscape();
                }
                else
                    Util.ExitProcess(SceneManager.GetActiveScene().name);
            }
        }
    }
}
