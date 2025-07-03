using Ladder.Multiplayer.Multiplayer.Syncing;
using UnityEditor;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;

[CustomEditor(typeof(Multiplayer))]
public class MultiplayerEditor : Editor
{
    public bool ButtonPressed = false;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Force Start Timer"))
        {
            ((Multiplayer)target).StartTimer();
        }
        if (GUILayout.Button("Force Trigger"))
        {
            ((Multiplayer)target).OnTriggerTime();
        }
    }
}
