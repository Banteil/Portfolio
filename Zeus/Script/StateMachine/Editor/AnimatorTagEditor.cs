using UnityEditor;
using UnityEngine;

namespace Zeus
{
    [CustomEditor(typeof(AnimatorTag), true)]
    public class AnimatorTagEditor : Editor
    {
        public GUISkin Skin;
        Rect _buttonRect;
        protected virtual string[] _propertiesExcluded => new string[] { "Tags", "StateInfos" };

        public override void OnInspectorGUI()
        {
            if (!Skin) Skin = Resources.Load("zSkin") as GUISkin;
            serializedObject.Update();
            GUILayout.BeginVertical(Skin.box);
            DrawPropertiesExcluding(serializedObject, _propertiesExcluded);
            var tags = serializedObject.FindProperty("Tags");
            if (GUILayout.Button("Add New Tag", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
            {
                PopupWindow.Show(_buttonRect, new TagListPopup((string value) => { Undo.RecordObject(serializedObject.targetObject, "Add Tag"); AddTag(tags, value); serializedObject.ApplyModifiedProperties(); }));
            }
            if (Event.current.type == EventType.Repaint) _buttonRect = GUILayoutUtility.GetLastRect();

            for (int i = 0; i < tags.arraySize; i++)
            {
                if (!DrawTag(tags, i)) break;
            }
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void AddTag(SerializedProperty list, string tag)
        {
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).stringValue = tag;
        }

        public virtual bool DrawTag(SerializedProperty list, int index)
        {
            GUILayout.BeginHorizontal(Skin.box);
            GUILayout.BeginVertical();
            DrawTagField(list.GetArrayElementAtIndex(index));
            GUILayout.Space(-10);
            GUILayout.EndVertical();
            if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                list.DeleteArrayElementAtIndex(index);
                return false;
            }

            GUILayout.EndHorizontal();
            return true;
        }

        public virtual void DrawTagField(SerializedProperty tag)
        {
            var info = EditorGUIUtility.IconContent("_Help");
            info.tooltip = AnimatorTagEditorHelper.GetTooltip(tag.stringValue);
            bool isDefault = AnimatorTagEditorHelper.IsDefaultTag(tag.stringValue);
            GUILayout.BeginHorizontal();
            GUILayout.Label(info);
            Color color = GUI.contentColor;
            GUI.contentColor = isDefault ? Color.green : color;

            tag.stringValue = EditorGUILayout.TextField(tag.stringValue, isDefault ? AnimatorTagEditorHelper.DefaultTagStyle : AnimatorTagEditorHelper.CustomTagStyle);

            GUI.contentColor = color;
            GUILayout.EndHorizontal();
        }

        public class TagListPopup : PopupWindowContent
        {
            Vector2 scrollview;
            System.Action<string> onSelect;
            GUIStyle style;
            public TagListPopup(System.Action<string> onSelect)
            {
                this.onSelect = onSelect;
                style = new GUIStyle(GUI.skin.box);
                style.fontStyle = FontStyle.Italic;
            }
            public override Vector2 GetWindowSize()
            {
                float height = GUI.skin.box.CalcHeight(new GUIContent("TAG"), 200) + EditorGUIUtility.standardVerticalSpacing * 2;
                float minHeight = height * 10;
                height *= AnimatorTagEditorHelper.TAGS.Count + 1;
                return new Vector2(200, Mathf.Min(height, minHeight) + EditorGUIUtility.standardVerticalSpacing * 4);
            }

            public override void OnGUI(Rect rect)
            {
                GUILayout.BeginArea(rect);
                scrollview = GUILayout.BeginScrollView(scrollview);

                if (GUILayout.Button(new GUIContent("Add Custom Tag...", "Create a New Custom Tag"), style, GUILayout.ExpandWidth(true)))
                {
                    onSelect?.Invoke("");
                    editorWindow.Close();
                }
                foreach (string key in AnimatorTagEditorHelper.TAGS.Keys)
                {
                    string tooltip = AnimatorTagEditorHelper.GetTooltip(key);
                    if (GUILayout.Button(new GUIContent(key, tooltip), "box", GUILayout.ExpandWidth(true)))
                    {
                        onSelect?.Invoke(key);
                        editorWindow.Close();
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }
    }
}