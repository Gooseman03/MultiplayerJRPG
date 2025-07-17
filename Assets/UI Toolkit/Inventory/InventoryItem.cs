using Unity.Properties;
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
    public VisualElement UIElement { get; private set; }

    public VisualElement Visual;

    public InventoryItem()
    {
        RegisterCallback<AttachToPanelEvent>(e =>
        {
            if (UIElement == null) return;
            RebuildUI();
        });
    }
    public VisualElement CreateUIElement(VisualTreeAsset template)
    {
        UIElement = template.CloneTree();
        return UIElement;
    }

    public void RebuildUI(string typeClass = "")
    {
        base.Clear();
        var background = UIElement;
        var image = background.Q<VisualElement>("Image");
        if (typeClass != "")
        {
            if (!image.ClassListContains(typeClass))
            {
                image.ClearClassList();
                image.AddToClassList("size");
            }
            image.AddToClassList(typeClass);
        }
        Visual = background;
        base.Add(background);
    }
    public void OnChangeType(TestItemType type)
    {
        if (UIElement == null) return;
        RebuildUI(GetClassName());
        string GetClassName()
        {
            return type switch
            {
                TestItemType.aidan => "Aidan",
                TestItemType.dude => "Dude",
                _ => null,
            };
        }
    }
}
