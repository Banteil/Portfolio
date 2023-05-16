using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Zeus
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CombatManager), true)]
    public class CombatManagerEditor : EditorBase
    {
        #region variables
        CombatManager _manager;
        Animator _animator;

        int _selectedID;
        int _seletedHitColliderIndex;
        int _toolBarBodyMembers;
        int _damagePercentage;

        bool _selLeftArm, _selRightArm, _selLeftLeg, _selRightLeg, _selHead, _selTorso;
        bool _showEvents;
        bool _showDefaultInfo;
        bool _inAddBodyMember;
        bool _isHuman;
        bool _inCreateHitBox;
        bool _inChangeHitBoxCollider;

        Transform _leftLowerArm, _rightLowerArm, _leftLowerLeg, _rightLowerLeg,
            _leftUpperArm, _rightUpperArm, _leftUpperLeg, _rightUpperLeg, _chest, _head;
        BodyMember _currentBodyMember;
        BodyMember _extraBodyMember;
        Component _hitCollider;

        HitBoxType _triggerType;

        string _seletedBone;
        string[] _ignoreProperties = new string[] { "m_Script", "Members", "DefaultDamage", "HitProperties", "LeftWeapon", "RightWeapon", "OnDamageHitEvent", "OnRecoilHitEvent", "_openCloseWindow", "_openCloseEvents", "_selectedToolbar", "OnEquipWeapon" };

        #endregion

        void OnSceneGUI()
        {
            if (_manager == null) return;
            var renderers = _manager.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                EditorUtility.SetSelectedRenderState(renderer, EditorSelectedRenderState.Hidden);
            }

            DrawRecoilRange();
        }

        protected override void OnEnable()
        {
            _manager = (CombatManager)target;
            base.OnEnable();
            M_Logo = Resources.Load("meleeIcon") as Texture2D;
            if (!_manager.gameObject.scene.IsValid())
            {
                return;
            }

            CreateDefaultBodyMembers();
            CheckMembersName(_manager.Members);
        }

        void UnpackPrefab(Transform tr)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(tr.gameObject))
            {
                if (tr.GetComponent<MonoBehaviour>() != null) return;
                PrefabUtility.UnpackPrefabInstance(tr.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }

            for (int i = 0; i < tr.childCount; i++)
            {
                UnpackPrefab(tr.GetChild(i));
            }
        }

        void ResetBodyMembers()
        {
            var rootTr = _manager.transform;
            if (PrefabUtility.IsPartOfPrefabInstance(rootTr.gameObject))
            {
                PrefabUtility.UnpackPrefabInstance(rootTr.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }
            UnpackPrefab(rootTr);

            var hitColliders = _manager.GetComponentsInChildren<HitCollider>();
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.HitDeterminationColliders != null)
                {
                    foreach (var col in hitCollider.HitDeterminationColliders)
                    {
                        if (col != null)
                            DestroyImmediate(col.gameObject);
                    }
                }
                DestroyImmediate(hitCollider);
            }
            CreateDefaultBodyMembers();
        }

        void CreateDefaultBodyMembers()
        {
            _animator = _manager.GetComponent<Animator>();

            if (_animator && _animator.isHuman)
            {
                _leftLowerArm = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                CheckSingleHitBox(_leftLowerArm, HumanBones.LeftLowerArm);
                _rightLowerArm = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                CheckSingleHitBox(_rightLowerArm, HumanBones.RightLowerArm);
                _leftLowerLeg = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                CheckSingleHitBox(_leftLowerLeg, HumanBones.LeftLowerLeg);
                _rightLowerLeg = _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                CheckSingleHitBox(_rightLowerLeg, HumanBones.RightLowerLeg);
                _leftUpperArm = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                CheckSingleHitBox(_leftUpperArm, HumanBones.LeftUpperArm);
                _rightUpperArm = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                CheckSingleHitBox(_rightUpperArm, HumanBones.RightUpperArm);
                _leftUpperLeg = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                CheckSingleHitBox(_leftUpperLeg, HumanBones.LeftUpperLeg);
                _rightUpperLeg = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                CheckSingleHitBox(_rightUpperLeg, HumanBones.RightUpperLeg);
                _chest = _animator.GetBoneTransform(HumanBodyBones.Chest);
                CheckSingleHitBox(_chest, HumanBones.Chest);
                _head = _animator.GetBoneTransform(HumanBodyBones.Head);
                CheckSingleHitBox(_head, HumanBones.Head);
            }
        }

        void CheckMembersName(List<BodyMember> Members)
        {
            foreach (var member in Members)
            {
                if (member.HitCollider)
                {
                    member.HitCollider.HitColliderName = member.BodyPart;
                }

                if (member.AttackObject)
                {
                    member.AttackObject.AttackObjectName = member.BodyPart;
                }
            }
        }

        void CheckSingleHitBox(Transform transform, HumanBones bodyPart, bool debug = false)
        {
            if (transform)
            {
                HitCollider hitCollider = transform.GetComponent<HitCollider>();
                if (hitCollider == null)
                {
                    hitCollider = transform.gameObject.AddComponent<HitCollider>();
                    transform.gameObject.layer = LayerMask.NameToLayer("Hit");
                    hitCollider.Owner = _manager.GetComponent<Character>();
                }

                if (_manager && _manager.Members != null)
                {
                    var bodyMembers = _manager.Members.FindAll(member => member.BodyPart == bodyPart.ToString());
                    if (bodyMembers.Count > 0)
                    {
                        bodyMembers[0].IsHuman = true;
                        bodyMembers[0].HitCollider = hitCollider;
                        bodyMembers[0].BodyPart = bodyPart.ToString();
                        bodyMembers[0].Transform = transform;
                        if (bodyMembers.Count > 1)
                        {
                            for (int i = 1; i < bodyMembers.Count; i++)
                            {
                                _manager.Members.Remove(bodyMembers[i]);
                            }
                        }
                        CheckHitColliders(bodyMembers[0], true);
                        EditorUtility.SetDirty(_manager);
                    }
                    else
                    {
                        BodyMember bodyMember = new BodyMember();
                        bodyMember.IsHuman = true;
                        bodyMember.HitCollider = hitCollider;
                        bodyMember.BodyPart = bodyPart.ToString();
                        bodyMember.Transform = transform;
                        _manager.Members.Add(bodyMember);
                        CheckHitColliders(bodyMember, true);
                        EditorUtility.SetDirty(_manager);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            var oldSkin = GUI.skin;

            GUI.skin = Skin;

            var script = serializedObject.FindProperty("m_Script");

            GUILayout.BeginVertical("COMBAT MANAGER", "window");
            GUILayout.Label(M_Logo, GUILayout.MaxHeight(25));

            OpenCloseWindow = GUILayout.Toggle(OpenCloseWindow, OpenCloseWindow ? "Close" : "Open", EditorStyles.toolbarButton);
            if (OpenCloseWindow)
            {
                if (script != null)
                {
                    EditorGUILayout.PropertyField(script);
                }

                GUI.enabled = !AssetDatabase.Contains(_manager.gameObject);
                if (_manager.Members == null || _manager.Members.Count == 0)
                {
                    if (GUILayout.Button("Create Default Body Members", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
                    {
                        CreateDefaultBodyMembers();
                    }
                }
                GUILayout.BeginVertical("box");

                OpenCloseDefaultInfo();
                OpenCloseEvents(oldSkin);
                AddExtraBodyPart();
                //GUI.enabled = true;

                GUILayout.EndVertical();

                var seletedBodyMember = _manager.Members.Find(member => member.BodyPart == _seletedBone);
                GUILayout.BeginVertical(seletedBodyMember != null ? "highlightBox" : "box");
                DrawBodyMemberToogles();
                if (seletedBodyMember != null)
                {
                    bool canRemove = seletedBodyMember.BodyPart != HumanBones.LeftLowerArm.ToString() && seletedBodyMember.BodyPart != HumanBones.RightLowerArm.ToString() &&
                                     seletedBodyMember.BodyPart != HumanBones.LeftLowerLeg.ToString() && seletedBodyMember.BodyPart != HumanBones.RightLowerLeg.ToString();
                    DrawBodyMember(ref seletedBodyMember, seletedBodyMember.BodyPart.ToString(), canRemove);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                GUILayout.Label("Who you can Hit?", GUILayout.ExpandWidth(true));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HitProperties"), true);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                GUILayout.Label("Weapons");
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical("box");
                GUILayout.Label("LeftWeapon", EditorStyles.miniLabel);
                _manager.LeftWeapon = EditorGUILayout.ObjectField(_manager.LeftWeapon, typeof(AttackObject), true) as AttackObject;
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                GUILayout.Label("RightWeapon", EditorStyles.miniLabel);
                _manager.RightWeapon = EditorGUILayout.ObjectField(_manager.RightWeapon, typeof(AttackObject), true) as AttackObject;
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                if (GUILayout.Button("Reset Body Member Setting", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
                {
                    ResetBodyMembers();
                }
            }
            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        void OpenCloseEvents(GUISkin oldSkin)
        {
            var onDamageHit = serializedObject.FindProperty("OnDamageHitEvent");
            var onRecoilHit = serializedObject.FindProperty("OnRecoilHitEvent");
            var onEquipWeapon = serializedObject.FindProperty("OnEquipWeapon");

            GUILayout.BeginVertical(_showEvents ? "highlightBox" : "box");
            _showEvents = GUILayout.Toggle(_showEvents, _showEvents ? "Close Events" : "Open Events", EditorStyles.miniButton);
            GUI.skin = oldSkin;
            if (_showEvents)
            {
                if (onDamageHit != null)
                {
                    EditorGUILayout.PropertyField(onDamageHit);
                }

                if (onRecoilHit != null)
                {
                    EditorGUILayout.PropertyField(onRecoilHit);
                }

                if (onEquipWeapon != null)
                {
                    EditorGUILayout.PropertyField(onEquipWeapon);
                }
            }
            GUI.skin = Skin;
            GUILayout.EndVertical();
        }

        void OpenCloseDefaultInfo()
        {
            GUILayout.BeginVertical(_showDefaultInfo ? "highlightBox" : "box");

            _showDefaultInfo = GUILayout.Toggle(_showDefaultInfo, _showDefaultInfo ? "Close Default Info" : "Open Default Info", EditorStyles.miniButton);
            var oldSkin = GUI.skin;
            GUI.skin = oldSkin;
            if (_showDefaultInfo)
            {
                //_manager.DefaultDamage.DamageValue = EditorGUILayout.FloatField("DefaultDamage", _manager.DefaultDamage.DamageValue);
                DrawPropertiesExcluding(serializedObject, _ignoreProperties);
            }
            GUI.skin = Skin;
            GUILayout.EndVertical();
        }

        void AddExtraBodyPart()
        {
            GUILayout.BeginVertical(_inAddBodyMember ? "highlightBox" : "box");
            if (!_inAddBodyMember && GUILayout.Button("Add Extra Body Member", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
            {
                _extraBodyMember = new BodyMember();
                _inAddBodyMember = true;
                _isHuman = true;
            }
            if (_inAddBodyMember)
            {
                DrawAddExtraBodyMember();
            }

            GUILayout.EndVertical();
        }

        void DrawRecoilRange()
        {
            var coll = _manager.gameObject.GetComponent<Collider>();
            if (coll != null && _manager != null && _manager.HitProperties != null && _manager.HitProperties.UseRecoil && _manager.HitProperties.DrawRecoilGizmos)
            {
                Handles.DrawWireDisc(coll.bounds.center, Vector3.up, 0.5f);
                Handles.color = new Color(1, 0, 0, 0.2f);
                Handles.DrawSolidArc(coll.bounds.center, Vector3.up, _manager.transform.forward, _manager.HitProperties.RecoilRange, 0.5f);
                Handles.DrawSolidArc(coll.bounds.center, Vector3.up, _manager.transform.forward, (float)-_manager.HitProperties.RecoilRange, 0.5f);
            }
        }

        void DrawBodyMemberToogles()
        {
            var bmleftLowerArm = _manager.Members.Find(member => member.BodyPart == HumanBones.LeftLowerArm.ToString());
            var bmrightLowerArm = _manager.Members.Find(member => member.BodyPart == HumanBones.RightLowerArm.ToString());
            var bmleftLowerLeg = _manager.Members.Find(member => member.BodyPart == HumanBones.LeftLowerLeg.ToString());
            var bmrightLowerLeg = _manager.Members.Find(member => member.BodyPart == HumanBones.RightLowerLeg.ToString());

            GUILayout.BeginVertical();
            GUILayout.Label("Body Members", GUILayout.ExpandWidth(true));

            GUILayout.EndVertical();
            // GUILayout.Box("Default Human Body Members", GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            if (bmleftLowerArm != null)
            {
                BodyMemberToogle(bmleftLowerArm.BodyPart, ref bmleftLowerArm, "LeftLowerArm");
            }

            if (bmrightLowerArm != null)
            {
                BodyMemberToogle(bmrightLowerArm.BodyPart, ref bmrightLowerArm, "RightLowerArm");
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (bmleftLowerLeg != null)
            {
                BodyMemberToogle(bmleftLowerLeg.BodyPart, ref bmleftLowerLeg, "LeftLowerLeg");
            }

            if (bmrightLowerLeg != null)
            {
                BodyMemberToogle(bmrightLowerLeg.BodyPart, ref bmrightLowerLeg, "RightLowerLeg");
            }

            GUILayout.EndHorizontal();

            //GUILayout.Box("Extra Human BodyMembers", GUILayout.ExpandWidth(true));
            for (int i = 0; i < _manager.Members.Count; i++)
            {
                if (_manager.Members[i] != bmleftLowerArm && _manager.Members[i] != bmrightLowerArm &&
                    _manager.Members[i] != bmleftLowerLeg && _manager.Members[i] != bmrightLowerLeg)
                {
                    var bodyMember = _manager.Members[i];
                    BodyMemberToogle(bodyMember.BodyPart, ref bodyMember, bodyMember.BodyPart.ToString());
                    CheckHitColliders(_manager.Members[i]);
                }
                else
                {
                    CheckHitColliders(_manager.Members[i], true);
                }
            }
        }

        void CheckHitColliders(BodyMember bodyMember, bool isDefault = false)
        {
            if (AssetDatabase.Contains(_manager.gameObject))
            {
                return;
            }

            var hitColliders = bodyMember.Transform.GetComponentsInChildren<Collider>();
            var _result = hitColliders.ZToList().FindAll(hitBox => (hitBox.transform.parent.Equals(bodyMember.Transform)) && hitBox.gameObject.name.Equals("HitCollider"));
            if (_result.Count > 0)
            {
                if (bodyMember.HitCollider) bodyMember.HitCollider.HitDeterminationColliders = _result;
            }
            else
            {
                var hitCollider = new GameObject("HitCollider", typeof(BoxCollider));
                var scale = Vector3.one * 0.15f;
                hitCollider.gameObject.layer = LayerMask.NameToLayer("Hit");
                hitCollider.GetComponent<BoxCollider>().isTrigger = true;
                if (isDefault)
                {
                    var lookDir = bodyMember.Transform.GetChild(0).position - bodyMember.Transform.position;
                    var rotation = Quaternion.LookRotation(lookDir);
                    scale.z = Vector3.Distance(bodyMember.Transform.position, bodyMember.Transform.GetChild(0).position);
                    var point = bodyMember.Transform.position + (lookDir.normalized) * (scale.z * 0.7f);
                    hitCollider.transform.position = point;
                    hitCollider.transform.rotation = rotation;
                    hitCollider.transform.localScale = scale;
                    hitCollider.transform.parent = bodyMember.Transform;
                }
                else
                {
                    hitCollider.transform.localScale = scale;
                    hitCollider.transform.parent = bodyMember.Transform;
                    hitCollider.transform.localPosition = Vector3.zero;
                    hitCollider.transform.localEulerAngles = Vector3.zero;
                }
            }
        }

        static void CalculateDirection(Vector3 point, out int direction, out float distance)
        {
            // Calculate longest axis
            direction = 0;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
            {
                direction = 1;
            }

            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
            {
                direction = 2;
            }

            distance = point[direction];
        }

        void DrawAddExtraBodyMember()
        {
            if (_extraBodyMember != null)
            {
                _isHuman = Convert.ToBoolean(EditorGUILayout.Popup("Member Type", Convert.ToInt32(_isHuman), new string[] { "Generic", "Human" }));
                _extraBodyMember.IsHuman = _isHuman;
                if (_isHuman)
                {
                    HumanBones humanBone = 0;
                    try
                    {
                        humanBone = (HumanBones)Enum.Parse(typeof(HumanBones), _extraBodyMember.BodyPart);
                    }
                    catch { }
                    humanBone = (HumanBones)EditorGUILayout.EnumPopup("Body Part", humanBone);
                    _extraBodyMember.BodyPart = humanBone.ToString();
                    var humanBodyBone = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), _extraBodyMember.BodyPart);
                    _extraBodyMember.Transform = _manager.GetComponent<Animator>().GetBoneTransform(humanBodyBone);
                }
                else
                {
                    _extraBodyMember.BodyPart = EditorGUILayout.TextField("BodyPart Name", _extraBodyMember.BodyPart);
                }

                _extraBodyMember.Transform = EditorGUILayout.ObjectField("Body Member", _extraBodyMember.Transform, typeof(Transform), true) as Transform;

                var valid = true;
                if (_extraBodyMember.Transform != null && _manager.Members.Find(member => member.Transform == _extraBodyMember.Transform) != null)
                {
                    EditorGUILayout.HelpBox("This Body Member already exists, select another", MessageType.Error);
                    valid = false;
                }

                if (_manager.Members.Find(member => member.BodyPart == _extraBodyMember.BodyPart) != null)
                {
                    EditorGUILayout.HelpBox("This Body Part already exists, select another", MessageType.Error);
                    valid = false;
                }
                GUILayout.BeginHorizontal();
                if (valid)
                {
                    if (GUILayout.Button("Create", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
                    {
                        BodyMember member = new BodyMember();
                        member.HitCollider = _extraBodyMember.Transform.gameObject.AddComponent<HitCollider>();
                        _extraBodyMember.Transform.gameObject.layer = LayerMask.NameToLayer("Hit");
                        member.HitCollider.Owner = _manager.GetComponent<Character>();
                        member.Transform = _extraBodyMember.Transform;
                        member.BodyPart = _extraBodyMember.BodyPart;

                        var hitObject = new GameObject("HitCollider", typeof(BoxCollider));
                        hitObject.transform.localScale = Vector3.one * 0.2f;
                        hitObject.transform.parent = member.Transform;
                        hitObject.transform.localPosition = Vector3.zero;
                        hitObject.transform.localEulerAngles = Vector3.zero;

                        var hitCol = hitObject.GetComponent<BoxCollider>();
                        hitCol.gameObject.layer = LayerMask.NameToLayer("Hit");
                        hitCol.isTrigger = true;
                        member.HitCollider.HitDeterminationColliders = new List<Collider>();
                        member.HitCollider.HitDeterminationColliders.Add(hitCol);
                        member.HitCollider.HitColliderName = member.BodyPart;
                        _inCreateHitBox = false;
                        _manager.Members.Add(member);
                        _extraBodyMember = null;
                        _inAddBodyMember = false;
                    }
                }
                if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
                {
                    _extraBodyMember = null;
                    _inAddBodyMember = false;
                }
                GUILayout.EndHorizontal();
            }
        }

        void DrawBodyMember(ref BodyMember bodyMember, string name, bool canRemove = false)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            //GUILayout.Box("Selected " + name, GUILayout.ExpandWidth(true));
            if (canRemove && GUILayout.Button("X"))
            {
                var hitColliders = bodyMember.HitCollider.HitDeterminationColliders;
                for (int i = 0; i < hitColliders.Count; i++)
                {
                    DestroyImmediate(hitColliders[i].gameObject);
                }
                DestroyImmediate(bodyMember.HitCollider.gameObject);
                _manager.Members.Remove(bodyMember);
            }
            GUILayout.EndHorizontal();
            bodyMember.HitCollider = EditorGUILayout.ObjectField("Hit Collider", bodyMember.HitCollider, typeof(HitCollider), true) as HitCollider;
            GUILayout.Box("Hit Determination Colliders", GUILayout.ExpandWidth(true));
            DrawHitCollidersList(ref bodyMember);
            GUILayout.EndVertical();
        }

        void DrawHitCollidersList(ref BodyMember bodyMember)
        {
            var hitCollider = bodyMember.HitCollider;
            if (hitCollider != null && hitCollider.HitDeterminationColliders != null)
            {
                for (int i = 0; i < hitCollider.HitDeterminationColliders.Count; i++)
                {
                    try
                    {
                        GUILayout.BeginHorizontal();
                        if (hitCollider.HitDeterminationColliders[i] != null && hitCollider.HitDeterminationColliders[i].transform == hitCollider.transform ||
                        (hitCollider.GetComponent<BoxCollider>() != null))
                        {
                            DestroyImmediate(hitCollider.GetComponent<BoxCollider>());
                            hitCollider.HitDeterminationColliders.RemoveAt(i);
                            GUILayout.EndHorizontal();
                            break;
                        }
                        Color color = GUI.color;
                        GUI.color = _seletedHitColliderIndex == i ? new Color(1, 1, 0, 0.6f) : color;

                        if (GUILayout.Button("Hit Collider " + (i + 1), EditorStyles.miniButton))
                        {
                            if (_seletedHitColliderIndex == i)
                            {
                                _seletedHitColliderIndex = -1;
                            }
                            else
                            {
                                _seletedHitColliderIndex = i;
                            }
                        }
                        GUI.color = color;
                        if (hitCollider.HitDeterminationColliders.Count > 1 && GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            if (hitCollider.HitDeterminationColliders[i] != null && hitCollider.HitDeterminationColliders[i].transform != hitCollider.transform)
                            {
                                DestroyImmediate(hitCollider.HitDeterminationColliders[i].gameObject);
                            }
                            hitCollider.HitDeterminationColliders.RemoveAt(i);
                            GUILayout.EndHorizontal();
                            break;
                        }
                        GUILayout.EndHorizontal();
                    }
                    catch { }
                }
            }

            if (_seletedHitColliderIndex > -1 && _seletedHitColliderIndex < hitCollider.HitDeterminationColliders.Count)
            {
                GUILayout.BeginVertical("box");
                var hitBox = hitCollider.HitDeterminationColliders[_seletedHitColliderIndex];
                if (hitBox)
                {
                    EditorGUILayout.ObjectField("Selected Hit Collider" + (_seletedHitColliderIndex + 1), hitBox, typeof(HitBox), true);
                    //GUILayout.Box("Hit Settings", GUILayout.ExpandWidth(true));
                    if (GUI.changed)
                    {
                        EditorUtility.SetDirty(hitBox);
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.Space(10);

            if (!_inCreateHitBox && GUILayout.Button("Create New Hit Collider", EditorStyles.miniButton))
            {
                _inCreateHitBox = true;
                _damagePercentage = 100;
                _triggerType = HitBoxType.Damage | HitBoxType.Recoil;
            }
            if (_inCreateHitBox)
            {
                GUILayout.Box("New Hit Collider", GUILayout.ExpandWidth(true));
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Create", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
                {
                    bodyMember.HitCollider = bodyMember.Transform.gameObject.AddComponent<HitCollider>();
                    bodyMember.Transform.gameObject.layer = LayerMask.NameToLayer("Hit");
                    bodyMember.HitCollider.Owner = _manager.GetComponent<Character>();

                    var hitObject = new GameObject("HitCollider", typeof(BoxCollider));
                    hitObject.layer = LayerMask.NameToLayer("Hit");
                    hitObject.transform.localScale = Vector3.one * 0.2f;
                    hitObject.transform.parent = bodyMember.Transform;
                    hitObject.transform.localPosition = Vector3.zero;
                    hitObject.transform.localEulerAngles = Vector3.zero;

                    var hitBox = hitObject.GetComponent<BoxCollider>();
                    hitBox.isTrigger = true;
                    bodyMember.HitCollider.HitDeterminationColliders.Add(hitBox);
                    bodyMember.HitCollider.HitColliderName = bodyMember.BodyPart;
                    _inCreateHitBox = false;
                }

                if (GUILayout.Button("Cancel", EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
                {
                    _inCreateHitBox = false;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
        }

        void BodyMemberToogle(string bodyPart, ref BodyMember bodyMember, string name)
        {
            if (bodyMember != null)
            {
                Color color = GUI.color;
                GUI.color = _seletedBone == bodyPart ? new Color(1, 1, 0, 0.6f) : color;
                if (GUILayout.Button(name, EditorStyles.miniButton, GUILayout.ExpandWidth(true)))
                {
                    if (_seletedBone == bodyPart)
                    {
                        _seletedBone = "null";
                    }
                    else
                    {
                        _seletedBone = bodyPart;
                    }

                    _seletedHitColliderIndex = -1;
                    Repaint();
                }
                GUI.color = color;
                if (bodyMember.HitCollider)
                {
                    foreach (var hitCollider in bodyMember.HitCollider.HitDeterminationColliders)
                    {
                        if (hitCollider != null)
                        {
                            hitCollider.gameObject.tag = _manager.gameObject.tag;
                            hitCollider.gameObject.layer = LayerMask.NameToLayer("Hit");
                        }
                    }
                }

            }
        }

    }
}