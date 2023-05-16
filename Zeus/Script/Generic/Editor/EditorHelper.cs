using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Zeus
{
    public class EditorHelper : Editor
    {
        /// <summary>
        /// Get PropertyName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyLambda">You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'</param>
        /// <returns></returns>
        public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            return me.Member.Name;
        }

        /// <summary>
        /// Check if type is a <see cref="UnityEngine.Events.UnityEvent"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsUnityEventyType(Type type)
        {
            if (type.Equals(typeof(UnityEngine.Events.UnityEvent))) return true;
            if (type.BaseType.Equals(typeof(UnityEngine.Events.UnityEvent))) return true;
            if (type.Name.Contains("UnityEvent") || type.BaseType.Name.Contains("UnityEvent")) return true;
            return false;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(zMonoBehaviour), true)]
    public class EditorBase : Editor
    {
        #region Variables   
        public string[] IgnoreEvents;
        public string[] NotEventProperties;
        public string[] Ignore_Mono = new string[] { "_openCloseWindow", "_openCloseEvents", "_selectedToolbar" };
        public ClassHeaderAttribute HeaderAttribute;
        public GUISkin Skin;
        public Texture2D M_Logo;
        public List<ToolBar> Toolbars;

        #endregion

        public class ToolBar
        {
            public string Title;
            public bool UseIcon;
            public string IconName;
            public List<string> Variables;
            public int Order;
            public ToolBar()
            {
                Title = string.Empty;
                Variables = new List<string>();
            }
        }
       
        protected virtual void OnEnable()
        {
         
            var targetObject = serializedObject.targetObject;
            var hasAttributeHeader = targetObject.GetType().IsDefined(typeof(ClassHeaderAttribute), true);
            if (hasAttributeHeader)
            {
                var attributes = Attribute.GetCustomAttributes(targetObject.GetType(), typeof(ClassHeaderAttribute), true);
                if (attributes.Length > 0)
                    HeaderAttribute = (ClassHeaderAttribute)attributes[0];
            }

            Skin = Resources.Load("zSkin") as GUISkin;
            M_Logo = Resources.Load("icon_v2") as Texture2D;
            var prop = serializedObject.GetIterator();

            if (((zMonoBehaviour)target) != null)
            {               
                List<string> events = new List<string>();

                Toolbars = new List<ToolBar>();
                var toolbar = new ToolBar();
                toolbar.Title = "Default";

                Toolbars.Add(toolbar);
                var index = 0;
                bool needReorder = false;
                int oldOrder = 0;
               
                while (prop.NextVisible(true))
                {
                    FieldInfo fieldInfo = null;
                    if (!targetObject.TryGetField( prop.name,out fieldInfo))
                    {                       
                        continue;
                    }

                 
                    if (fieldInfo != null)
                    {
                       
                        var toolBarAttributes = fieldInfo.GetCustomAttributes(typeof(EditorToolbarAttribute), true);

                        if (toolBarAttributes.Length > 0)
                        {
                            var attribute = toolBarAttributes[0] as EditorToolbarAttribute;
                            var _toolbar = Toolbars.Find(tool => tool != null && tool.Title == attribute.Title);

                            if (_toolbar == null)
                            {
                                toolbar = new ToolBar();
                                toolbar.Title = attribute.Title;
                                toolbar.UseIcon = attribute.UseIcon;
                                toolbar.IconName = attribute.Icon;
                                Toolbars.Add(toolbar);
                                toolbar.Order = attribute.order;
                                if (oldOrder < attribute.order) needReorder = true;
                                index = Toolbars.Count - 1;

                            }
                            else
                            {
                                index = Toolbars.IndexOf(_toolbar);
                                if (index < Toolbars.Count)
                                {
                                    if (attribute.OverrideChildOrder)
                                    {
                                        if (oldOrder < attribute.order) needReorder = true;
                                        Toolbars[index].Order = attribute.order;
                                    }
                                    if (attribute.OverrideIcon)
                                    {
                                        Toolbars[index].UseIcon = true;
                                        Toolbars[index].IconName = attribute.Icon;
                                    }
                                }
                            }
                        }
                        if (index < Toolbars.Count)
                        {
                            Toolbars[index].Variables.Add(prop.name);
                        }

                        if ((EditorHelper.IsUnityEventyType(fieldInfo.FieldType) && !events.Contains(fieldInfo.Name)))
                        {
                            events.Add(fieldInfo.Name);
                        }
                    }
                }
                if (needReorder)
                    Toolbars.Sort((a, b) => a.Order.CompareTo(b.Order));
                var nullToolBar = Toolbars.FindAll(tool => tool != null && (tool.Variables == null || tool.Variables.Count == 0));
                for (int i = 0; i < nullToolBar.Count; i++)
                {
                    if (Toolbars.Contains(nullToolBar[i]))
                        Toolbars.Remove(nullToolBar[i]);
                }

                IgnoreEvents = events.ZToArray();
                if (HeaderAttribute != null)
                    M_Logo = Resources.Load(HeaderAttribute.IconName) as Texture2D;
                //else HeaderAttribute = new vClassHeaderAttribute(target.GetType().Name);
            }
        }

        protected bool OpenCloseWindow
        {
            get
            {
                return serializedObject.FindProperty("_openCloseWindow").boolValue;
            }
            set
            {
                var _OpenClose = serializedObject.FindProperty("_openCloseWindow");
                if (_OpenClose != null && value != _OpenClose.boolValue)
                {
                    _OpenClose.boolValue = value;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        protected bool OpenCloseEventsProperty
        {
            get
            {
                var _OpenCloseEvents = serializedObject.FindProperty("_openCloseEvents");
                return _OpenCloseEvents != null ? _OpenCloseEvents.boolValue : false;
            }
            set
            {
                var _OpenCloseEvents = serializedObject.FindProperty("_openCloseEvents");
                if (_OpenCloseEvents != null && value != _OpenCloseEvents.boolValue)
                {
                    _OpenCloseEvents.boolValue = value;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        protected int SelectedToolBar
        {
            get
            {
                var _selectedToolBar = serializedObject.FindProperty("_selectedToolbar");
                return _selectedToolBar != null ? _selectedToolBar.intValue : 0;
            }
            set
            {
                var _selectedToolBar = serializedObject.FindProperty("_selectedToolbar");

                if (_selectedToolBar != null && value != _selectedToolBar.intValue)
                {
                    _selectedToolBar.intValue = value;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (Toolbars != null && Toolbars.Count > 1)
            {
                GUILayout.BeginVertical(HeaderAttribute != null ? HeaderAttribute.Header : target.GetType().Name, Skin.window);

                GUILayout.Label(M_Logo, Skin.label, GUILayout.MaxHeight(25));

                if (HeaderAttribute.OpenClose)
                {
                    OpenCloseWindow = GUILayout.Toggle(OpenCloseWindow, OpenCloseWindow ? "Close Properties" : "Open Properties", EditorStyles.toolbarButton);
                }

                if (!HeaderAttribute.OpenClose || OpenCloseWindow)
                {
                    var titles = getToobarTitles();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

                    var customToolbar = Skin.GetStyle("customToolbar");                   
                   
                    if (HeaderAttribute.UseHelpBox)
                    {
                        EditorStyles.helpBox.richText = true;
                        EditorGUILayout.HelpBox(HeaderAttribute.HelpBoxText, MessageType.Info);
                    }
                       
                    GUILayout.Space(10);
                    SelectedToolBar = GUILayout.SelectionGrid(SelectedToolBar, titles, titles.Length > 2 ? 3 : titles.Length, customToolbar, GUILayout.MinWidth(250));
                    if (!(SelectedToolBar < Toolbars.Count)) SelectedToolBar = 0;
                    GUILayout.Space(10);
                    //GUILayout.Box(Toolbars[selectedToolBar].title, skin.box, GUILayout.ExpandWidth(true));
                    var ignore = getIgnoreProperties(Toolbars[SelectedToolBar]);
                    var ignoreProperties = ignore.Append(Ignore_Mono);
                    DrawPropertiesExcluding(serializedObject, ignoreProperties);
                    AdditionalGUI();
                }               
                GUILayout.EndVertical();
            }
            else
            {
                if (HeaderAttribute == null)
                {
                    if (((zMonoBehaviour)target) != null)
                        DrawPropertiesExcluding(serializedObject, Ignore_Mono);
                    else
                        base.OnInspectorGUI();
                    AdditionalGUI();
                }
                else
                {
                    GUILayout.BeginVertical(HeaderAttribute.Header, Skin.window);
                    GUILayout.Label(M_Logo, Skin.label, GUILayout.MaxHeight(25));
                    if (HeaderAttribute.OpenClose)
                    {
                        OpenCloseWindow = GUILayout.Toggle(OpenCloseWindow, OpenCloseWindow ? "Close Properties" : "Open Properties", EditorStyles.toolbarButton);
                    }

                    if (!HeaderAttribute.OpenClose || OpenCloseWindow)
                    {
                        if (HeaderAttribute.UseHelpBox)
                        {
                            EditorStyles.helpBox.richText = true;                           
                            EditorGUILayout.HelpBox(HeaderAttribute.HelpBoxText, MessageType.Info);
                        }                           

                        if (IgnoreEvents != null && IgnoreEvents.Length > 0)
                        {
                            var ignoreProperties = IgnoreEvents.Append(Ignore_Mono);
                            DrawPropertiesExcluding(serializedObject, ignoreProperties);
                            OpenCloseEventsProperty = GUILayout.Toggle(OpenCloseEventsProperty, (OpenCloseEventsProperty ? "Close " : "Open ") + "Events ", Skin.button);

                            if (OpenCloseEventsProperty)
                            {
                                foreach (string propName in IgnoreEvents)
                                {
                                    var prop = serializedObject.FindProperty(propName);
                                    if (prop != null)
                                        EditorGUILayout.PropertyField(prop);
                                }
                            }
                        }
                        else
                        {
                            var ignoreProperties = IgnoreEvents.Append(Ignore_Mono);
                            DrawPropertiesExcluding(serializedObject, ignoreProperties);
                        }
                    }
                    AdditionalGUI();
                    EditorGUILayout.EndVertical();
                }
            }

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }

        public GUIContent[] getToobarTitles()
        {
            List<GUIContent> props = new List<GUIContent>();
            for (int i = 0; i < Toolbars.Count; i++)
            {
                if (Toolbars[i] != null)
                {
                    Texture icon = null;
                    var _icon = Resources.Load(Toolbars[i].IconName);
                    if (_icon) icon = _icon as Texture;
                    GUIContent content = new GUIContent(Toolbars[i].UseIcon ? "" : Toolbars[i].Title, icon, "");

                    props.Add(content);
                }

            }
            return props.ZToArray();
        }

        public string[] getIgnoreProperties(ToolBar toolbar)
        {
            List<string> props = new List<string>();
            for (int i = 0; i < Toolbars.Count; i++)
            {
                if (Toolbars[i] != null && toolbar != null && toolbar.Variables != null)
                {
                    for (int a = 0; a < Toolbars[i].Variables.Count; a++)
                    {
                        if (!props.Contains(Toolbars[i].Variables[a]) && !toolbar.Variables.Contains(Toolbars[i].Variables[a]))
                        {
                            props.Add(Toolbars[i].Variables[a]);
                        }
                    }
                }
            }

            props.Add("m_Script");
            return props.ZToArray();
        }

        protected virtual void AdditionalGUI()
        {

        }
    }
}