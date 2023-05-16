using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace Zeus
{
    public class CreateAIWindow : EditorWindow
    {
        [MenuItem("Zeus/FSM AI/Create New AI", false, 0)]
        public static void ShowWindow()
        {
            var window = CreateInstance<CreateAIWindow>();
            window.titleContent = new GUIContent("Create New AI");
            window.ShowUtility();
        }

        public GUISkin Skin;
        public GameObject SelectedPrefab;
        public UnityEngine.Object SelectedFSM;
        public Texture2D M_Logo;
        public int SelectedType;
        public Type TargetType;
        public Editor HumanoidPreview;
        RuntimeAnimatorController _controller;

        void OnEnable()
        {
            M_Logo = Resources.Load("icon_v2") as Texture2D;
        }

        private void OnGUI()
        {
            if (!Skin) Skin = Resources.Load("zSkin") as GUISkin;
            GUI.skin = Skin;
            GUILayout.BeginVertical("AI CREATOR WINDOW", "window");
            GUILayout.Label(M_Logo, GUILayout.MaxHeight(25));
            GUILayout.Space(5);
            SelectedPrefab = EditorGUILayout.ObjectField("Character Model", SelectedPrefab, typeof(GameObject), true) as GameObject;
            _controller = EditorGUILayout.ObjectField("Animator Controller: ", _controller, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
            SelectedFSM = EditorGUILayout.ObjectField("FSM Behaviour Controller", SelectedFSM, typeof(FSMBehaviour), false);
            if (SelectedFSM)
            {
                var requiredTypes = (SelectedFSM as FSMBehaviour).GetRequiredTypes();
                var types = FindDerivedTypes(typeof(IControlAI).Assembly, typeof(IControlAI), requiredTypes);

                if (types != null)
                {
                    var _types = types.Cast<Type>().ToList();

                    if (SelectedType >= _types.Count) SelectedType = 0;
                    if (_types.Count > 0)
                    {
                        var names = TypesToStringArray(_types);
                        SelectedType = EditorGUILayout.Popup("Type of Controller", SelectedType, names);
                        TargetType = _types[SelectedType];
                        if (SelectedPrefab)
                        {
                            bool hasController = (SelectedPrefab as GameObject).GetComponent<IControlAI>() != null;
                            var fsmBehaviour = (SelectedPrefab as GameObject).GetComponent<IFSMBehaviourController>();
                            Animator charAnimator = null;
                            if (SelectedPrefab)
                                charAnimator = (SelectedPrefab as GameObject).GetComponent<Animator>();
                            var charExist = charAnimator != null;
                            var isHuman = charExist ? charAnimator.isHuman : false;
                            var isValidAvatar = charExist ? isHuman && charAnimator.avatar.isValid : false;

                            if (hasController || fsmBehaviour != null)
                            {
                                this.minSize = new Vector2(400, 160);
                                this.maxSize = new Vector3(400, 160);
                                EditorGUILayout.HelpBox("Please select a Model without any AI Component", MessageType.Error);
                            }
                            else if (!isValidAvatar)
                            {
                                this.minSize = new Vector2(400, 160);
                                this.maxSize = new Vector3(400, 160);
                                string message = "";
                                if (!charExist)
                                    message += "*Missing a Animator Component";
                                else if (!isHuman)
                                    message += "\n*This is not a Humanoid";
                                else if (!isValidAvatar)
                                    message += "\n*" + SelectedPrefab.name + " is a invalid Humanoid";
                                EditorGUILayout.HelpBox(message, MessageType.Error);
                            }
                            else
                            {
                                if (HumanoidPreview == null || HumanoidPreview.target != SelectedPrefab)
                                {
                                    HumanoidPreview = Editor.CreateEditor(SelectedPrefab);
                                }
                                else
                                {
                                    this.minSize = new Vector2(400, 570);
                                    this.maxSize = new Vector3(400, 570);
                                    DrawHumanoidPreview();
                                }

                                if (isValidAvatar && _controller != null && GUILayout.Button("CREATE"))
                                {
                                    Create();
                                }
                            }
                        }
                        else
                        {
                            this.minSize = new Vector2(400, 100);
                            this.maxSize = new Vector3(400, 120);
                        }
                    }
                    else
                    {
                        this.minSize = new Vector2(400, 100);
                        this.maxSize = new Vector3(400, 120);
                    }
                }
            }
            else
            {
                this.minSize = new Vector2(400, 100);
                this.maxSize = new Vector3(400, 120);
            }
            GUILayout.EndVertical();
        }

        private GameObject InstantiateNewCharacter(GameObject selected)
        {
            if (selected == null) return selected;
            if (selected.scene.IsValid()) return selected;

            return PrefabUtility.InstantiatePrefab(selected) as GameObject;

        }
        /// <summary>
        /// Created the Third Person Controller
        /// </summary>
        public virtual void Create()
        {
            // base for the character
            GameObject newCharacter = InstantiateNewCharacter(SelectedPrefab);

            if (!newCharacter)
                return;
            if (newCharacter)
            {
                newCharacter.name = "AI_" + newCharacter.name;
                var t = newCharacter.AddComponent(TargetType);
                (t as IControlAI).CreatePrimaryComponents();
                DestroyImmediate(t);
                var fms = newCharacter.AddComponent<FSMBehaviourController>();
                fms.FsmBehaviour = SelectedFSM as FSMBehaviour;
                t = newCharacter.AddComponent(TargetType);
                if (_controller) newCharacter.GetComponent<Animator>().runtimeAnimatorController = _controller;
                newCharacter.tag = "Enemy";
                var aiLayer = LayerMask.NameToLayer("Character");
                newCharacter.layer = aiLayer;
                (t as IControlAI).CreateSecondaryComponents();
            }
            this.Close();
        }

        void DrawHumanoidPreview()
        {
            GUILayout.FlexibleSpace();

            if (HumanoidPreview != null)
            {
                HumanoidPreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100, 400), "window");
            }
        }

        public List<Type> GetValidTypes(List<Type> types, List<Type> requiredTypes)
        {
            List<Type> validTypes = new List<Type>();
            for (int i = 0; i < types.Count; i++)
            {
                if (IsValidType(types[i], requiredTypes) && !validTypes.Contains(types[i])) validTypes.Add(types[i]);
            }
            return validTypes;
        }

        public string[] TypesToStringArray(List<Type> types)
        {
            string[] names = new string[types.Count];
            for (int i = 0; i < types.Count; i++)
            {
                names[i] = types[i].Name;
            }
            return names;
        }

        public bool IsValidType(Type type, List<Type> types)
        {
            var interfaces = type.GetInterfaces();

            var typeCount = 0;
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (types.Contains(interfaces[i]))
                {
                    typeCount++;
                }
            }
            return typeCount == types.Count || types.Contains(type);
        }

        public IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType, List<Type> requiredTypes)
        {
            return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && (t.IsSubclassOf(typeof(MonoBehaviour)) || t.Equals(typeof(MonoBehaviour))) && IsValidType(t, requiredTypes));
        }
    }
}