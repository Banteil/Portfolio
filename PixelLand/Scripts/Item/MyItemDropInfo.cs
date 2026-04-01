using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "new MyItemDropInfo", menuName = "PixelLand/Data/MyItemDropInfo", order = 0)]
public class MyItemDropInfo : ScriptableObject
{
    [SerializeField]
    List<ItemDropInfo> _dropInfos;
    public List<ItemDropInfo> DropInfos { get { return _dropInfos; } set { _dropInfos = value; } }

    [SerializeField]
    int _minMoney;
    public int MinMoney { get { return _minMoney; } set { _minMoney = value; } }
    [SerializeField]
    int _maxMoney;
    public int MaxMoney { get { return _maxMoney; } set { _maxMoney = value; } }

    public void ApplyDirty()
    {
        EditorUtility.SetDirty(this);
    }
}

[CustomEditor(typeof(MyItemDropInfo))]
public class MyItemDropInfoInspector : Editor
{
    private MyItemDropInfo _data;
    private SerializedProperty _dataProperty;

    public void OnEnable()
    {
        _data = (MyItemDropInfo)target;
        _dataProperty = serializedObject.FindProperty("_dropInfos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawList(_dataProperty, "아이템 드랍 정보 ");

        _data.MinMoney = EditorGUILayout.IntField("최소 드랍 금액", _data.MinMoney);
        _data.MaxMoney = EditorGUILayout.IntField("최대 드랍 금액", _data.MaxMoney);

        if (GUILayout.Button("저장"))
        {
            _data.ApplyDirty();
            Debug.Log("아이템 드랍 정보 저장 완료");
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawList(SerializedProperty _list, string _labalName)
    {
        //리스트 갯수 표시
        EditorGUILayout.PropertyField(_list.FindPropertyRelative("Array.size"), new GUIContent("드랍 아이템 갯수"));
        int Count = _list.arraySize;
        for (int i = 0; i < Count; ++i)
        {
            EditorGUILayout.PropertyField(_list.GetArrayElementAtIndex(i), new GUIContent(_labalName + i));
        }
    }
}
