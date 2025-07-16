using Ladder.Multiplayer.Multiplayer.Syncing;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Console))]
public class ConsoleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Force LogError"))
        {
            Debug.LogError("Test Hello im a test.");
        }
        if (GUILayout.Button("Force Assert"))
        {
            Debug.Assert(false, "Test Hello im a test.");
        }
        if (GUILayout.Button("Force LogWarning"))
        {
            Debug.LogWarning("Test Hello im a test.");
        }
        if (GUILayout.Button("Force Log"))
        {
            Debug.Log("Test Hello im a test.");
        }
        if (GUILayout.Button("Force LogException"))
        {
            Debug.LogException(new System.Exception("Test Hello im a test."));
        }
    }
}
