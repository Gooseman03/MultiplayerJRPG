using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
public class InventoryItemManager
{
    public InventoryItem InventoryItemVisual {  get; private set; }

    public VisualElement MakeVisual(TestItemType itemType)
    {
        InventoryItemVisual = new InventoryItem();
        
        return InventoryItemVisual;
    }
}