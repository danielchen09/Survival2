using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChunkController))]
public class ChunkControllerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }
}
