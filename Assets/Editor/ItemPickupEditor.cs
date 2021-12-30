using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemPickup))]
public class ItemPickupEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        ItemPickup itemPickup = (ItemPickup)target;
        itemPickup.interactRadius = EditorGUILayout.FloatField("Interact Radius", itemPickup.interactRadius);
    }
}
