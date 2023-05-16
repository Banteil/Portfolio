using UnityEngine;

namespace Zeus
{
    [System.Serializable]
    public class FSMViewBase
    {
        // public event System.Action<Event> actions;
        #region Public Variables
        public string ViewTitle;
        public Rect ViewRect;
        #endregion

        #region Protected Variables
        protected GUISkin _viewSkin;
        protected FSMBehaviour _currentFSM;
        #endregion

        #region Constructors
        public FSMViewBase(string title)
        {
            this.ViewTitle = title;
        }

        public virtual void InitView()
        {
            GetEditorSkin();
        }
        #endregion

        #region Main Methods
        //public virtual void InitActionEvents()
        //{
        //    actions += a=> { if (a.type == EventType.mouseDown) { } };
        //}

        public virtual void UpdateView(Event e, FSMBehaviour curGraph)
        {
            if (_viewSkin == null)
            {
                GetEditorSkin();
                return;
            }
            // Set the current View Graph
            this._currentFSM = curGraph;
        }

        public virtual void ProcessEvents(Event e) { }
        #endregion

        #region Utility Methods
        protected void GetEditorSkin()
        {
            _viewSkin = (GUISkin)Resources.Load("GUISkins/EditorSkins/NodeEditorSkin");
        }
        #endregion
    }
}