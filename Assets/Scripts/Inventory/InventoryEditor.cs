using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using UnityEngine.ResourceManagement.AsyncOperations;

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor
{
    public VisualTreeAsset m_InspectorUXML;

    public override VisualElement CreateInspectorGUI()
    {
        if (m_InspectorUXML == null)
        {
            var handle = Addressables.LoadAssetAsync<VisualTreeAsset>("InventoryUIEditor");
            handle.WaitForCompletion();
            if (handle.Status == AsyncOperationStatus.Succeeded)
                m_InspectorUXML = handle.Result;
        }
        // Create a new VisualElement to be the root of our Inspector UI.
        VisualElement myInspector = new VisualElement();

        // Load the UXML file and clone its tree into the inspector.
        if (m_InspectorUXML != null)
        {
            VisualElement uxmlContent = m_InspectorUXML.CloneTree();
            myInspector.Add(uxmlContent);
            //myInspector.Q<InventoryUIEditor>("Inventory").Init((Inventory)target);
        }

        // Return the finished Inspector UI.
        return myInspector;
    }
}
