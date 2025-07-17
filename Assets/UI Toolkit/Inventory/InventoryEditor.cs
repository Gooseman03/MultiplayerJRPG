using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Force Add Aidan"))
        {
            ((Inventory)target).AddLog(TestItemType.aidan);
        }
        if (GUILayout.Button("Force Add Dude"))
        {
            ((Inventory)target).AddLog(TestItemType.dude);
        }
    }
}
