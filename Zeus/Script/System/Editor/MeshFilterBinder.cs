using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class MeshFilterBinder : EditorWindow
{
    private GameObject sourceObject;
    public Mesh[] Meshes;

    [MenuItem("Tools/MeshFilterBinder")]
    // Start is called before the first frame update
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        MeshFilterBinder window = (MeshFilterBinder)EditorWindow.GetWindow(typeof(MeshFilterBinder));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        sourceObject = EditorGUILayout.ObjectField("Source", sourceObject, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Binding"))
            Binding();
        EditorGUILayout.EndHorizontal();

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("Meshes");

        EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
        so.ApplyModifiedProperties(); // Remember to apply modified properties
    }

    private void Binding()
    {
        if (sourceObject == null)
            return;

        var filters = sourceObject.GetComponentsInChildren<MeshFilter>();
        foreach (var filter in filters)
        {
            foreach (var item in Meshes)
            {
                if (item.name.Equals(filter.name))
                {
                    var col = filter.GetComponent<MeshCollider>();
                    if (col != null)    
                        col.sharedMesh = item;

                    filter.mesh = item;
                    Debug.Log($"Binding Success! {item.name}");
                }
            }
        }
    }
}
