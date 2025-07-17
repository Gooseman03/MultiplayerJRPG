using System;
using Unity.Properties;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Item
{
    public InventoryItem itemUI;
    [SerializeField, DontCreateProperty]
    private TestItemType m_Type;
    [CreateProperty]
    public TestItemType Type
    {
        get => m_Type;
        set
        {
            m_Type = value;
            if (itemUI == null) return;
            itemUI.OnChangeType(value);
        }
    }
    public void CreateVisual(VisualTreeAsset template)
    {
        itemUI = new InventoryItem();
        itemUI.CreateUIElement(template);
        itemUI.OnChangeType(Type);
    }
    public void ForceReload()
    {
        itemUI.OnChangeType(Type);
    }
}
