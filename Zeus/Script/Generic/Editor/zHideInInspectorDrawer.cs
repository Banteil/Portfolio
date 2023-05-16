using UnityEditor;
using UnityEngine;

namespace Zeus
{
    [CustomPropertyDrawer(typeof(zHideInInspectorAttribute),true)]
    public class zHideInInspectorDrawer : PropertyDrawer
    {       
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            zHideInInspectorAttribute _attribute = attribute as zHideInInspectorAttribute;
          
            if (_attribute != null && property.serializedObject.targetObject)
            {               
                var propertyName = property.propertyPath.Replace(property.name, "");
                var booleamProperties = _attribute.RefbooleanProperty.Split(';');             
                for (int i = 0; i < booleamProperties.Length; i++)
                {
                    var booleanProperty = property.serializedObject.FindProperty(propertyName + booleamProperties[i]);                  
                    if (booleanProperty != null)
                    {
                        _attribute.HideProperty = (bool)_attribute.InvertValue ? booleanProperty.boolValue : !booleanProperty.boolValue;
                        if (_attribute.HideProperty)
                        {
                            break;
                        }
                    }
                    else
                    {

                        EditorGUI.PropertyField(position, property,label, true);
                    }
                }
                if (!_attribute.HideProperty)
                {                  
                    EditorGUI.PropertyField(position, property, label, true);
                }                
            }
            else
                EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            zHideInInspectorAttribute _attribute = attribute as zHideInInspectorAttribute;
            if (_attribute != null)
            {
                var propertyName = property.propertyPath.Replace(property.name, "");
                var booleamProperties = _attribute.RefbooleanProperty.Split(';');
                var valid = true;
                for (int i = 0; i < booleamProperties.Length; i++)
                {
                    var booleamProperty = property.serializedObject.FindProperty(propertyName + booleamProperties[i]);
                    if (booleamProperty != null)
                    {
                        valid = _attribute.InvertValue ? !booleamProperty.boolValue : booleamProperty.boolValue;
                        if (!valid) break;
                    }
                }
                if (valid) return base.GetPropertyHeight(property, label);
                else return 0;
            }
            return base.GetPropertyHeight(property, label);
        }
       
    }
}