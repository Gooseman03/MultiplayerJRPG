using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryUI))]
public class InventoryUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("ReloadUI"))
        {
            ((InventoryUI)target).ForceReload();
        }
    }
}