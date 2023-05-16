#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Zeus
{
    [Serializable]
    public class FSMWorkView : FSMViewBase
    {

        #region Public Variables

        #endregion

        #region Protected Variables

        protected Vector2 mousePosition;
        protected bool indrag = false;
        protected int selectedNodeID = 0;
        protected bool inDrawRect = false;
        protected Rect selectorRect = new Rect();
        protected bool inDragNode;
        protected int selectedNodesCount;
        protected List<FSMState> selectedStates = new List<FSMState>();
        protected Texture backgroundTexture;

        #endregion

        #region Constructor
        public FSMWorkView() : base("Work View")
        {

        }
        #endregion        

        #region Main Methods
        public override void UpdateView(Event e, FSMBehaviour curGraph)
        {
            base.UpdateView(e, curGraph);

            if (!backgroundTexture) this.backgroundTexture = Resources.Load("grid") as Texture;
            var color = GUI.color;
            GUI.color = FSMBehaviourPreferences.gridBackgroundColor;
            if (!indrag)
            {
                if (curGraph)
                {
                    curGraph.PanOffset.x = curGraph.PanOffset.x.NearestRound(FSMHelper.DragSnap);
                    curGraph.PanOffset.y = curGraph.PanOffset.y.NearestRound(FSMHelper.DragSnap);
                }

            }
            GUI.Box(ViewRect, GUIContent.none);
            if (Event.current.type == EventType.Repaint)
            { // Draw Background when Repainting
              // Offset from origin in tile units

                Vector2 tileOffset = new Vector2(-(1 + (curGraph ? curGraph.PanOffset.x : 0)) / backgroundTexture.width,
                    ((1 + (curGraph ? curGraph.PanOffset.y : 0))) / backgroundTexture.height);
                // Amount of tiles
                Vector2 tileAmount = new Vector2(Mathf.Round(ViewRect.width * 1) / backgroundTexture.width,
                    Mathf.Round(ViewRect.height * 1) / backgroundTexture.height);
                // Draw tiled background
                GUI.color = FSMBehaviourPreferences.gridLinesColor;

                GUI.DrawTextureWithTexCoords(ViewRect, backgroundTexture, new Rect(tileOffset, tileAmount));
                if (curGraph == null)
                {
                    GUI.Label(ViewRect, "NO BEHAVIOUR", _viewSkin.GetStyle("ViewMessage"));
                }
            }

            GUI.color = color;

            if (inDrawRect)
            {
                GUI.Box(selectorRect, "", _viewSkin.GetStyle("SelectorArea"));
            }

            if (curGraph)
            {
                if (curGraph.OnSelectState == null) curGraph.OnSelectState = OnSelectState;
                curGraph.UpdateGraphGUI(e, ViewRect, _viewSkin);

            }
            #region Draw Work View Icons
            GUI.BeginGroup(ViewRect);

            GUI.color = new Color(1, 1, 1, 0.2f);
            GUI.DrawTexture(new Rect(ViewRect.width - 105, ViewRect.height - 105, 100, 100), Resources.Load("Textures/Editor/logo") as Texture2D);
            if (_currentFSM != null && _currentFSM.Icon != null)
                GUI.DrawTexture(new Rect(ViewRect.width - 105, ViewRect.y + 5, 100, 100), _currentFSM ? _currentFSM.Icon : null);
            GUI.color = color;
            #endregion
            GUI.EndGroup();
            GUI.Box(ViewRect, "", _viewSkin.GetStyle("BoxShadown"));
            ProcessEvents(e);
        }

        public override void ProcessEvents(Event e)
        {
            base.ProcessEvents(e);
            mousePosition = e.mousePosition;
            if (_currentFSM && ViewRect.Contains(mousePosition) || inDrawRect)
            {
                // GUILayout.BeginArea(ViewRect);

                if (e.button == 0 && !e.alt)
                {
                    if (e.type == EventType.MouseDrag && !_currentFSM.WantConnection)
                    {
                        if (_currentFSM.States.Count > 0)
                        {
                            for (int i = 0; i < _currentFSM.States.Count; i++)
                            {
                                if (_currentFSM.States[i].NodeRect.Contains(e.mousePosition))
                                {
                                    inDragNode = true;
                                }
                            }
                        }
                        if (inDragNode && !inDrawRect)
                        {
                            if (selectedStates.Count > 0)
                            {
                                for (int i = 0; i < selectedStates.Count; i++)
                                {
                                    if (selectedStates[i].IsSelected)
                                    {
                                        selectedStates[i].OnDrag(e.delta);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!inDrawRect)
                            {
                                inDrawRect = true;
                                selectorRect = new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0);


                            }
                            if (inDrawRect)
                            {
                                selectorRect.width += e.delta.x;
                                selectorRect.height += e.delta.y;
                                if (_currentFSM.States.Count > 0)
                                {

                                    for (int i = 0; i < _currentFSM.States.Count; i++)
                                    {

                                        if (selectorRect.Overlaps(_currentFSM.States[i].NodeRect, true))
                                        {
                                            if (!_currentFSM.States[i].IsSelected || !selectedStates.Contains(_currentFSM.States[i]))
                                            {
                                                _currentFSM.States[i].IsSelected = true;
                                                if (!selectedStates.Contains(_currentFSM.States[i]))
                                                    selectedStates.Add(_currentFSM.States[i]);
                                            }

                                        }
                                        else
                                        {
                                            _currentFSM.States[i].IsSelected = false;
                                            if (selectedStates.Contains(_currentFSM.States[i]))
                                                selectedStates.Remove(_currentFSM.States[i]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (e.type == EventType.MouseDown)
                    {
                        if (_currentFSM != null)
                        {
                            _currentFSM.OverNode = false;
                            if (_currentFSM.States.Count > 0 && !inDragNode && !inDrawRect)
                            {
                                for (int i = 0; i < _currentFSM.States.Count; i++)
                                {

                                    if (_currentFSM.States[i].NodeRect.Contains(e.mousePosition))
                                    {
                                        _currentFSM.OverNode = true;
                                        {
                                            selectedNodeID = i;
                                            if (!e.shift && _currentFSM.States[i].IsSelected == false)
                                            {
                                                if (_currentFSM.SelectedNode) _currentFSM.SelectedNode.IsSelected = false;
                                                Selection.activeObject = _currentFSM.States[i];
                                                _currentFSM.States[i].IsSelected = true;
                                                _currentFSM.SelectedNode = _currentFSM.States[i];
                                                selectedStates.Clear();
                                                selectedStates.Add(_currentFSM.States[i]);
                                                _currentFSM.DeselectAllExcludinCurrent();
                                                selectedNodesCount = 1;
                                            }

                                            else if (e.shift && !selectedStates.Contains(_currentFSM.States[i]))
                                            {
                                                _currentFSM.States[i].IsSelected = true;
                                                selectedStates.Add(_currentFSM.States[i]);
                                                selectedNodesCount++;
                                            }
                                            else
                                            {
                                                Selection.activeObject = _currentFSM.States[i];
                                            }
                                            e.Use();
                                        }
                                        break;
                                    }

                                }
                                if (!_currentFSM.OverNode && !inDragNode && !inDrawRect)
                                {
                                    _currentFSM.DeselectAll();
                                }
                            }
                            if (!_currentFSM.OverNode)
                            {
                                Selection.activeGameObject = null;
                            }
                        }
                    }
                    else if (e.type == EventType.MouseUp)
                    {

                        _currentFSM.OverNode = false;
                        if (_currentFSM != null)
                        {

                            if (_currentFSM.States.Count > 0 && !inDragNode && !inDrawRect)
                            {
                                for (int i = 0; i < _currentFSM.States.Count; i++)
                                {
                                    if (_currentFSM.States[i] != _currentFSM.TransitionPreviewVariable.State && _currentFSM.States[i].NodeRect.Contains(e.mousePosition))
                                    {
                                        _currentFSM.OverNode = true;
                                        if (_currentFSM.WantConnection)
                                        {
                                            _currentFSM.ConnectToState(_currentFSM.States[i]);
                                        }

                                        break;
                                    }
                                }
                            }

                            if (!_currentFSM.OverNode && !inDragNode && !inDrawRect)
                            {
                                _currentFSM.WantConnection = false;
                            }
                        }
                        if (inDragNode)
                        {
                            inDragNode = false;
                            if (selectedStates.Count > 0)
                            {
                                for (int i = 0; i < selectedStates.Count; i++)
                                {
                                    if (selectedStates[i].IsSelected)
                                    {
                                        selectedStates[i].OnEndDrag();
                                    }
                                }
                            }
                        }

                        inDrawRect = false;

                    }
                }
                if (e.button == 1 && !e.alt)
                {
                    if (e.type == EventType.MouseUp)
                    {
                        _currentFSM.OverNode = false;
                        if (_currentFSM != null)
                        {
                            if (_currentFSM.States.Count > 0)
                            {
                                for (int i = 0; i < _currentFSM.States.Count; i++)
                                {
                                    if (_currentFSM.States[i].NodeRect.Contains(e.mousePosition))
                                    {
                                        _currentFSM.OverNode = true; break;
                                    }
                                }
                            }
                        }

                    }

                    if (e.type == EventType.MouseDown)
                    {
                        _currentFSM.OverNode = false;

                        var validNode = false;
                        if (_currentFSM != null)
                        {
                            if (_currentFSM.States.Count > 0)
                            {
                                for (int i = 0; i < _currentFSM.States.Count; i++)
                                {
                                    if (_currentFSM.States[i].NodeRect.Contains(e.mousePosition))
                                    {
                                        if (_currentFSM.States[i] is FSMState) validNode = true;
                                        selectedNodeID = i;
                                        _currentFSM.OverNode = true; break;
                                    }
                                }
                            }
                        }
                        if (validNode || !_currentFSM.OverNode)
                            ProcessContexMenu(e, _currentFSM.OverNode ? 1 : 0);
                    }


                }
                if (e.button == 2 || e.alt && e.button == 0)
                {
                    if (e.type == EventType.MouseDrag)
                    {
                        if (_currentFSM != null)
                        {
                            if (_currentFSM.States.Count > 0)
                            {
                                _currentFSM.OverNode = false;
                                for (int i = 0; i < _currentFSM.States.Count; i++)
                                {
                                    if (_currentFSM.States[i].NodeRect.Contains(e.mousePosition))
                                    {
                                        selectedNodeID = i;
                                        _currentFSM.OverNode = true; break;
                                    }
                                }
                            }
                        }
                        if (!_currentFSM.OverNode)
                        {
                            indrag = true;
                            var delta = e.delta;// RoundedDelta(e.delta);
                            _currentFSM.PanOffset.x += delta.x;
                            _currentFSM.PanOffset.y += delta.y;
                            for (int i = 0; i < _currentFSM.States.Count; i++)
                            {

                                _currentFSM.States[i].OnDrag(delta, false);
                            }
                        }
                    }
                    if (e.type == EventType.MouseUp && indrag)
                    {
                        indrag = false;
                        for (int i = 0; i < _currentFSM.States.Count; i++)
                        {

                            _currentFSM.States[i].OnEndDrag();
                        }
                    }



                }
                if (e.isKey && e.keyCode == KeyCode.F)
                {
                    CenterView(_currentFSM);
                }
                if (indrag)
                    EditorGUIUtility.AddCursorRect(new Rect(e.mousePosition.x - 50, e.mousePosition.y - 50, 100, 100), MouseCursor.Pan);
            }
            else if (_currentFSM)
            {
                GUILayout.BeginArea(ViewRect);
                _currentFSM.OverNode = false;

                GUILayout.EndArea();
                if (e.type == EventType.MouseUp && ViewRect.Contains(e.mousePosition))
                {
                    inDrawRect = false;
                    inDragNode = false;
                }
            }
        }

        public void CenterView(FSMBehaviour behaviour)
        {
            behaviour.PanOffset.x = behaviour.States[0].NodeRect.x;
            behaviour.PanOffset.y = behaviour.States[0].NodeRect.y;
            var x = behaviour.States[0].NodeRect.x;
            var y = behaviour.States[0].NodeRect.y;
            Vector2 position = new Vector2(x, y);
            behaviour.States[0].NodeRect.x = this.ViewRect.position.x + this.ViewRect.width / 2;
            behaviour.States[0].NodeRect.y = 100;
            if (behaviour.States.Count > 1)
            {
                for (int i = 1; i < behaviour.States.Count; i++)
                {
                    var diferencePosition = behaviour.States[i].NodeRect.position - position;
                    var newPosition = behaviour.States[0].NodeRect.position + diferencePosition;
                    behaviour.States[i].NodeRect.position = newPosition;
                }
            }
        }

        #endregion

        #region Utility Methods
        void ProcessContexMenu(Event e, int contexID)
        {
            GenericMenu menu = new GenericMenu();
            if (contexID == 0)
            {
                if (_currentFSM != null)
                {
                    var possibleStates = FindSubClassesOfNode<FSMState>().ToList();
                    menu.AddItem(new GUIContent($"New State"), false, () => { NodeUtility.CreateNode(_currentFSM, mousePosition); });
                    for (int i = 0; i < possibleStates.Count; i++)
                    {
                        Type type = possibleStates[i];
                        string name = possibleStates[i].Name;
                        menu.AddItem(new GUIContent($"New {name}"), false, () => { NodeUtility.CreateNode(type, _currentFSM, mousePosition); });
                    }

                }
            }
            if (contexID == 1)
            {
                if (_currentFSM != null)
                {
                    Undo.RegisterCompleteObjectUndo(_currentFSM, "Delete Node");
                    Undo.RecordObjects(_currentFSM.States.ToArray(), "Delete Nodes");
                    if (_currentFSM.States[selectedNodeID].UseDecisions)
                        menu.AddItem(new GUIContent("New Transition"), false, () => { _currentFSM.States[selectedNodeID].AddNewTransition(); });
                    if (_currentFSM.States[selectedNodeID].CanSetAsDefault)
                        menu.AddItem(new GUIContent("Set as Default Node"), false, () => { _currentFSM.States[0].DefaultTransition = _currentFSM.States[selectedNodeID] as FSMState; });
                    if ((selectedStates.Count <= 1 && _currentFSM.States[selectedNodeID].CanRemove) || (selectedStates.Count > 1 && selectedStates.Find(s => s.CanRemove)))
                        menu.AddItem(new GUIContent("Delete Node"), false, () => { if (_currentFSM.States[selectedNodeID].CanRemove) NodeUtility.DeleteNode(selectedNodeID, _currentFSM); foreach (var state in selectedStates) if (state.CanRemove) NodeUtility.DeleteNode(_currentFSM.States.IndexOf(state), _currentFSM); });

                    menu.ShowAsContext();
                }
            }
            menu.ShowAsContext();
            e.Use();

        }
        void OnSelectState(FSMState state)
        {
            Event e = Event.current;
            if (!e.shift && !inDrawRect)
                state.ParentGraph.DeselectAll();
            if (!state.IsSelected)
                state.IsSelected = true;
            if (Selection.activeObject != state) Selection.activeObject = state;
            if (!selectedStates.Contains(state))
            {
                selectedStates.Add(state);
            }
        }
        public Vector2 offset;
        public IEnumerable<Type> FindSubClassesOfNode<T>()
        {
            IEnumerable<Type> exporters = typeof(T)
             .Assembly.GetTypes()
             .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract);
            return exporters;
        }
        #endregion
    }
}