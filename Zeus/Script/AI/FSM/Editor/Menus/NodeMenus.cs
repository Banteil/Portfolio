using System.IO;
using UnityEditor;
using UnityEngine;

namespace Zeus
{
    public static class NodeMenus
    {
        #region FSM Script Templates
        public const string StateTemplate = "FSMStateTemplate";
        public const string ActionTemplate = "FSMActionTemplate";
        public const string DecisionTemplate = "FSMDecisionTemplate";
        #endregion

        #region FSM Menus
        public const string ZeusFSMMenu = "Zeus/FSM AI/FSM Behaviour/";
        public const string CreateFSMAssetMenu = "Assets/Create/Zeus/FSM Behaviour/";
        #endregion      

        #region FSM Create Behaviour
        [MenuItem(ZeusFSMMenu + "Create/New FSMBehaviour", priority = 1)]
        [MenuItem(CreateFSMAssetMenu + "New FSMBehaviour", priority = 1)]
        static void CreateGraph()
        {
            NodeUtility.CreateGraph();
        }
        #endregion

        #region FSM Create Components 
        [MenuItem(ZeusFSMMenu + "Create/New FSM Action")]
        [MenuItem(CreateFSMAssetMenu + "New FSM Action")]
        public static void CreateNewAction()
        {
            var assetTemplate = ActionTemplate;
            CreateFSMScript(assetTemplate, "MyFSMAction.cs");

        }

        [MenuItem(ZeusFSMMenu + "Create/New FSM Decision")]
        [MenuItem(CreateFSMAssetMenu + "New FSM Decision")]
        public static void CreateNewDecision()
        {
            var assetTemplate = DecisionTemplate;
            CreateFSMScript(assetTemplate, "MyFSMDecision.cs");

        }

        [MenuItem(ZeusFSMMenu + "Create/New FSM State")]
        [MenuItem(CreateFSMAssetMenu + "New FSM State")]
        static void CreateNewState(MenuCommand cmd)
        {
            var assetTemplate = StateTemplate;
            CreateFSMScript(assetTemplate, "MyFSMState.cs");

        }

        static void CreateFSMScript(string assetTemplate, string defaultName)
        {
            var path = "";
            var t = Resources.Load(assetTemplate) as TextAsset;
            if (Selection.activeObject != null)
                path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (File.Exists(path))
                path = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(path)) path = "Assets/";
            Resources.UnloadAsset(t);
            CreateScriptAsset(AssetDatabase.GetAssetPath(t.GetInstanceID()), GetDestinPath() + "/" + defaultName);
            AssetDatabase.Refresh();
        }

        static string GetDestinPath()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
                break;
            }
            return path;
        }

#if UNITY_2018
        static readonly MethodInfo createScriptMethod = typeof(ProjectWindowUtil)
            .GetMethod("CreateScriptAsset", BindingFlags.Static | BindingFlags.NonPublic);

        static void CreateScriptAsset(string templatePath, string destName)
        {
            createScriptMethod.Invoke(null, new object[] { templatePath, destName });
        }
#elif UNITY_2019_1_OR_NEWER
        static void CreateScriptAsset(string templatePath, string destName)
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, destName);
        }
#endif
        #endregion

        #region FSM Open Windows
        [MenuItem(ZeusFSMMenu + "Open FSM Behaviour Window", priority = 0)]
        public static void OpenFSMNodeEditor()
        {
            FSMNodeEditorWindow.InitEditorWindow();
        }

        [MenuItem(ZeusFSMMenu + "Open FSM Debug Window", priority = 0)]
        public static void OpenFSMDebug()
        {
            FSMBehaviourControllerDebugWindow.InitEditorWindow();
        }

        #endregion
    }
}