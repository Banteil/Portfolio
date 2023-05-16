using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace Zeus
{
    public static class NodeUtility
    {
        public const string newfsmpath = "/Invector-AIController/My FSM Behaviours";

        public static void CreateGraph()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New FSM Behaviour.asset");

            if (!string.IsNullOrEmpty(assetPathAndName))
            {
                CreateNewGraph(assetPathAndName);
            }
        }
        public static float NearestRound(float x, float multiple)
        {
            if (multiple < 1)
            {
                float i = (float)Math.Floor(x);
                float x2 = i;
                while ((x2 += multiple) < x) ;
                float x1 = x2 - multiple;
                return (Math.Abs(x - x1) < Math.Abs(x - x2)) ? x1 : x2;
            }
            else
            {
                return (float)Math.Round(x / multiple, MidpointRounding.AwayFromZero) * multiple;
            }
        }

        public static void NearestRound(this Vector2 vector, float multiple)
        {
            vector.x.NearestRound(multiple);
            vector.y.NearestRound(multiple);
        }

        public static void CreateNewGraph(string path)
        {
            FSMBehaviour curGraph = ScriptableObject.CreateInstance<FSMBehaviour>();
            if (curGraph != null)
            {
                AssetDatabase.CreateAsset(curGraph, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = curGraph;
                var entryNode = CreateNode<FSMState>("Entry", curGraph);
                entryNode.ResetCurrentDestination = false;
                entryNode.ParentGraph = curGraph;
                entryNode.UseDecisions = false;
                entryNode.UseActions = false;
                entryNode.CanEditName = false;
                entryNode.CanRemove = false;
                entryNode.CanTranstTo = false;
                entryNode.CanEditColor = false;
                entryNode.CanSetAsDefault = false;
                entryNode.Description = "This State Run Just in Start\n to init first state";
                entryNode.NodeColor = Color.green;
                var anyState = CreateNode<FSMState>("AnyState", curGraph);
                anyState.ResetCurrentDestination = false;
                anyState.UseDecisions = true;
                anyState.UseActions = false;
                anyState.CanEditName = false;
                anyState.CanRemove = false;
                anyState.CanTranstTo = false;
                anyState.CanEditColor = false;
                anyState.CanSetAsDefault = false;
                anyState.Description = "This State Run after current state";
                anyState.NodeColor = Color.cyan;
                anyState.NodeRect.y += 100;
                anyState.ParentGraph = curGraph;
                curGraph.States.Add(entryNode);
                curGraph.States.Add(anyState);
                curGraph.InitGraph();
                FSMNodeEditorWindow.InitEditorWindow(curGraph);
            }
            else
            {
                EditorUtility.DisplayDialog("Node Message", "Unable to create new graph, please see your friendly programmer!", "OK");
            }
        }

        public static int GetSameComponentNameCount<T>(this UnityEngine.Object obj)
        {
            var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(obj.GetInstanceID()));
            int count = 0;
            foreach (var o in objs)
            {
                if (o != null)
                {
                    if ((o.GetType().Equals(typeof(T)) || o.GetType().IsSubclassOf(typeof(T))) && o != obj && o.name.Equals(obj.name)) count++;
                }

            }
            return count;
        }

        public static void LoadGraph()
        {
            FSMBehaviour curGraph = null;
            string graphPath = EditorUtility.OpenFilePanel("Load FSM Behaviour", Application.dataPath + "/Invector-AIController/", "asset");
            if (!string.IsNullOrEmpty(graphPath))
            {
                int dataPathLength = Application.dataPath.Length - 6;
                string finalPah = graphPath.Substring(dataPathLength);
                curGraph = (FSMBehaviour)AssetDatabase.LoadAssetAtPath(finalPah, typeof(FSMBehaviour));

                FSMNodeEditorWindow curwindow = EditorWindow.GetWindow<FSMNodeEditorWindow>();
                if (curwindow != null)
                {
                    curwindow.curGraph = curGraph;
                    Selection.activeObject = curGraph;
                }
            }
        }

        public static void UnloadGraph()
        {
            FSMNodeEditorWindow curwindow = EditorWindow.GetWindow<FSMNodeEditorWindow>();
            if (curwindow != null)
            {
                if (curwindow.curGraph != null && Selection.activeObject == curwindow.curGraph)
                    Selection.activeObject = null;
                curwindow.curGraph = null;
            }
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        public static void CreateNode(FSMBehaviour curGraph, Vector2 mousePos)
        {
            if (curGraph != null)
            {
                FSMState curState = null;
                Undo.RecordObject(curGraph, "New Node");
                curState = ScriptableObject.CreateInstance<FSMState>();

                curState.Name = "State " + (curGraph.States.Count > 1 ? (curGraph.States.Count - 2).ToString() : "");

                if (curGraph.States.Count > 0)
                {
                    if (curGraph.States[0].DefaultTransition == null)
                    {
                        curGraph.States[0].DefaultTransition = curState;
                    }
                }
                curState.NodeColor = Color.white;
                if (curState != null)
                {
                    curState.InitNode();
                    curState.NodeRect.x = mousePos.x;
                    curState.NodeRect.y = mousePos.y;
                    curState.ParentGraph = curGraph;
                    curState.hideFlags = HideFlags.HideInHierarchy;

                    curGraph.States.Add(curState);

                    AssetDatabase.AddObjectToAsset(curState, curGraph);
                    var count = curState.GetSameComponentNameCount<FSMState>();
                    if (count > 0)
                        curState.Name += " " + (count - 1).ToString();

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        public static void CreateNode(Type type, FSMBehaviour curGraph, Vector2 mousePos)
        {
            if (curGraph != null && type.IsSubclassOf(typeof(FSMState)))
            {
                var curState = ScriptableObject.CreateInstance(type.FullName);
                Undo.RegisterCreatedObjectUndo(curState, "Create object");
                curState.name = "State " + (curGraph.States.Count > 1 ? (curGraph.States.Count - 2).ToString() : "");

                if (curGraph.States.Count > 0)
                {
                    if (curGraph.States[0].DefaultTransition == null)
                    {
                        curGraph.States[0].DefaultTransition = curState as FSMState;
                    }
                }
                (curState as FSMState).NodeColor = Color.white;
                if (curState != null)
                {
                    (curState as FSMState).InitNode();
                    (curState as FSMState).NodeRect.x = mousePos.x;
                    (curState as FSMState).NodeRect.y = mousePos.y;
                    (curState as FSMState).ParentGraph = curGraph;
                    curState.hideFlags = HideFlags.HideInHierarchy;
                    curGraph.States.Add((curState as FSMState));

                    AssetDatabase.AddObjectToAsset(curState, curGraph);
                    var count = curState.GetSameComponentNameCount<FSMState>();
                    if (count > 0)
                        (curState as FSMState).Name += " " + (count - 1).ToString();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        public static T CreateNode<T>(string name, FSMBehaviour parentGraph) where T : ScriptableObject
        {

            T curNode = null;
            curNode = (T)ScriptableObject.CreateInstance<T>();
            curNode.name = name;

            if (curNode != null)
            {
                AssetDatabase.AddObjectToAsset(curNode, parentGraph);
                curNode.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return curNode;

        }

        public static T Get<T>(Type type)
        {
            return (T)Convert.ChangeType(type, typeof(T));
        }

        public static void DeleteNode(int id, FSMBehaviour curGraph)
        {
            if (curGraph)
            {
                if (id >= 0 && id < curGraph.States.Count)
                {
                    FSMState deleteNode = curGraph.States[id];
                    if (deleteNode)
                    {
                        curGraph.SelectedNode = null;
                        curGraph.OverNode = false;
                        curGraph.States.RemoveAt(id);

                        for (int i = 0; i < deleteNode.Actions.Count; i++)
                        {
                            if (deleteNode.Actions[i])
                            {
                                var o = new SerializedObject(deleteNode.Actions[i]);
                                o.ApplyModifiedProperties();
                            }
                        }
                        for (int i = 0; i < deleteNode.Transitions.Count; i++)
                        {
                            if (deleteNode.Transitions[i].Decisions != null)
                            {
                                for (int a = 0; a < deleteNode.Transitions[i].Decisions.Count; a++)
                                {
                                    if (deleteNode.Transitions[i].Decisions[a].Decision)
                                    {
                                        var o = new SerializedObject(deleteNode.Transitions[i].Decisions[a].Decision);
                                        o.ApplyModifiedProperties();
                                    }
                                }
                            }
                        }
                        if (curGraph.States.Count > 2 && curGraph.States[0].DefaultTransition == deleteNode)
                        {
                            curGraph.States[0].DefaultTransition = curGraph.States[2] as FSMState;
                        }

                        Undo.DestroyObjectImmediate(deleteNode);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
}