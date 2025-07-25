using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[UxmlElement]
public partial class ItemUI : VisualElement
{
    public AsyncOperationHandle<StyleSheet> handle;
    private ItemTooltip m_CustomTooltip;

    public ItemTooltip CustomTooltip
    {
        get
        {
            if (m_CustomTooltip == null)
                m_CustomTooltip = new ItemTooltip(DrawTooltip);
            return m_CustomTooltip;
        }
        set => m_CustomTooltip = value;
    }
    public ItemUI() 
    {
        RegisterCallbackOnce<AttachToPanelEvent>(evt =>
        {
            name = "Image";
            style.width = resolvedStyle.height;
            AddToClassList("item");
            Add(CustomTooltip);
            CustomTooltip.style.display = DisplayStyle.None;
            handle = Addressables.LoadAssetAsync<StyleSheet>("ItemStyle");
            handle.Completed += OnStyleLoaded;
        });

        RegisterCallback<GeometryChangedEvent>(evt =>
        {
            style.width = resolvedStyle.height;
        });
        
        RegisterCallback<MouseEnterEvent>(evt =>
        {
            CustomTooltip.style.display = DisplayStyle.Flex;
        });
        
        RegisterCallback<MouseMoveEvent>(evt =>
        {
            CustomTooltip.style.top = evt.originalMousePosition.y;
            CustomTooltip.style.left = evt.originalMousePosition.x;
        });
        
        RegisterCallback<MouseLeaveEvent>(evt =>
        {
            CustomTooltip.style.display = DisplayStyle.None;
        });
    }

    private VisualElement[] DrawTooltip()
    {
        Label[] elements =
        {
            new()
            {
                text = "Default Item Name",
                //bindingPath = "Name"
            },
            new()
            {
                text = "Default Item Description",
                //bindingPath= "Description"
            }
        };
        return elements;
    }

    private void OnStyleLoaded(AsyncOperationHandle<StyleSheet> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var itemStyle = handle.Result;
            styleSheets.Insert(0,itemStyle);
        }
    }
}