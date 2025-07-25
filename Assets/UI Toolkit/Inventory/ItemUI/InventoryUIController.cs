using UnityEngine;
using UnityEngine.UIElements;
[ExecuteInEditMode]
public class InventoryUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Inventory inventory;
    private VisualElement grid;
    private void Awake()
    {
        grid = uiDocument.rootVisualElement.Q("background");

        inventory.OnInventoryItemAdded += ReloadUI;
        inventory.OnInventoryItemRemoved += ReloadUI;
        ReloadUI();
    }
    private void ReloadUI()
    {
        grid.Clear();
        foreach (Item item in inventory.items)
        {
            var itemUI = new ItemUI();
            itemUI.dataSource = item;
            grid.Add(itemUI);
        }
    }
}
