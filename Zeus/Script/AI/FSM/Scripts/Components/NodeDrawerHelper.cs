#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Zeus
{
    public static class NodeDrawerHelper
    {
        public static void InitNode(this FSMState state)
        {
            state.NodeRect.width = 150;
            state.NodeRect.height = 30;
            state.PositionRect = state.NodeRect.position;
            state.Transitions = new List<StateTransition>();
        }

        /// <summary>
        /// Update Node GUI for Node
        /// </summary>
        /// <param name="state"></param>
        /// <param name="e"></param>
        /// <param name="viewRect"></param>
        /// <param name="viewSkin"></param>
        public static void UpdateNodeGUI(this FSMState state, Event e, Rect viewRect, GUISkin viewSkin)
        {
            var color = GUI.color;
            var stateIndex = state.ParentGraph.States.IndexOf(state);
            GUI.color = (stateIndex > 1 ? state.NodeColor : Color.white);
            if (!state.InDrag && !state.ResizeLeft && !state.ResizeRight)
            {
                state.PositionRect.x = state.NodeRect.position.x.NearestRound(FSMHelper.DragSnap);
                state.PositionRect.y = state.NodeRect.position.y.NearestRound(FSMHelper.DragSnap);
                state.NodeRect.position = state.PositionRect;
            }
            if (!state.ResizeLeft && !state.ResizeRight)
            {
                state.RectWidth = state.NodeRect.width.NearestRound(FSMHelper.DragSnap);
                state.NodeRect.width = state.RectWidth;
            }
            IFSMBehaviourController fsmBehaviour = (Selection.activeGameObject ? Selection.activeGameObject.GetComponent<IFSMBehaviourController>() : null);

            if (fsmBehaviour != null)
            {
                var controller = (Selection.activeGameObject ? Selection.activeGameObject.GetComponent<IControlAI>() : null);
                if (controller != null)
                {
                    var interfaces = controller.GetType().GetInterfaces();
                    bool contains = false;
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (interfaces[i].Equals(state.RequiredType))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                        Debug.Log("REQUIRED TYPE OF CONTROLLER IS " + state.RequiredType.Name);
                }
            }
            bool isRunningInPlayMode = fsmBehaviour != null && Application.isPlaying && fsmBehaviour.FsmBehaviour && fsmBehaviour.FsmBehaviour.States.Contains(state) && fsmBehaviour.FsmBehaviour.States.IndexOf(state) == fsmBehaviour.IndexOffCurrentState;

            var nodeBackgroundStyle = viewSkin.GetStyle("NodeDefault");
            var nodeBorderStyle = viewSkin.GetStyle("NodeBorder");
            if ((!Application.isPlaying && state.ParentGraph && state.ParentGraph.States[0] && state.ParentGraph.States[0].DefaultTransition == state) || isRunningInPlayMode)
            {

                var shadowRect = new Rect(state.NodeRect.x - 5, state.NodeRect.y - 5, state.NodeRect.width + 10, state.NodeRect.height + 10);
                if (isRunningInPlayMode && fsmBehaviour != null && !fsmBehaviour.IsStopped && fsmBehaviour.IAIController != null && (fsmBehaviour.IAIController as MonoBehaviour).enabled)
                {
                    var t = EditorPrefs.GetFloat("vStateBorderTimer", 1f);
                    t += 0.01f;
                    GUI.color *= Mathf.Clamp(Mathf.PingPong(t, 1f), 0.5f, 1f);
                    EditorPrefs.SetFloat("vStateBorderTimer", t);
                }
                else if (EditorPrefs.HasKey("vStateBorderTimer")) EditorPrefs.DeleteKey("vStateBorderTimer");
                GUI.Box(shadowRect, "", viewSkin.GetStyle("Glow"));
            }

            GUI.SetNextControlName(state.name);
            var borderWidth = state.IsSelected ? 5 : 0;
            var borderRect = new Rect(state.NodeRect.x - borderWidth, state.NodeRect.y - (borderWidth), state.NodeRect.width + (borderWidth * 2), state.NodeRect.height + (borderWidth * 2));
            GUI.color = (stateIndex > 1 ? state.NodeColor : Color.white);
            GUI.color *= FSMBehaviourPreferences.borderAlpha;
            GUI.Box(borderRect, "", nodeBorderStyle);
            if (stateIndex > 1) GUI.color = state.IsSelected ? FSMBehaviourPreferences.selectedStateColor : FSMBehaviourPreferences.defaultStateColor;
            else GUI.color = state.IsSelected ? (stateIndex == 1 ? FSMBehaviourPreferences.anySelectedColor : FSMBehaviourPreferences.entrySelectedColor) : (stateIndex == 1 ? FSMBehaviourPreferences.anyNormalColor : FSMBehaviourPreferences.entryNormalColor);

            GUI.Box(state.NodeRect, "", nodeBackgroundStyle);
            GUI.color = color;


            GUILayout.BeginArea(state.NodeRect);
            {
                try
                {
                    var style = new GUIStyle(nodeBackgroundStyle);
                    style.normal.background = null;
                    style.hover.background = null;
                    style.active.background = null;
                    style.alignment = TextAnchor.MiddleCenter;
                    if (stateIndex > 1)
                        style.normal.textColor = state.IsSelected ? FSMBehaviourPreferences.selectedStateFontColor : FSMBehaviourPreferences.defaultStateFontColor;
                    else
                    {
                        if (stateIndex == 0) style.normal.textColor = state.IsSelected ? FSMBehaviourPreferences.entrySelectedFontColor : FSMBehaviourPreferences.entryNormalFontColor;
                        else if (stateIndex == 1) style.normal.textColor = state.IsSelected ? FSMBehaviourPreferences.anySelectedFontColor : FSMBehaviourPreferences.anyNormalFontColor;
                    }
                    GUILayout.Label(new GUIContent(state.Name, state.Description), style, GUILayout.Height(30));
                }
                catch { }

            }
            GUILayout.EndArea();
            state.UpdateStateGUI(e, viewRect, viewSkin);
            EditorUtility.SetDirty(state);

        }

        public static float NearestRound(this float x, float delX)
        {
            float rem = x % delX;
            return rem >= 5 ? (x - rem + delX) : (x - rem);
        }

        /// <summary>
        /// Update Node GUI for FSMState
        /// </summary>
        /// <param name="state"></param>
        /// <param name="e"></param>
        /// <param name="viewRect"></param>
        /// <param name="viewSkin"></param>
        public static void UpdateStateGUI(this FSMState state, Event e, Rect viewRect, GUISkin viewSkin)
        {
            var color = GUI.color;
            GUI.color = Color.white;

            if ((state.UseDecisions) && state.Transitions.Count > 0)
            {
                var foldoutRect = new Rect(state.NodeRect.x + 10f, state.NodeRect.y + 5f, 20, 20);
                state.IsOpen = EditorGUI.Toggle(foldoutRect, state.IsOpen, viewSkin.GetStyle("FoldoutClean"));
            }
            else
                state.IsOpen = false;
            GUI.color = color;
            if (state.UseDecisions)
                state.DrawTransitionHandles(e, viewRect, viewSkin);

            var ResizeLeft = state.NodeRect;
            var ResizeRight = state.NodeRect;
            ResizeLeft.width = 2;
            ResizeRight.width = 2;
            ResizeLeft.x -= 2;
            ResizeRight.x += state.NodeRect.width;
            state.Resize(ResizeLeft, e, ref state.ResizeLeft, true);
            state.Resize(ResizeRight, e, ref state.ResizeRight, false);
        }

        public static void Resize(this FSMState state, Rect rect, Event e, ref bool inResize, bool left = false)
        {
            if (!inResize)
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            else EditorGUIUtility.AddCursorRect(new Rect(e.mousePosition.x - 25, e.mousePosition.y - 25, 50, 50), MouseCursor.ResizeHorizontal);
            if (rect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDown)
                {
                    inResize = true;
                }
            }

            if (e.type == EventType.MouseUp)
            {
                inResize = false;
                // resizingPropView = false;
            }
            if (e.type == EventType.MouseDrag)
            {
                if (e.button == 0)
                {
                    if (inResize)
                    {
                        if (state.NodeRect.width <= 400 && state.NodeRect.width >= 100)
                        {
                            if (left)
                            {
                                if ((state.NodeRect.width - e.delta.x) <= 400 && (state.NodeRect.width - e.delta.x) >= 100)
                                {
                                    state.RectWidth += -e.delta.x;
                                    state.PositionRect.x += e.delta.x;

                                }
                            }
                            else if ((state.NodeRect.width + e.delta.x) <= 400 && (state.NodeRect.width + e.delta.x) >= 100)
                            {
                                state.RectWidth += e.delta.x;
                            }
                        }
                        else if (state.NodeRect.width < 100)
                        {
                            state.RectWidth = 100;
                        }
                        else if (state.NodeRect.width > 400)
                        {
                            state.RectWidth = 400;
                        }

                        state.NodeRect.width = state.RectWidth.NearestRound(FSMHelper.DragSnap);
                        state.NodeRect.x = state.PositionRect.x.NearestRound(FSMHelper.DragSnap);
                        e.Use();
                    }
                }
            }
        }

        static void DrawTransitionHandles(this FSMState state, Event e, Rect viewRect, GUISkin viewSkin)
        {
            var color = GUI.color;

            if (state.Transitions.Count > 0)
            {
                Vector2 transitionSize = new Vector2(10f, 10f);
                float space = state.IsOpen ? 2f : 0;
                float height = state.IsOpen ? ((((transitionSize.y * 2) * state.Transitions.Count)) + (state.Transitions.Count > 0 ? space * state.Transitions.Count : 0) + 30) + 10 : 30;
                state.NodeRect.height = height;
                var labelStyle = new GUIStyle(EditorStyles.whiteMiniLabel);
                labelStyle.alignment = TextAnchor.MiddleCenter;

                for (int i = 0; i < state.Transitions.Count; i++)
                {
                    var nullDecisions = state.Transitions[i].Decisions.FindAll(t => t.Decision == null);

                    for (int iNull = 0; iNull < nullDecisions.Count; iNull++) state.Transitions[i].Decisions.Remove(nullDecisions[iNull]);
                    bool trueRightSide = state.Transitions[i].TrueState ? state.NodeRect.x > state.Transitions[i].TrueState.NodeRect.x ? false : true : true;
                    bool falseRightSide = state.Transitions[i].FalseState ? state.NodeRect.x > state.Transitions[i].FalseState.NodeRect.x ? false : true : true;
                    var decisionRect = new Rect(state.NodeRect.x + 5f, state.Transitions[i].TrueRect.y, state.NodeRect.width - 10, transitionSize.y * 2);
                    if (state.IsOpen)
                    {
                        state.Transitions[i].TrueSideRight = trueRightSide;
                        state.Transitions[i].FalseSideRight = falseRightSide;
                        state.Transitions[i].TrueRect.width = transitionSize.x;
                        state.Transitions[i].TrueRect.height = transitionSize.y;
                        state.Transitions[i].FalseRect.width = transitionSize.x;
                        state.Transitions[i].FalseRect.height = transitionSize.y;

                        state.Transitions[i].TrueRect.x = trueRightSide ? ((state.NodeRect.x) + state.NodeRect.width) : (state.NodeRect.x) - transitionSize.x;
                        state.Transitions[i].TrueRect.y = state.IsOpen ? ((state.NodeRect.y + ((transitionSize.y * 2) * i)) + (i > 0 ? space * i : 0) + 30) : (state.NodeRect.y + 15) - transitionSize.y;
                        state.Transitions[i].FalseRect.x = falseRightSide ? (state.NodeRect.x + state.NodeRect.width) : state.NodeRect.x - transitionSize.x;
                        state.Transitions[i].FalseRect.y = state.IsOpen ? ((state.NodeRect.y + (transitionSize.y + ((transitionSize.y * 2) * i))) + (i > 0 ? space * i : 0) + 30) : transitionSize.y + (state.NodeRect.y + 15) - transitionSize.y;
                    }
                    else
                    {
                        state.Transitions[i].TrueRect.size = Vector2.zero;
                        state.Transitions[i].TrueRect.x = state.NodeRect.center.x;
                        state.Transitions[i].TrueRect.y = state.NodeRect.center.y;
                        state.Transitions[i].FalseRect.size = Vector2.zero;
                        state.Transitions[i].FalseRect.x = state.NodeRect.center.x;
                        state.Transitions[i].FalseRect.y = state.NodeRect.center.y;
                    }

                    GUI.color = color * 0.5f;

                    ///Transition selector
                    {
                        if (state.IsOpen)
                        {
                            GUI.enabled = state.SelectedDecisionIndex == i;
                            GUILayout.BeginArea(decisionRect, "", EditorStyles.helpBox);
                            {
                                GUI.color = color;
                                try
                                {
                                    var text = "";
                                    if (trueRightSide && falseRightSide || (!trueRightSide && !falseRightSide) || (!trueRightSide && falseRightSide))
                                    {
                                        if (state.Transitions[i].TransitionType == TransitionOutputType.TrueFalse || state.Transitions[i].TransitionType == TransitionOutputType.Default) text += (state.Transitions[i].TrueState ? state.Transitions[i].TrueState.name : "None");
                                        if (state.Transitions[i].TransitionType == TransitionOutputType.TrueFalse) text += " || ";
                                        if (state.Transitions[i].TransitionType == TransitionOutputType.TrueFalse) text += (state.Transitions[i].FalseState ? state.Transitions[i].FalseState.name : "None");
                                    }
                                    else if (trueRightSide && !falseRightSide)
                                    {
                                        if (state.Transitions[i].TransitionType == TransitionOutputType.TrueFalse) text += (state.Transitions[i].FalseState ? state.Transitions[i].FalseState.name : "None");
                                        if (state.Transitions[i].TransitionType == TransitionOutputType.TrueFalse) text += " || ";
                                        if (state.Transitions[i].TransitionType == TransitionOutputType.TrueFalse || state.Transitions[i].TransitionType == TransitionOutputType.Default) text += (state.Transitions[i].TrueState ? state.Transitions[i].TrueState.name : "None");
                                    }
                                    GUILayout.Space(-transitionSize.y / 2);

                                    GUILayout.Label(text, labelStyle);
                                }
                                catch { }
                            }
                            GUILayout.EndArea();
                            GUI.enabled = true;

                            if ( GUI.Button(decisionRect, "", GUIStyle.none) && viewRect.Contains(e.mousePosition))
                            {
                                state.ParentGraph.OnSelectState(state);
                                state.SelectedDecisionIndex = i;
                                state.Transitions[i].Select();
                            }

                            if ( decisionRect.Contains(e.mousePosition) && viewRect.Contains(e.mousePosition))
                            {
                                if (e.button == 1)
                                {
                                    GenericMenu menu = new GenericMenu();
                                    var transition = new StateTransition(null);

                                    for (int a = 0; a < state.Transitions[i].Decisions.Count; a++)
                                    {
                                        transition.Decisions.Add(state.Transitions[i].Decisions[a].Copy());
                                    }
                                    int index = i;
                                    var transitionToChange = state.Transitions[i];
                                    menu.AddItem(new GUIContent("Remove"), false, () =>
                                    {
                                        transitionToChange.parentState.Transitions.RemoveAt(index); e.Use();
                                    });

                                    menu.AddItem(new GUIContent("Duplicate"), false, () =>
                                    {
                                        state.Transitions.Add(transition);
                                        SerializedObject serializedObject = new SerializedObject(state);
                                        serializedObject.ApplyModifiedProperties();
                                        e.Use();
                                    });

                                    menu.ShowAsContext();
                                }
                            }
                        }
                    }
                    ///Output Button
                    {
                        GUI.enabled = true;
                        GUI.color = color;
                        decisionRect.x = decisionRect.x + decisionRect.width;
                        decisionRect.width = 15;
                        decisionRect.height = 15;
                        GUI.color = FSMBehaviourPreferences.transitionTrueColor;
                        if (state.Transitions[i].Decisions.Count == 0) GUI.color = FSMBehaviourPreferences.transitionDefaultColor;
                        if (state.Transitions[i].useTruState && state.IsOpen)
                        {
                            var matrix = GUI.matrix;
                            if (!trueRightSide)
                            {
                                var pivotPoint = new Vector2(state.Transitions[i].TrueRect.x + state.Transitions[i].TrueRect.width / 2, state.Transitions[i].TrueRect.y + state.Transitions[i].TrueRect.height / 2);
                                GUIUtility.RotateAroundPivot(180, pivotPoint);
                            }
                            GUI.Box(state.Transitions[i].TrueRect, "", viewSkin.GetStyle("InputButton"));
                            if (state.IsOpen && state.Transitions[i].TrueRect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
                            {
                                if (e.button == 0)
                                {
                                    state.ParentGraph.WantConnection = true;
                                    state.ParentGraph.TransitionPreviewVariable.SideRight = state.Transitions[i].TrueSideRight;
                                    state.ParentGraph.TransitionPreviewVariable.TransitionRect = state.Transitions[i].TrueRect;
                                    state.ParentGraph.TransitionPreviewVariable.State = state;
                                    state.ParentGraph.TransitionPreviewVariable.OnValidate = state.Transitions[i].SetTrueState;
                                }
                            }
                            GUI.matrix = matrix;
                        }

                        GUI.color = FSMBehaviourPreferences.transitionFalseColor;
                        if (state.Transitions[i].Decisions.Count == 0) GUI.color = FSMBehaviourPreferences.transitionDefaultColor;
                        if (state.Transitions[i].useFalseState && state.IsOpen)
                        {
                            var matrix = GUI.matrix;
                            if (!falseRightSide)
                            {
                                var pivotPoint = new Vector2(state.Transitions[i].FalseRect.x + state.Transitions[i].FalseRect.width / 2, state.Transitions[i].FalseRect.y + state.Transitions[i].FalseRect.height / 2);
                                GUIUtility.RotateAroundPivot(180, pivotPoint);
                            }
                            GUI.Box(state.Transitions[i].FalseRect, "", viewSkin.GetStyle("InputButton"));
                            if (state.Transitions[i].FalseRect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
                            {
                                if (e.button == 0)
                                {
                                    state.ParentGraph.WantConnection = true;                                  
                                    state.ParentGraph.TransitionPreviewVariable.SideRight = state.Transitions[i].FalseSideRight;
                                    state.ParentGraph.TransitionPreviewVariable.TransitionRect = state.Transitions[i].FalseRect;
                                    state.ParentGraph.TransitionPreviewVariable.State = state;
                                    state.ParentGraph.TransitionPreviewVariable.OnValidate = state.Transitions[i].SetFalseState;
                                }
                            }
                            GUI.matrix = matrix;
                        }
                    }
                }
            }
            else state.NodeRect.height = 30;
            GUI.color = color;
        } 
        public static void AddNewTransition(this FSMState state)
        {
           state.Transitions.Add(new StateTransition(null));
          
           state.ParentGraph.WantConnection = true;
            state.ParentGraph.TransitionPreviewVariable.SideRight = null;
           state.ParentGraph.TransitionPreviewVariable.TransitionRect = state.NodeRect;
            state.ParentGraph.TransitionPreviewVariable.State = state;
            state.ParentGraph.TransitionPreviewVariable.OnValidate = state.Transitions[state.Transitions.Count - 1].SetTrueState;
        }
        static void AddActionsMenu(this FSMState state, ref GenericMenu menu)
        {
            List<GenericMenuItem> menuItems = new List<GenericMenuItem>();

            for (int i = 0; i < state.ParentGraph.Actions.Count; i++)
            {
                if (state.ParentGraph.Actions[i] && state.ParentGraph.Actions[i].target)
                {
                    if (state.ParentGraph.Actions[i].target.GetType().IsSubclassOf(typeof(StateAction)) || state.ParentGraph.Actions[i].target.GetType().Equals(typeof(StateAction)))
                    {
                        var action = state.ParentGraph.Actions[i].target as StateAction;
                        menuItems.Add(new GenericMenuItem(new GUIContent("Action/" + state.ParentGraph.Actions[i].target.name), () =>
                        {

                            state.Actions.Add(action);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }));
                    }
                }
            }
            menuItems.Sort((x, y) => string.Compare(x.Content.text, y.Content.text));
            foreach (var item in menuItems)
            {
                menu.AddItem(item.Content, false, item.Func);
            }
        }
       
        static void AddDecisionsMenu(this StateTransition transition, ref GenericMenu menu)
        {
            List<GenericMenuItem> menuItems = new List<GenericMenuItem>();

            for (int i = 0; i < transition.parentState.ParentGraph.Decisions.Count; i++)
            {
                if (transition.parentState.ParentGraph.Decisions[i] && transition.parentState.ParentGraph.Decisions[i].target)
                {
                    if (transition.parentState.ParentGraph.Decisions[i].target.GetType().IsSubclassOf(typeof(StateDecision)) || transition.parentState.ParentGraph.Decisions[i].target.GetType().Equals(typeof(StateDecision)))
                    {
                        var decision = transition.parentState.ParentGraph.Decisions[i].target as StateDecision;
                        menuItems.Add(new GenericMenuItem(new GUIContent("Decision/" + transition.parentState.ParentGraph.Decisions[i].target.name), () =>
                        {
                            transition.Decisions.Add(new StateDecisionObject(decision));
                            EditorUtility.SetDirty(transition.parentState);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }));
                    }
                }
            }
            menuItems.Sort((x, y) => string.Compare(x.Content.text, y.Content.text));
            foreach (var item in menuItems)
            {
                menu.AddItem(item.Content, false, item.Func);
            }
        }

        public static void DrawPrimaryProperties(this FSMState state, SerializedObject serializedObject, GUISkin viewSkin)
        {
            SerializedProperty description = serializedObject.FindProperty("description");
            if (description != null) EditorGUILayout.PropertyField(description);
            if (state.CanEditColor)
            {
                SerializedProperty color = serializedObject.FindProperty("NodeColor");
                if (color != null) EditorGUILayout.PropertyField(color);
            }
            if (state.CanEditName)
            {
                SerializedProperty changeCurrentSpeed = serializedObject.FindProperty("changeCurrentSpeed");
                SerializedProperty customSpeed = serializedObject.FindProperty("customSpeed");
                SerializedProperty resetCurrentDestination = serializedObject.FindProperty("resetCurrentDestination");
                if (changeCurrentSpeed != null) EditorGUILayout.PropertyField(changeCurrentSpeed);
                if (customSpeed != null) EditorGUILayout.PropertyField(customSpeed);
                if (resetCurrentDestination != null) EditorGUILayout.PropertyField(resetCurrentDestination);
            }
        }

        public static void DrawProperties(this FSMState state, SerializedObject serializedObject, GUISkin viewSkin)
        {
            try
            {
                Event e = Event.current;
                state.Actions = state.Actions.FindAll(a => a != null);
                state.Transitions = state.Transitions.FindAll(t => t != null);

                if (state.UseActions)
                {
                    GUILayout.Space(10);
                    GUILayout.BeginVertical(viewSkin.box);
                    //Draw Actions
                    {
                        GUILayout.Label("State Actions", viewSkin.GetStyle("LabelHeader"), GUILayout.ExpandWidth(true)); GUILayout.Space(5);
                        if (state.Actions.Count > 0 && state.ParentGraph.Actions.Count > 0)
                        {
                            var actionsToDraw = state.ParentGraph.Actions.FindAll(a => state.Actions.Contains(a.target as StateAction));

                            var rect = new Rect();
                            bool click = false;
                            for (int i = 0; i < state.Actions.Count; i++)
                            {
                                state.Actions[i].ParentFSM = state.ParentGraph;
                            }
                            for (int i = 0; i < actionsToDraw.Count; i++)
                            {
                                if (!(actionsToDraw[i] == null || actionsToDraw[i].target == null))
                                {
                                    actionsToDraw[i].OnInspectorGUI();
                                    rect = GUILayoutUtility.GetLastRect();
                                    rect.x = rect.width - EditorGUIUtility.singleLineHeight * 0.4f;
                                    rect.height = EditorGUIUtility.singleLineHeight;
                                    rect.width = EditorGUIUtility.singleLineHeight;
                                    click = GUI.Button(rect, "-", viewSkin.box);
                                    if (rect.Contains(e.mousePosition) && click)
                                    {
                                        if (e.button == 0)
                                        {
                                            GenericMenu menu = new GenericMenu();
                                            int index = state.Actions.IndexOf(actionsToDraw[i].target as StateAction);
                                            menu.AddItem(new GUIContent("Remove"), false, () => { state.Actions.RemoveAt(index); e.Use(); });
                                            menu.ShowAsContext();
                                        }
                                    }
                                    click = false;
                                }
                            }
                        }
                    }
                    GUILayout.EndVertical();

                    /*Add Actions To State*/
                    {
                        var plusButtonRect = GUILayoutUtility.GetLastRect();
                        plusButtonRect.y += plusButtonRect.height;
                        plusButtonRect.x += plusButtonRect.width - EditorGUIUtility.singleLineHeight;
                        plusButtonRect.width = EditorGUIUtility.singleLineHeight;
                        plusButtonRect.height = EditorGUIUtility.singleLineHeight;
                        if (GUI.Button(plusButtonRect, new GUIContent("+", "Add Decision"), viewSkin.box))
                        {
                            GenericMenu menu = new GenericMenu();
                            AddActionsMenu(state, ref menu);
                            menu.ShowAsContext();
                        }
                    }
                }


                if (state.UseDecisions && state.Transitions.Count > 0)
                {
                    GUILayout.Space(EditorGUIUtility.singleLineHeight * 2);
                    GUILayout.Box("", viewSkin.GetStyle("Separator"), GUILayout.Height(2), GUILayout.ExpandWidth(true));
                    GUILayout.Space(EditorGUIUtility.singleLineHeight);
                    GUILayout.BeginVertical(viewSkin.box);
                    GUILayout.BeginVertical(viewSkin.box);
                    //Draw Transition Selector
                    {
                        GUILayout.Label("State Transitions ", viewSkin.GetStyle("LabelHeader"), GUILayout.ExpandWidth(true)); GUILayout.Space(5);

                        for (int i = 0; i < state.Transitions.Count; i++)
                        {
                            GUILayout.BeginVertical("", viewSkin.box);
                            {
                                GUI.enabled = state.SelectedDecisionIndex == i;
                                if (!state.Transitions[i].parentState) state.Transitions[i].parentState = state;

                                state.Transitions[i].DrawTransitionSelector(e, viewSkin, (state.SelectedDecisionIndex == i));

                                GUI.enabled = true;
                            }
                            GUILayout.EndVertical();
                            var decisionRect = GUILayoutUtility.GetLastRect();
                            if (GUI.Button(decisionRect, "", GUIStyle.none))
                            {
                                if (state.SelectedDecisionIndex != i)
                                {
                                    state.SelectedDecisionIndex = i;
                                    state.Transitions[i].Select();

                                    if (Selection.activeObject != state)
                                        Selection.activeObject = state;
                                }
                                else
                                {
                                    state.Transitions[i].Deselect();
                                }
                            }
                        }
                    }

                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(10);
                        if (state.Transitions.Count > 0 && state.SelectedDecisionIndex >= 0 && state.SelectedDecisionIndex < state.Transitions.Count)
                        {
                            state.Transitions[state.SelectedDecisionIndex].DrawTransitionsProperties(e, viewSkin, true);
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndVertical();
                }
            }
            catch { }
        }

        public static void UpdateNodeConnections(this FSMState state, Rect viewRect, Event e)
        {
            if (state.UseDecisions) state.UpdateStateConnections(viewRect, e);
            else if (state.DefaultTransition)
                DrawNodeCurve(e, state.NodeRect, state.DefaultTransition.NodeRect, FSMBehaviourPreferences.transitionDefaultColor);
        }

        static void UpdateStateConnections(this FSMState state, Rect viewRect, Event e)
        {
            for (int i = state.Transitions.Count - 1; i >= 0; i--)
            {
                if (state.Transitions[i].TrueState && state.Transitions[i].useTruState)
                {
                    state.DrawNodeCurve(e, state.Transitions[i].TrueRect, state.Transitions[i].TrueState.NodeRect, FSMBehaviourPreferences.transitionTrueColor, state.Transitions[i], true);
                }
                if (state.Transitions[i].FalseState && state.Transitions[i].useFalseState)
                {
                    state.DrawNodeCurve(e, state.Transitions[i].FalseRect, state.Transitions[i].FalseState.NodeRect, FSMBehaviourPreferences.transitionFalseColor, state.Transitions[i], false);
                }
            }
            for (int i = 0; i < state.Transitions.Count; i++)
            {
                if (state.Transitions[i].TrueState && state.Transitions[i].useTruState)
                {
                    state.DrawNodeCurveSelectable(viewRect, e, state.Transitions[i].TrueRect, state.Transitions[i].TrueState.NodeRect, FSMBehaviourPreferences.transitionTrueColor, state.Transitions[i], true);
                }
                if (state.Transitions[i].FalseState && state.Transitions[i].useFalseState)
                {
                    state.DrawNodeCurveSelectable(viewRect, e, state.Transitions[i].FalseRect, state.Transitions[i].FalseState.NodeRect, FSMBehaviourPreferences.transitionFalseColor, state.Transitions[i], false);
                }
            }
        }

        static void DrawNodeCurveSelectable(this FSMState state, Rect viewRect, Event e, Rect start, Rect end, Color color, StateTransition transition, bool value)
        {
            Handles.BeginGUI();
            Vector3 startPos = Vector3.zero;
            Vector3 endPos = Vector3.zero;
            Vector3 startTan = Vector3.zero;
            Vector3 endTan = Vector3.zero;
            CalculateBezier(start, end, transition, value, ref startPos, ref startTan, ref endPos, ref endTan);
            var dist = (endPos - startPos).magnitude;
            var points = Handles.MakeBezierPoints(startPos, endPos, state.IsOpen ? startTan : startPos, state.IsOpen ? endTan : endPos, (int)(Mathf.Clamp(dist, 2, 100)));
            var length = (uint)points.Length;
            var transitionCount = state.Transitions.FindAll(t => state.Transitions.IndexOf(t) > state.Transitions.IndexOf(transition) && ((value && t.TrueState && t.TrueState == transition.TrueState) || (!value && t.FalseState && t.FalseState == transition.FalseState)));

            if (!state.IsOpen && transitionCount.Count > 0)
            {
                length = (uint)Mathf.Clamp((points.Length - (points.Length * .15f) * (1 + transitionCount.Count)), 2, points.Length);
            }
            #region Debug Selector
            //for (int i = 0; i < length; i++)
            //{
            //    var rect = new Rect(points[i].x - ((i == length - 1) ? 5 : 2.5f), points[i].y - ((i == length - 1) ? 5 : 2.5f), ((i == length - 1) ? 10 : 5f), ((i == length - 1) ? 10 : 5f));

            //    GUI.Box(rect, "-");
            //}
            #endregion
            if (e.type == EventType.MouseDown && !state.ParentGraph.OverNode && viewRect.Contains(e.mousePosition) && !state.NodeRect.Contains(e.mousePosition))
            {
                var buttom = e.button;
                var selected = false;

                for (int i = 0; i < length; i++)
                {
                    var rect = new Rect(points[i].x - ((i == length - 1) ? 10 : 2.5f), points[i].y - ((i == length - 1) ? 10 : 2.5f), ((i == length - 1) ? 20 : 5f), ((i == length - 1) ? 20 : 5f));
                    if (rect.Contains(e.mousePosition))
                    {
                        selected = true;
                        state.IsSelected = true;
                        Selection.activeObject = state;
                        state.ParentGraph.SelectedNode = state;
                        state.SelectedDecisionIndex = state.Transitions.IndexOf(transition);
                        state.ParentGraph.DeselectAllExcludinCurrent();
                    }
                }

                if (selected)
                {
                    if (buttom == 1)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Delete"), false, () => { if (value) transition.TrueState = null; else transition.FalseState = null; });
                        menu.ShowAsContext();
                    }
                    else
                    {
                        if (value)
                        {
                            transition.Select(false, true);
                        }
                        else
                        {
                            transition.Select(true, false);
                        }
                        EditorUtility.SetDirty(state);
                    }
                    e.Use();
                }
            }
            Handles.EndGUI();
        }

        static void DrawNodeCurve(this FSMState state, Event e, Rect start, Rect end, Color color, StateTransition transition, bool value)
        {
            //if(decision!=null)
            {
                Handles.BeginGUI();
                Vector3 startPos = Vector3.zero;
                Vector3 endPos = Vector3.zero;
                Vector3 startTan = Vector3.zero;
                Vector3 endTan = Vector3.zero;
                CalculateBezier(start, end, transition, value, ref startPos, ref startTan, ref endPos, ref endTan);
                var dist = (endPos - startPos).magnitude;

                var points = Handles.MakeBezierPoints(startPos, endPos, state.IsOpen ? startTan : startPos, state.IsOpen ? endTan : endPos, (int)(Mathf.Clamp(dist, 2, 100)));
                var length = (uint)0;
                var transitionCount = state.Transitions.FindAll(t => state.Transitions.IndexOf(t) > state.Transitions.IndexOf(transition) && ((value && t.TrueState && t.TrueState == transition.TrueState) || (!value && t.FalseState && t.FalseState == transition.FalseState)));

                if (!state.IsOpen && transitionCount.Count > 0)
                {
                    length = (uint)Mathf.Clamp((points.Length - (points.Length * .15f) * (1 + transitionCount.Count)), 2, points.Length);
                    var pRef = new Vector3[length];
                    for (int i = 0; i < length; i++)
                    {
                        pRef[i] = points[i];
                    }
                    points = pRef;
                }

                if (value && transition.MuteTrue || !value && transition.MuteFalse) color = FSMBehaviourPreferences.transitionMuteColor;
                else if (transition.Decisions.Count == 0) color = FSMBehaviourPreferences.transitionDefaultColor;
                var lineWidth = 3;
                var isSelectedLine = (value) ? transition.SelectedTrue : transition.SelectedFalse;
                if (isSelectedLine)
                {
                    if (value && transition.MuteTrue || !value && transition.MuteFalse) color = color + Color.cyan * 0.2f;
                    else color = color + Color.cyan;
                    lineWidth = 5;
                }

                var _color = Handles.color;
                Handles.color = color;
                Handles.DrawAAPolyLine(Resources.Load("line") as Texture2D, lineWidth, points);
                DrawArrow(points[Mathf.Clamp(points.Length - 10, 0, points.Length)], points[points.Length - 1], lineWidth, color);
                Handles.color = _color;
                Handles.EndGUI();
            }
        }

        static void DrawNodeCurve(Event e, Rect start, Rect end, Color color)
        {
            //if(decision!=null)
            {
                Handles.BeginGUI();

                Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
                Vector3 endPos = new Vector3(end.x + end.width * 0.5f, end.y + end.height * 0.5f, 0);
                var dist = (endPos - startPos).magnitude;
                Bounds bound = new Bounds(end.center, end.size);
                endPos = bound.ClosestPoint(endPos + (startPos - endPos));
                endPos += (startPos - endPos).normalized * 20f;
                var magniture = Mathf.Clamp(((endPos - startPos).magnitude / 200f) - 0.5f, 0f, 1f);
                Vector3 startTan = startPos;
                var endTanDir = -(endPos - startPos).normalized;
                Vector3 endTan = endPos + endTanDir * (100 * magniture);
                var lineWidth = 4;
                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, Resources.Load("line") as Texture2D, lineWidth);
                DrawArrow(startPos, endPos, lineWidth, color);
                Handles.EndGUI();
            }
        }

        static void CalculateBezier(Rect start, Rect end, StateTransition transition, bool value, ref Vector3 refStart, ref Vector3 refStartTan, ref Vector3 refEnd, ref Vector3 refEndTan)
        {
            Handles.BeginGUI();
            var sideRight = value ? transition.TrueSideRight : transition.FalseSideRight;
            Vector3 startPos = new Vector3(start.x + (sideRight ? start.width : 0), start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x + end.width * 0.5f, end.y + end.height * 0.5f, 0);
            Bounds bound = new Bounds(value ? transition.TrueState.NodeRect.center : transition.FalseState.NodeRect.center, value ? transition.TrueState.NodeRect.size : transition.FalseState.NodeRect.size);
            endPos = bound.ClosestPoint(endPos + (startPos - endPos));
            endPos += (startPos - endPos).normalized * 20f;
            var magniture = Mathf.Clamp(((endPos - startPos).magnitude / 200) - 0.5f, 0f, 1f);
            Vector3 startTan = startPos + (sideRight ? Vector3.right : Vector3.left) * (200 * magniture);
            var endTanDir = -(endPos - startPos).normalized;
            Vector3 endTan = endPos + endTanDir * (100 * magniture);

            refStart = startPos;
            refStartTan = startTan;
            refEnd = endPos;
            refEndTan = endTan;
            Handles.EndGUI();
        }

        static void DrawArrow(Vector3 start, Vector3 end, float lineWidth, Color color)
        {
            var forward = Quaternion.identity * (new Vector3(end.x, 0, end.y) - new Vector3(start.x, 0, start.y));
            Matrix4x4 m = GUI.matrix;
            var _color = GUI.color;

            GUI.color = color;

            var texture = Resources.Load("line_arrow") as Texture2D;
            var width = 8 + lineWidth * 2;
            var pos = new Vector3(end.x - width * 0.5f, end.y - width * 0.5f, 0);

            var angle = forward != Vector3.zero ? new Vector3(0, -(Quaternion.LookRotation(forward).eulerAngles.y - 90), 0).NormalizeAngle().y : 0;

            GUIUtility.RotateAroundPivot(angle, end);
            GUI.DrawTexture(new Rect(pos.x, pos.y, width, width), texture);
            GUI.matrix = m;
            GUI.color = _color;
        }

        public static void OnDrag(this FSMState state, Vector2 delta, bool snap = true)
        {
            state.InDrag = true;
            if (state.PositionRect.magnitude < state.NodeRect.position.magnitude) state.PositionRect = state.NodeRect.position;
            if (snap)
            {
                state.PositionRect.x += delta.x;
                state.PositionRect.y += delta.y;
                state.NodeRect.x = state.PositionRect.x.NearestRound(FSMHelper.DragSnap);
                state.NodeRect.y = state.PositionRect.y.NearestRound(FSMHelper.DragSnap);
            }
            else
            {
                state.NodeRect.x += delta.x;
                state.NodeRect.y += delta.y;
                state.PositionRect = state.NodeRect.position;
            }
        }

        public static void OnEndDrag(this FSMState state)
        {
            state.InDrag = false;
        }

        public static void DrawTransitionSelector(this StateTransition transition, Event e, GUISkin viewSkin, bool isSelected = false)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    var color = GUI.color;
                    transition.DrawConnectionText(true, true);
                    GUI.color = color;

                    if (GUILayout.Button(new GUIContent("X", "Remove Transition"), viewSkin.box, GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        GenericMenu menu = new GenericMenu();
                        int index = transition.parentState.Transitions.IndexOf(transition);
                        menu.AddItem(new GUIContent("Remove"), false, () =>
                        {
                            transition.parentState.Transitions.RemoveAt(index); e.Use();
                        });
                        menu.ShowAsContext();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                if (transition.Decisions.Count > 0)
                {
                    GUILayout.Label("Output", EditorStyles.whiteMiniLabel);
                    transition.TransitionType = (TransitionOutputType)EditorGUILayout.EnumPopup("", transition.TransitionType, viewSkin.GetStyle("DropDown"), GUILayout.Width(80), GUILayout.Height(EditorGUIUtility.singleLineHeight * 0.9f));
                }
                else
                {
                    transition.TransitionType = TransitionOutputType.Default;
                    GUILayout.Label(new GUIContent("Output Direct", "If transition dont have decisions, this value is ever true"), EditorStyles.whiteMiniLabel);
                }
                GUILayout.FlexibleSpace();
                GUILayout.Label("Transition Delay", EditorStyles.whiteMiniLabel);
                transition.TransitionDelay = EditorGUILayout.FloatField("", transition.TransitionDelay, GUILayout.Width(50));
                if (GUI.changed) EditorUtility.SetDirty(transition.parentState);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        public static void DrawTransitionsProperties(this StateTransition transition, Event e, GUISkin viewSkin, bool isSelected = false)
        {
            if (isSelected)
            {
                GUILayout.Space(10);
                GUILayout.Box("", viewSkin.GetStyle("Separator"), GUILayout.Height(2), GUILayout.ExpandWidth(true));
                GUILayout.Space(10);
                GUILayout.BeginVertical(viewSkin.box);
                {
                    var stl = new GUIStyle(EditorStyles.helpBox);
                    stl.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("Decisions", stl, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));

                    if (transition.Decisions.Count > 0)
                    {
                        EditorGUILayout.HelpBox("Decisions will return a True or False result", MessageType.Info);
                        var rect = new Rect();
                        IFSMBehaviourController fsmBehaviour = (Selection.activeGameObject && Application.isPlaying ? Selection.activeGameObject.GetComponent<IFSMBehaviourController>() : null);
                        bool isRunningInPlayMode = fsmBehaviour != null && fsmBehaviour.FsmBehaviour;
                        bool click = false;
                        for (int i = 0; i < transition.Decisions.Count; i++)
                        {
                            if (transition.Decisions[i].Decision)
                            {
                                transition.Decisions[i].Decision.ParentFSM = transition.parentState.ParentGraph;
                            }

                            var color = GUI.color;
                            if (isRunningInPlayMode)
                            {
                                GUI.color = transition.Decisions[i].IsValid ? Color.green : Color.red;
                            }
                            transition.Decisions[i].DrawDecisionEditor();
                            GUI.color = color;
                            rect = GUILayoutUtility.GetLastRect();
                            rect.x = rect.width - 50;
                            rect.width = 44;
                            rect.y += 2;
                            rect.height = EditorGUIUtility.singleLineHeight;
                            rect.height -= 4;
                            transition.Decisions[i].TrueValue = (EditorGUI.Popup(rect, transition.Decisions[i].TrueValue ? 0 : 1, new string[] { "true", "false" }, viewSkin.GetStyle("DropDown")) == 0 ? true : false);

                            rect.y -= 2;
                            rect.height += 4;
                            rect.x += 44;// rect.width - EditorGUIUtility.singleLineHeight * 0.4f;
                            rect.width = EditorGUIUtility.singleLineHeight;
                            click = GUI.Button(rect, "-", viewSkin.box);
                            if (rect.Contains(e.mousePosition) && click)
                            {
                                if (e.button == 0)
                                {
                                    transition.Decisions.RemoveAt(i);
                                }
                            }
                            click = false;
                        }
                    }
                }
                GUILayout.EndVertical();

                /*Add Decisions To Transition*/
                {
                    var plusButtonRect = GUILayoutUtility.GetLastRect();
                    plusButtonRect.y += plusButtonRect.height;
                    plusButtonRect.x += plusButtonRect.width - EditorGUIUtility.singleLineHeight;
                    plusButtonRect.width = EditorGUIUtility.singleLineHeight;
                    plusButtonRect.height = EditorGUIUtility.singleLineHeight;
                    if (GUI.Button(plusButtonRect, new GUIContent("+", "Add Decision"), viewSkin.box))
                    {
                        GenericMenu menu = new GenericMenu();
                        transition.AddDecisionsMenu(ref menu);
                        e.Use();
                        menu.ShowAsContext();
                    }
                }
            }
        }

        public static void DrawConnectionText(this StateTransition transition, bool ignoreTrue = false, bool ignoreFalse = false)
        {
            GUILayout.BeginVertical();
            var fontTransitionStyle = new GUIStyle(UnityEditor.EditorStyles.whiteMiniLabel);
            if (transition.useTruState && (transition.SelectedTrue || ignoreTrue))
            {

                GUILayout.BeginHorizontal();
                var color = GUI.color;
                GUI.color = transition.MuteTrue ? Color.black : Color.grey;
                transition.MuteTrue = GUILayout.Toggle(transition.MuteTrue, new GUIContent("", "Mute Transition"), EditorStyles.radioButton, GUILayout.ExpandWidth(false));
                GUI.color = color;
                GUILayout.Label((transition.TransitionType == TransitionOutputType.TrueFalse ? "True: " : "") + transition.parentState.name + "   >>>  " + (transition.TrueState && !transition.MuteTrue ? transition.TrueState.name : "None"), fontTransitionStyle);
                GUILayout.EndHorizontal();
            }

            if (transition.useFalseState && (transition.SelectedFalse || ignoreFalse))
            {
                GUILayout.BeginHorizontal();
                var color = GUI.color;
                GUI.color = transition.MuteFalse ? Color.black : Color.grey;
                transition.MuteFalse = GUILayout.Toggle(transition.MuteFalse, new GUIContent("", "Mute Transition"), EditorStyles.radioButton, GUILayout.ExpandWidth(false));
                GUI.color = color;
                GUILayout.Label((transition.TransitionType == TransitionOutputType.TrueFalse ? "False: " : "") + transition.parentState.name + "   >>>  " + (transition.FalseState && !transition.MuteFalse ? transition.FalseState.name : "None"), fontTransitionStyle);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        public static void Deselect(this StateTransition transition)
        {
            if (!transition.parentState) return;
            for (int i = 0; i < transition.parentState.Transitions.Count; i++)
            {
                transition.parentState.Transitions[i].SelectedFalse = false;
                transition.parentState.Transitions[i].SelectedTrue = false;
            }
            transition.SelectedTrue = false;
            transition.SelectedFalse = false;
        }

        public static void Select(this StateTransition transition, bool selectFalse = true, bool selectTrue = true)
        {
            if (!transition.parentState) return;

            for (int i = 0; i < transition.parentState.Transitions.Count; i++)
            {
                transition.parentState.Transitions[i].SelectedFalse = false;
                transition.parentState.Transitions[i].SelectedTrue = false;
            }
            transition.SelectedTrue = selectTrue ? true : false;
            transition.SelectedFalse = selectFalse ? true : false;
        }
    }
}
#endif