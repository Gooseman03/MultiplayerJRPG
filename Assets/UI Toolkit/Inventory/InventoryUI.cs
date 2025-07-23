using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Inventory inventory;
    private void Awake()
    {
        uiDocument.rootVisualElement.Q<InventoryUIEditor>().Init(inventory);
    }
}
