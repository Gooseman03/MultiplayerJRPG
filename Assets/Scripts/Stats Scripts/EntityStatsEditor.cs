using UnityEngine;
using UnityEditor;
using Ladder.EntityStatistics;

[CustomEditor(typeof(EntityStats))]
public class EntityStatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Force 5 Physical Damage"))
        {
            ((IDamageable)target).DoDamage(DamageType.physical, 5);
        }
        if (GUILayout.Button("Force 5 Magical Damage"))
        {
            ((IDamageable)target).DoDamage(DamageType.magical, 5);
        }
        if (GUILayout.Button("Force 1000000 Damage"))
        {
            ((IDamageable)target).DoDamage(DamageType.trueDamage, 1000000);
        }
        if (GUILayout.Button("Force Kill"))
        {
            ((IKillable)target).Kill();
        }
    }
}
