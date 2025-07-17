using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Inventory inventory;
    public VisualTreeAsset ItemTemplate;
    private VisualElement inventoryVisual;
    private List<InventoryItem> items = new();

    private void Awake()
    {
        inventory.OnInventoryItemAdded += CreateUIElement;
        inventory.OnInventoryItemRemoved += DestoryUIElement;
    }
    private void OnDestroy()
    {
        inventory.OnInventoryItemAdded -= CreateUIElement;
        inventory.OnInventoryItemRemoved -= DestoryUIElement;
    }

    private void Start()
    {
        inventoryVisual = uiDocument.rootVisualElement.Q<VisualElement>("Grid");
    }
    public void CreateUIElement(Item item)
    {
        item.CreateVisual(ItemTemplate);
        inventoryVisual.Add(item.itemUI);
        items.Add(item.itemUI);
    }
    public void DestoryUIElement(Item item)
    {
        inventoryVisual.Remove(item.itemUI);
    }
    public void ForceReload()
    {
        foreach (var item in inventory.items)
        {
            item.ForceReload();
        }
    }
}
