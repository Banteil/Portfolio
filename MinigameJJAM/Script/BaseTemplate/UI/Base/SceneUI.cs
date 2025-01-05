using System;
using UnityEngine;

namespace starinc.io
{
    public class SceneUI : BaseUI
    {
        #region Cache
        protected const int SCENE_ORDER = 5;

        [SerializeField]
        protected SoundResourceTable _sceneSoundTable;
        public SoundResourceTable SoundTable { get { return _sceneSoundTable; } }

        [SerializeField]
        protected bool _isAddressableScene = true;
        public bool IsAddressableScene { get { return _isAddressableScene; } }
        #endregion

        protected override void OnAwake()
        {
            SceneInitialization();
            Manager.UI.OnEscape += EscapeAction;
        }

        protected virtual void SceneInitialization()
        {
            var uiManager = Manager.UI;
            uiManager.SetCanvas(gameObject, false);
            uiManager.SetSceneUI(this);
            UICanvas.sortingOrder = SCENE_ORDER;
        }

        protected override void OnDestroy()
        {
            ReleaseSceneResources();
        }

        protected virtual void ReleaseSceneResources() { }

        protected virtual void EscapeAction() { }
    }
}
