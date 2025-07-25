using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

[CustomPropertyDrawer(typeof(Item))]
public class ItemDrawerUIE : PropertyDrawer
{
    public VisualTreeAsset m_InspectorUXML;
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create a new VisualElement to be the root of our Inspector UI.
        VisualElement myInspector = new();

        // Load the UXML file and clone its tree into the inspector.
        if (m_InspectorUXML != null)
        {
            VisualElement uxmlContent = m_InspectorUXML.CloneTree();
            myInspector.Add(uxmlContent);
            myInspector.dataSource = property.boxedValue;
            myInspector.TrackPropertyValue(property, (evt) =>
            {
                myInspector.dataSource = property.boxedValue;
                //myInspector.MarkDirtyRepaint();
            });
        }

        // Return the finished Inspector UI.
        return myInspector;
    }
}
