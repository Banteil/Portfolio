using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Zeus
{
    public class FSMPropertyView : FSMViewBase
    {
        public FSMPropertyView() : base("FSM Property View") { }
        public string[] toolBar = new string[] { "Parameters", "Components" };
        public int selected;
        public Vector2 componentsScrollView, parametersScrollView;
        public SerializedObject curGraphSerialized;

        public override void UpdateView(Event e, FSMBehaviour curGraph)
        {
            if ((!Application.isPlaying || Selection.activeGameObject == null) && curGraph)
            {
                if (curGraphSerialized == null || curGraphSerialized.targetObject != curGraph) curGraphSerialized = new SerializedObject(curGraph);
            }
            else if (Application.isPlaying && Selection.activeGameObject != null)
            {
                var fsmBehaviour = Selection.activeGameObject.GetComponent<IFSMBehaviourController>();
                if (fsmBehaviour != null)
                {
                    List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>();
                    Selection.activeGameObject.GetComponents<MonoBehaviour>(monoBehaviours);
                    if (monoBehaviours.Count > 0)
                    {
                        var monoFSM = monoBehaviours.Find(m => m is IFSMBehaviourController);
                        if (monoFSM) curGraphSerialized = new SerializedObject(monoFSM);
                        else if (curGraphSerialized == null || curGraphSerialized.targetObject != curGraph) curGraphSerialized = new SerializedObject(curGraph);
                    }

                }
                else if (curGraph)
                {
                    if (curGraphSerialized == null || curGraphSerialized.targetObject != curGraph) curGraphSerialized = new SerializedObject(curGraph);
                }
            }
            else if (curGraph)
            {
                if (curGraphSerialized == null || curGraphSerialized.targetObject != curGraph) curGraphSerialized = new SerializedObject(curGraph);
            }
            base.UpdateView(e, curGraph);
            GUI.Box(ViewRect, "", _viewSkin.GetStyle("ToolBar"));
            var toolbarRect = ViewRect;
            toolbarRect.height = 20;
            // selected = GUI.Toolbar(toolbarRect,selected, toolBar,viewSkin.GetStyle("ToolBar"));
            var rectoffset = ViewRect;
            // rectoffset.y += 20;
            rectoffset.x += 5;
            rectoffset.width -= 10;
            rectoffset.height -= 20;
            GUILayout.BeginArea(rectoffset);
            {
                if (curGraph)
                {
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal(_viewSkin.box, GUILayout.Height(20));
                    GUILayout.Label("FSM COMPONENTS", _viewSkin.GetStyle("LabelHeader"));
                    if (GUILayout.Button("+", _viewSkin.box, GUILayout.Width(20), GUILayout.ExpandHeight(true)))
                    {
                        var rect = GUILayoutUtility.GetLastRect();
                        AddComponentContext(curGraph, rect);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    componentsScrollView = GUILayout.BeginScrollView(componentsScrollView);
                    GUILayout.Box("", _viewSkin.GetStyle("Separator"), GUILayout.Height(2), GUILayout.ExpandWidth(true));
                    GUILayout.Space(10);
                    GUILayout.BeginVertical(_viewSkin.box);
                    GUILayout.Label("Actions", _viewSkin.GetStyle("LabelHeader"), GUILayout.ExpandWidth(true)); GUILayout.Space(5);
                    if (curGraph.Actions.Count > 0)
                    {
                        for (int a = 0; a < curGraph.Actions.Count; a++)
                        {
                            if (curGraph.Actions[a])
                            {
                                curGraph.Actions[a].OnInspectorGUI();
                                var rect = GUILayoutUtility.GetLastRect();
                                rect.height = 15;
                                if (rect.Contains(e.mousePosition) && e.type != EventType.ContextClick)
                                {
                                    if (e.button == 1)
                                    {
                                        GenericMenu menu = new GenericMenu();
                                        int index = a;
                                        var refList = curGraph.Actions;
                                        var action = curGraph.Actions[a].target as StateAction;
                                        menu.AddItem(new GUIContent("Delete"), false, () => { DeleteObjet(ref refList, index, GetStatesUsingAction(action)); e.Use(); });
                                        menu.ShowAsContext();
                                    }
                                }

                                var tooltip = GetStatesUsingAction(curGraph.Actions[a].target as StateAction);
                                bool isInUse = !String.IsNullOrEmpty(tooltip);
                                var _color = GUI.color;
                                GUI.color = isInUse ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 0.1f);
                                if (!isInUse)
                                {
                                    tooltip = "Not being used";
                                }
                                var _checkerRect = rect;
                                _checkerRect.x = ViewRect.width - 25 + componentsScrollView.x;
                                _checkerRect.width = 10;
                                _checkerRect.height = 10;
                                GUI.Toggle(_checkerRect, false, new GUIContent("", tooltip), EditorStyles.radioButton);
                                GUI.color = _color;
                            }
                            else
                            {
                                curGraph.Actions.RemoveAt(a);
                            }
                        }
                    }
                    else GUILayout.Box("NONE", _viewSkin.box, GUILayout.ExpandWidth(true));
                    GUILayout.EndVertical();
                    GUILayout.Space(10);
                    GUILayout.Box("", _viewSkin.GetStyle("Separator"), GUILayout.Height(2), GUILayout.ExpandWidth(true));
                    GUILayout.Space(10);

                    GUILayout.BeginVertical(_viewSkin.box);
                    GUILayout.Box("Decisions", _viewSkin.GetStyle("LabelHeader"), GUILayout.ExpandWidth(true)); GUILayout.Space(5);
                    if (curGraph.Decisions.Count > 0)
                    {
                        for (int a = 0; a < curGraph.Decisions.Count; a++)
                        {
                            if (curGraph.Decisions[a])
                            {
                                curGraph.Decisions[a].OnInspectorGUI();
                                var rect = GUILayoutUtility.GetLastRect();
                                rect.height = 15;
                                if (rect.Contains(e.mousePosition) && e.type != EventType.ContextClick)
                                {
                                    if (e.button == 1)
                                    {
                                        GenericMenu menu = new GenericMenu();
                                        int index = a;
                                        var refList = curGraph.Decisions;
                                        var decision = curGraph.Decisions[a].target as StateDecision;
                                        menu.AddItem(new GUIContent("Delete"), false, () => { DeleteObjet(ref refList, index, GetStatesUsingDecision(decision)); e.Use(); });
                                        menu.ShowAsContext();
                                    }
                                }
                                var tooltip = GetStatesUsingDecision(curGraph.Decisions[a].target as StateDecision);
                                bool isInUse = !String.IsNullOrEmpty(tooltip);
                                var _color = GUI.color;
                                GUI.color = isInUse ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 0.1f);
                                if (!isInUse)
                                {
                                    tooltip = "Not being used";
                                }
                                var _checkerRect = rect;
                                _checkerRect.x = ViewRect.width - 25 + componentsScrollView.x;
                                _checkerRect.width = 10;
                                _checkerRect.height = 10;
                                GUI.Toggle(_checkerRect, false, new GUIContent("", tooltip), EditorStyles.radioButton);
                                GUI.color = _color;
                            }
                            else
                            {
                                curGraph.Decisions.RemoveAt(a);
                                break;
                            }
                        }
                    }
                    else GUILayout.Box("NONE", _viewSkin.box, GUILayout.ExpandWidth(true));
                    GUILayout.EndVertical();

                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndArea();
        }

        string GetStatesUsingAction(StateAction action)
        {
            if (!action) return string.Empty;
            var states = _currentFSM.States.FindAll(state => state != null && state.Actions != null && state.Actions.Exists(a => a != null && a.Equals(action)));
            if (states == null || states.Count == 0) return string.Empty;
            StringBuilder text = new StringBuilder();


            text.Append(action.name + " is being used in " + states.Count + " state(s)\n");
            for (int i = 0; i < states.Count; i++)
            {
                text.Append(states[i].Name);
                if (i < states.Count - 1) text.Append(" , ");
            }

            return text.ToString();
        }

        string GetStatesUsingDecision(StateDecision decision)
        {
            if (!decision) return string.Empty;
            var states = _currentFSM.States.FindAll(state => state != null && state.Transitions != null && state.Transitions.Exists(t => t != null && t.Decisions != null && t.Decisions.Exists(d => d != null && d.Decision.Equals(decision))));
            if (states == null || states.Count == 0) return string.Empty;
            StringBuilder text = new StringBuilder();

            text.Append(decision.Name + " is being used in " + states.Count + " state(s)\n");
            for (int i = 0; i < states.Count; i++)
            {
                text.Append(states[i].Name);
                if (i < states.Count - 1) text.Append(" , ");
            }

            return text.ToString();
        }

        void AddComponentContext(FSMBehaviour graph, Rect rect)
        {
            try
            {
                GenericMenu menu = new GenericMenu();
                List<GenericMenuItem> menuItems = new List<GenericMenuItem>();
                var possibleActions = typeof(StateAction).FindSubClasses();
                menu.AddItem(new GUIContent("Action/New Action Script"), false, () => { NodeMenus.CreateNewAction(); });
                menu.AddSeparator("Action/");

                foreach (var type in possibleActions)
                {
                    var instance = (StateAction)ScriptableObject.CreateInstance(type.FullName);
                    if (instance)
                    {
                        menuItems.Add(new GenericMenuItem(new GUIContent("Action/" + (instance.CategoryName) + (instance.DefaultName)), () => { AddAction(graph, type); if (instance) GameObject.DestroyImmediate(instance); }));
                    }

                }
                //   menuItems.Sort((x, y) => string.Compare(x.content.text, y.content.text));
                //  menuItems.Sort((x, y) => (x.content.text.Split('/').Length > y.content.text.Split('/').Length)?0:1);
                foreach (var item in menuItems)
                {
                    menu.AddItem(item.Content, false, item.Func);
                }
                menuItems.Clear();


                menu.AddItem(new GUIContent("Decision/New Decision Script"), false, () => { NodeMenus.CreateNewDecision(); });
                menu.AddSeparator("Decision/");
                var possibleDecisions = typeof(StateDecision).FindSubClasses();
                foreach (var type in possibleDecisions)
                {
                    var instance = (StateDecision)ScriptableObject.CreateInstance(type.Name);
                    if (instance)
                        menuItems.Add(new GenericMenuItem(new GUIContent("Decision/" + (instance.CategoryName) + (instance.DefaultName)), () => { AddTransition(graph, type); if (instance) GameObject.DestroyImmediate(instance); }));
                }
                menuItems.Sort((x, y) => string.Compare(x.Content.text, y.Content.text));

                foreach (var item in menuItems)
                {
                    menu.AddItem(item.Content, false, item.Func);
                }

                menu.ShowAsContext();
            }
            catch (UnityException e) { Debug.LogWarning("Add FSM Compornent Error :\n" + e.Message, _currentFSM); }
        }

        public void AddTransition(FSMBehaviour curGraph, Type type)
        {
            if (curGraph != null && type != null)
            {
                var decision = ScriptableObject.CreateInstance(type) as StateDecision;
                decision.name = decision.DefaultName;
                if (decision != null)
                {
                    decision.hideFlags = HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(decision, curGraph);
                    curGraph.OnChangeChilds();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        public void AddAction(FSMBehaviour curGraph, Type type)
        {
            if (curGraph != null && type != null)
            {
                var action = ScriptableObject.CreateInstance(type) as StateAction;
                action.name = action.DefaultName;
                if (action != null)
                {
                    action.hideFlags = HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(action, curGraph);
                    curGraph.OnChangeChilds();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        public void DeleteObjet(ref List<Editor> editorList, int index, string aditionalText)
        {
            editorList[index].serializedObject.Update();

            if (EditorUtility.DisplayDialog("Delete " + editorList[index].target.name + " Component", "Are you sure you want to remove this component?" + "\n" + aditionalText, "OK", "Cancel"))
            {
                GameObject.DestroyImmediate(editorList[index].target, true); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
                editorList.RemoveAt(index);
            }
        }

    }
}