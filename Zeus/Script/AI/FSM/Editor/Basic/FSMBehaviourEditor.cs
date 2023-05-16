using UnityEngine;
using UnityEditor;

namespace Zeus
{
    [CustomEditor(typeof(FSMBehaviour))]
    public class FSMBehaviourEditor : Editor
    {
        private void OnEnable()
        {
            ((FSMBehaviour)target).ReloadChilds();
        }
        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical();
            if (!FSMNodeEditorWindow.curWindow)
                if (GUILayout.Button("Open in FSM Editor Window"))
                {
                    FSMNodeEditorWindow.InitEditorWindow(target as FSMBehaviour);
                }

            base.OnInspectorGUI();
            GUILayout.EndVertical();
        }
        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
}