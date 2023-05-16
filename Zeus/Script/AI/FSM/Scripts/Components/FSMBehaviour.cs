#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Zeus
{
    public sealed class FSMBehaviour : ScriptableObject
    {
       
        #region Public Variables      

        public string GraphName
        {
            get { return this.name; }
            set { this.name = value; }
        }
       
        [HideInInspector, SerializeField] public FSMState SelectedNode;
        [HideInInspector, SerializeField] public bool WantConnection = false;
        [HideInInspector, SerializeField] public TransitionPreview TransitionPreviewVariable = new TransitionPreview();
      
        [HideInInspector, SerializeField] public FSMState ConnectionNode;

        [HideInInspector, SerializeField] public bool ShowProperties;
        [HideInInspector, SerializeField] public List<FSMState> States;
        [HideInInspector, SerializeField] public Vector2 PanOffset;
        [HideInInspector, SerializeField] public bool OverNode = false;
        public struct TransitionPreview
        {
            public Rect TransitionRect;
            public bool? SideRight;
            public Action<FSMState> OnValidate;
            public FSMState State;
        }
        #endregion       

        void OnEnable()
        {
            if (States == null) States = new List<FSMState>();
#if UNITY_EDITOR
            ReloadChilds();
#endif
        }

#if UNITY_EDITOR       
        [HideInInspector, SerializeField]
        public List<Editor> Actions;
        [HideInInspector, SerializeField]
        public List<Editor> Decisions;
        public Texture Icon;
        public UnityEngine.Events.UnityAction<FSMState> OnSelectState;
        #region Main Methods

        public void UpdateGraphGUI(Event e, Rect viewRect, GUISkin viewSkin)
        {
            //if (nodes.Count > 0)
            {
               
                //Line For Clear AnnyState Actions and Entry Decisions and actions
                //if (States.Count > 1)
                //{
                //    States[0].Transitions.Clear();
                //    States[0].Actions.Clear();
                //    States[0].resetCurrentDestination = false;
                //    States[0].resetCurrentSpeed = false;
                //    States[1].resetCurrentDestination = false;
                //    States[1].resetCurrentSpeed = false;
                //    States[1].Actions.Clear();
                //}
                for (int i = 0; i < States.Count; i++)
                {
                    if (States[i] == null)
                    {
                        States.RemoveAt(i); break;
                    }
                  
                    States[i].UpdateNodeConnections(viewRect,e);
                }

                for (int i = 0; i < States.Count; i++)
                {
                    if (SelectedNode == null || SelectedNode != States[i])
                    {
                        States[i].UpdateNodeGUI(e, viewRect, viewSkin);
                    }
                }

                if (SelectedNode)
                {
                    SelectedNode.UpdateNodeGUI(e, viewRect, viewSkin);
                }
                if (WantConnection)
                {
                    //if(connectionNode!=null)
                    {
                        DrawConnectionToMouse(e.mousePosition);
                    }
                }
              //  GUILayout.EndArea();

                if (e.type == EventType.Layout)
                {
                    if (SelectedNode != null)
                    {
                        ShowProperties = true;
                    }
                }
            }
            //Lets Look for Connection Node
            EditorUtility.SetDirty(this);
        }

        public void ReloadChilds()
        {
            var data = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this.GetInstanceID()));
            if (Actions == null) Actions = new List<Editor>();
            if (Decisions == null) Decisions = new List<Editor>();
            Actions.Clear();
            Decisions.Clear();          
            foreach (var d in data)
            {
                if (d == null)
                {

                }
                else
                {
                    if (d.GetType().Equals(typeof(StateAction)) || d.GetType().IsSubclassOf(typeof(StateAction)))
                        Actions.Add(Editor.CreateEditor(d));

                    if (d.GetType().Equals(typeof(StateDecision)) || d.GetType().IsSubclassOf(typeof(StateDecision)))
                        Decisions.Add(Editor.CreateEditor(d));
                }
              
            }
        }

        [ContextMenu("Show Components")]
        void ShowComponents()
        {
            HideShowComponents();
        }

        [ContextMenu("Hide Components")]
        void HideComponents()
        {
            HideShowComponents(false);
        }
        [ContextMenu("Delete Unused States")]
        void DeleteUnusedComponents()
        {
            var data = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this.GetInstanceID()));

            for (int i = 0; i < data.Length; i++)
            {
                var d = data[i];
                if (d != null)
                {
                    if (d.GetType().Equals(typeof(FSMState)) || d.GetType().IsSubclassOf(typeof(FSMState)) || d is FSMState)
                        if (!States.Contains(d as FSMState))
                        {
                            Debug.Log("Delet " + d.name + " of " + this.name);
                            DestroyImmediate(d as FSMState,true);
                        }

                }

            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void HideShowComponents(bool show = true)
        {
            var data = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this.GetInstanceID()));

            foreach (var d in data)
            {        
                if(d!=null)
                {
                    if (d.GetType().Equals(typeof(FSMState)) || d.GetType().IsSubclassOf(typeof(FSMState)) || d is FSMState)
                        if (show) d.hideFlags = HideFlags.None; else d.hideFlags = HideFlags.HideInHierarchy;
                    if (d.GetType().Equals(typeof(StateAction)) || d.GetType().IsSubclassOf(typeof(StateAction)))
                        if (show) d.hideFlags = HideFlags.None; else d.hideFlags = HideFlags.HideInHierarchy;
                    if (d.GetType().Equals(typeof(StateDecision)) || d.GetType().IsSubclassOf(typeof(StateDecision)))
                        if (show) d.hideFlags = HideFlags.None; else d.hideFlags = HideFlags.HideInHierarchy;
                }
              
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void OnChangeChilds()
        {
            ReloadChilds();
        }

        public void InitGraph()
        {
            if (States.Count > 0)
            {               
                for (int i = 0; i < States.Count; i++)
                {
                    States[i].InitNode();
                }
            }
        }

        #endregion

        #region Utility Methods
        
        void ProcessEvents(Event e, Rect viewRect)
        {
            //if (viewRect.Contains(e.mousePosition))
            //{
            //    GUILayout.BeginArea(viewRect);
            //    if (e.button == 0)
            //    {
            //        if (e.type == EventType.mouseDown && !WantConnection || e.type == EventType.mouseUp && WantConnection)
            //        {
            //            showProperties = false;
            //            SelectNodes(e);
            //        }
            //    }
            //    GUILayout.EndArea();
            //}
        }

        void DrawConnectionToMouse(Vector2 mousePosition)
        {
            Handles.BeginGUI();
            {
                Rect a = TransitionPreviewVariable.TransitionRect;
                Rect b = new Rect(mousePosition.x, mousePosition.y, 0, 0);
                DrawNodeCurve(a, b, TransitionPreviewVariable.SideRight);
            }
            Handles.EndGUI();
        }

        void DrawNodeCurve(Rect start, Rect end, bool? sideRight)
        {
            Vector3 startPos = sideRight!=null? new Vector3(start.x + start.width, start.y + start.height / 2, 0): new Vector3(start.x + (start.width/2),start.y+(start.height/2),0);
            Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
            var magniture = Mathf.Clamp(((endPos - startPos).magnitude / 200f) - 0.5f, 0f, 1f);

            Vector3 startTan = startPos + (sideRight!=null?((bool)sideRight ? Vector3.right : Vector3.left) * (200 * magniture):Vector3.zero);
            Vector3 endTan = endPos + (sideRight != null ? ((bool)sideRight ? Vector3.left : Vector3.right) * (200 * magniture) : Vector3.zero);
            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 2);
        }

        public void SelectState(FSMState state)
        {
           
            if(OnSelectState!=null)
                OnSelectState.Invoke(state);
        }

        public void DeselectAll()
        {
            if (States.Count > 0)
            {
                for (int i = 0; i < States.Count; i++)
                {
                    States[i].IsSelected = false;
                    if (States[i] is FSMState)
                    {

                        var node = States[i] as FSMState;
                        node.SelectedDecisionIndex = 0;
                        for (int a = 0; a < node.Transitions.Count; a++)
                        {
                            node.Transitions[a].SelectedFalse = false;
                            node.Transitions[a].SelectedTrue = false;
                            node.SelectedDecisionIndex = 0;
                        }
                    }
                }
            }
        }

        public void DeselectAllExcludinCurrent()
        {
            if (States.Count > 0)
            {
                for (int i = 0; i < States.Count; i++)
                {
                    if (SelectedNode == null || SelectedNode != States[i])
                    {
                        States[i].IsSelected = false;
                        if (States[i] is FSMState)
                        {
                            var node = States[i] as FSMState;
                            node.SelectedDecisionIndex = 0;
                            for (int a = 0; a < node.Transitions.Count; a++)
                            {
                                node.Transitions[a].SelectedFalse = false;
                                node.Transitions[a].SelectedTrue = false;
                            }
                        }
                    }
                }
            }
        }

        public void ConnectToState(FSMState state)
        {
            if (WantConnection)
            {
                TransitionPreviewVariable.OnValidate.Invoke(state);
               
            }
            WantConnection = false;
        }

        public List<Type> GetRequiredTypes()
        {
            List < Type > types = new List<Type>();
            for (int i = 0; i < States.Count; i++)
            {
                if(States[i] != null)
                {
                    if (!types.Contains(States[i].RequiredType))
                        types.Add(States[i].RequiredType);
                    for (int a = 0; a < States[i].Actions.Count; a++)
                    {                       
                        if (States[i].Actions[a] != null && !types.Contains(States[i].Actions[a].RequiredType))
                            types.Add(States[i].Actions[a].RequiredType);                                                   
                    }
                    for (int a = 0; a < States[i].Transitions.Count; a++)
                    {
                        for (int b = 0; b < States[i].Transitions[a].Decisions.Count; b++)
                        {
                            if (States[i].Transitions[a] != null && States[i].Transitions[a].Decisions[b] != null && States[i].Transitions[a].Decisions[b].Decision != null)
                            {
                                if (!types.Contains(States[i].Transitions[a].Decisions[b].Decision.RequiredType))
                                    types.Add(States[i].Transitions[a].Decisions[b].Decision.RequiredType);
                            }
                        }
                    }
                }                
            }          
            return types;
        }
        #endregion

#endif
    }
}