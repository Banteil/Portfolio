using UnityEditor;
using UnityEngine;

namespace Zeus
{
    [CustomPropertyDrawer(typeof(Damage))]
    public class DamageDrawer : PropertyDrawer
    {
        public bool IsOpen;
        public bool Valid;
        GUISkin _skin;
        float _helpBoxHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var oldSkin = GUI.skin;
            if (!_skin) _skin = Resources.Load("zSkin") as GUISkin;
            if (_skin) GUI.skin = _skin;
            position = EditorGUI.IndentedRect(position);
            GUI.Box(position, "");
            position.width -= 10;
            position.height = 15;
            position.y += 5f;
            position.x += 5;
            IsOpen = GUI.Toggle(position, IsOpen, "Damage Options", EditorStyles.miniButton);

            if (IsOpen)
            {
                var attackName = property.FindPropertyRelative("DamageType");
                var value = property.FindPropertyRelative("DamageValue");
                var staminaBlockCost = property.FindPropertyRelative("StaminaBlockCost");
                var staminaRecoveryDelay = property.FindPropertyRelative("StaminaRecoveryDelay");
                var ignoreDefense = property.FindPropertyRelative("IgnoreDefense");
                var activeRagdoll = property.FindPropertyRelative("ActiveRagdoll");
                var hitreactionID = property.FindPropertyRelative("ReactionID");
                var hitrecoilID = property.FindPropertyRelative("RecoilID");
                var senselessTime = property.FindPropertyRelative("SenselessTime");
                var obj = (property.serializedObject.targetObject as MonoBehaviour);

                Valid = true;
                if (obj != null)
                {
                    var parent = obj.transform.parent;
                    if (parent != null)
                    {
                        var manager = parent.GetComponentInParent<CombatManager>();
                        Valid = !(obj.GetType() == typeof(MeleeWeapon) || obj.GetType().IsSubclassOf(typeof(MeleeWeapon))) || manager == null;
                    }
                }

                if (!Valid)
                {
                    position.y += 20;
                    var style = new GUIStyle(EditorStyles.helpBox);
                    var content = new GUIContent("Damage type and other options can be overridden by the Animator Attack State\nIf the weapon is used by a character with an ItemManager, the damage value can be overridden by the item attribute");
                    _helpBoxHeight = style.CalcHeight(content, position.width);
                    position.height = _helpBoxHeight;
                    GUI.Box(position, content.text, style);
                    position.y += _helpBoxHeight - 20;
                }
                position.height = EditorGUIUtility.singleLineHeight;
                if (attackName != null)
                {
                    position.y += 20;

                    EditorGUI.PropertyField(position, attackName);
                }
                if (value != null)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, value);
                }
                if (staminaBlockCost != null)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, staminaBlockCost);
                }
                if (staminaRecoveryDelay != null)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, staminaRecoveryDelay);
                }
                if (ignoreDefense != null && Valid)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, ignoreDefense);
                }
                if (activeRagdoll != null && Valid)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, activeRagdoll);
                    position.y += 20;
                    EditorGUI.PropertyField(position, senselessTime);
                }
                if (hitreactionID != null && Valid)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, hitreactionID);
                }
                if (hitrecoilID != null && Valid)
                {
                    position.y += 20;
                    EditorGUI.PropertyField(position, hitrecoilID);
                }
            }

            GUI.skin = oldSkin;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return !IsOpen ? 25 : (Valid ? 210 : 130 + _helpBoxHeight);
        }
    }
}