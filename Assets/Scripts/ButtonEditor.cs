using Ladder.Multiplayer.Multiplayer.Syncing;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlaceableButton))]
public class ButtonEditor : Editor
{
    public bool ButtonPressed = false;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Lock Button Pressed"))
        {
            ((PlaceableButton)target).DebugLockAsPressed();
        }
        if (GUILayout.Button("Unlock Button Pressed"))
        {
            ((PlaceableButton)target).DebugUnlockAsPressed();
        }
    }
}
