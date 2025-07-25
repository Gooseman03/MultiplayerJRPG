using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ItemTooltip : VisualElement
{
    public delegate VisualElement[] DrawElementCallback();
    public DrawElementCallback callback = () =>
    {
        VisualElement[] elements =
        {
            new Label("This is a default title"),
            new Label("How did this go wrong")
        };

        elements[0].AddToClassList("tooltip__overlay--title");
        elements[1].AddToClassList("tooltip__overlay--description");
        return elements;
    };
    public ItemTooltip() 
    {
        RegisterCallbackOnce<AttachToPanelEvent>(OnPanelAttach);
    }

    public ItemTooltip(DrawElementCallback callback)
    {
        this.callback = callback;
        RegisterCallbackOnce<AttachToPanelEvent>(OnPanelAttach);
    }

    public void OnPanelAttach(AttachToPanelEvent e)
    {
        var element = new VisualElement()
        {
            name = "Overlay"
        };
        element.AddToClassList("tooltip__overlay");
        Add(element);
        style.position = Position.Absolute;
        callback?.Invoke().ToList().ForEach((elementToAdd) =>
        {
            element.Add(elementToAdd);
        });
        this.focusable = false;
    }
}
