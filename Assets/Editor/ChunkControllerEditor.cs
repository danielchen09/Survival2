using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChunkController))]
public class ChunkControllerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        FieldInfo[] fields = typeof(MaterialController).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        foreach (FieldInfo field in fields) {
            field.SetValue(null, EditorGUILayout.ColorField(field.Name, (Color)field.GetValue(null)));
        }
    }
}
