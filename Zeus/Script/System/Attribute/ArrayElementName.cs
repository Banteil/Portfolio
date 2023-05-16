using UnityEngine;
using UnityEditor;

namespace Zeus
{
    /// <summary>
    /// �迭 ����� �̸��� ����. 
    /// ������ ���̿� �迭�� ���̰� �ٸ��� error
    /// </summary>
    public class ArrayElementNameAttribute : PropertyAttribute
    {
        public readonly string[] Names;
        public ArrayElementNameAttribute(params string[] names)
        {
            this.Names = names;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ArrayElementNameAttribute))]
    public class ArrayElementNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                int index = int.Parse(property.propertyPath.Split('[', ']')[1]);
                EditorGUI.PropertyField(position, property, new GUIContent(((ArrayElementNameAttribute)attribute).Names[index]), true);
            }
            catch
            {
                EditorGUI.ObjectField(position, property, label);
            }
        }
    }
#endif
}