using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class InventoryUIEditor : VisualElement
{
    [SerializeField, DontCreateProperty]
    private VisualTreeAsset m_ItemTemplate;
    [UxmlAttribute,CreateProperty] 
    public VisualTreeAsset ItemTemplate
    {
        get => m_ItemTemplate;
        set => m_ItemTemplate = value;
    }
    private Inventory Inventory;
    private VisualElement grid => this.Q<VisualElement>("Grid");
    public void Init(Inventory inventory)
    {
        Inventory = inventory;
#if UNITY_EDITOR
        this.Q<VisualElement>("Grid").AddToClassList("background-disable");
#endif
        this.Q<PropertyField>("elements").RegisterValueChangeCallback((evt) => 
        {
            UpdateUi();
        });
        inventory.OnInventoryItemAdded += UpdateUi;
        inventory.OnInventoryItemRemoved += UpdateUi;
        UpdateUi();
    }
    private void UpdateUi()
    {
        grid.Clear();
        foreach(Item item in Inventory.items)
        {
            var itemUI = ItemTemplate.CloneTree();
            itemUI.dataSource = item;
            grid.Add(itemUI);
        }
    }
}