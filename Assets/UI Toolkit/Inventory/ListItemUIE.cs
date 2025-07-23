using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(List<Item>))]
public class ListItemUIE : PropertyDrawer
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
        }

        // Return the finished Inspector UI.
        return myInspector;
    }
}