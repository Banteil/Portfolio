using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Zeus
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FSMState), true)]
    public class SimpleNodeEditor : Editor
    {
        GUISkin skin;
        string valueName;
        Vector2 scrool;
        string[] ignoreProperties = new string[] { "Actions", "Transitions", "ParentGraph", "DefaultTransition", "Components", "NodeColor", "NodeRect", "IsSelected", "EditingName", "IsOpen", "m_Script", "ResizeLeft", "ResizeRight", "ChangeCurrentSpeed", "ResetCurrentDestination", "Description", "customSpeed" };

        void OnEnable()
        {
            if (skin == null) skin = (GUISkin)Resources.Load("GUISkins/EditorSkins/NodeEditorSkin");
        }

        public override void OnInspectorGUI()
        {

            if (target)
            {
                try
                {
                    var contentColor = GUI.contentColor;
                    GUI.contentColor = Color.white;
                    serializedObject.Update();
                    //  scrool = GUILayout.BeginScrollView(scrool);
                    GUI.SetNextControlName("None");
                    GUILayout.BeginVertical(skin.box);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(10);
                        GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                        {
                            var attribute = target.GetType().GetCustomAttributes(typeof(FSMHelpboxAttribute), true).FirstOrDefault() as FSMHelpboxAttribute;
                            if (attribute != null)
                            {
                                EditorGUILayout.HelpBox(attribute.Text, attribute.MessageType);
                            }
                            (target as FSMState).DrawPrimaryProperties(serializedObject, skin);
                            DrawPropertiesExcluding(serializedObject, ignoreProperties);
                            (target as FSMState).DrawProperties(serializedObject, skin);

                            GUILayout.FlexibleSpace();
                            GUILayout.Space(20);
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(10);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    //  GUILayout.EndScrollView();
                    serializedObject.ApplyModifiedProperties();
                    GUI.contentColor = contentColor;
                }
                catch { }

            }
        }

        public override bool UseDefaultMargins()
        {
            return false;// base.UseDefaultMargins();
        }

        public Rect propertyRect;
        protected override void OnHeaderGUI()
        {
            serializedObject.Update();
            if (GUI.GetNameOfFocusedControl().Equals("Name"))
            {
                if (!serializedObject.FindProperty("EditingName").boolValue)
                {
                    serializedObject.FindProperty("EditingName").boolValue = true;
                    serializedObject.ApplyModifiedProperties();
                    valueName = serializedObject.FindProperty("m_Name").stringValue;
                }
                if (Event.current.keyCode == KeyCode.Return || Event.current.type == EventType.MouseDown)
                    if (serializedObject.FindProperty("EditingName").boolValue = true && valueName != serializedObject.FindProperty("m_Name").stringValue)
                    {
                        var countSameName = target.GetSameComponentNameCount<FSMState>();
                        if (countSameName > 0) serializedObject.FindProperty("m_Name").stringValue += " " + (countSameName - 1).ToString();
                        valueName = serializedObject.FindProperty("m_Name").stringValue;
                        serializedObject.FindProperty("EditingName").boolValue = false;
                        serializedObject.ApplyModifiedProperties();
                    }
            }
            else
            {
                var countSameName = target.GetSameComponentNameCount<FSMState>();
                if (countSameName > 0)
                {
                    serializedObject.FindProperty("m_Name").stringValue += " " + (countSameName - 1).ToString();
                    valueName = serializedObject.FindProperty("m_Name").stringValue;
                    serializedObject.FindProperty("EditingName").boolValue = false;
                    serializedObject.ApplyModifiedProperties();

                }

            }

            if (!skin) skin = (GUISkin)Resources.Load("GUISkins/EditorSkins/NodeEditorSkin");
            var rect = EditorGUILayout.GetControlRect(true, 40, GUILayout.ExpandWidth(true));

            var content = EditorGUIUtility.ObjectContent(target, typeof(FSMState)).image;
            var imageRect = rect;
            imageRect.y += 5;
            imageRect.x += 5;
            imageRect.width = 35;
            rect.x -= 5;
            rect.height += 10;
            rect.width += 10;
            GUI.Box(rect, "", skin.box);
            GUI.Box(imageRect, content, GUIStyle.none);

            propertyRect = rect;
            propertyRect.x += 50;
            propertyRect.y += 10;
            propertyRect.width -= 80;
            propertyRect.height = 20;
            if ((target as FSMState).CanEditName)
            {
                GUI.SetNextControlName("Name");
                EditorGUI.DelayedTextField(propertyRect, serializedObject.FindProperty("m_Name"), GUIContent.none);
            }
            else
                GUI.Label(propertyRect, serializedObject.FindProperty("m_Name").stringValue);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }

        }

        void OnSceneGUI()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                GUI.FocusControl("NONE");
            }
        }
    }

}
