using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public enum TestItemType
{
    aidan,
    dude
}

[UxmlElement]
public partial class InventoryItem : VisualElement
{
    [UxmlAttribute]
    public string Name { get; set; } = "default_value";
    [UxmlAttribute] 
    public TestItemType ItemType { get; set; } = TestItemType.aidan;
    private VisualTreeAsset _asset;
    [UxmlAttribute]
    public VisualTreeAsset Asset 
    { 
        get => _asset; 
        set
        {
            _asset = value;
            UpdateBackground();
        } 
    }

    public VisualElement Visual;

    public InventoryItem()
    {
        RegisterCallback<AttachToPanelEvent>(e =>
        {
            UpdateBackground();
        });
    }

    private void UpdateBackground()
    {

        if (Asset == null) return;
        base.Clear();
        var background = Asset.Instantiate();
        background.Q<VisualElement>("Image").AddToClassList(GetClassName());
        Visual = background;
        base.Add(background);
        string GetClassName()
        {
            return ItemType switch
            {
                TestItemType.aidan => "Aidan",
                TestItemType.dude => "Dude",
                _ => null,
            };
        }
    }
}
