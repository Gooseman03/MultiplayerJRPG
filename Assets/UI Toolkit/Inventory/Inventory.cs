using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class Inventory : MonoBehaviour
{
    [SerializeField] public List<Item> items = new();
    
    public event Action<Item> OnInventoryItemAdded;
    public event Action<Item> OnInventoryItemRemoved;

    public void AddLog(TestItemType type)
    {
        var item = new Item
        {
            Type = type
        };
        items.Add(item);
        OnInventoryItemAdded?.Invoke(item);
    }

    public void RemoveLog(Item item)
    {
        OnInventoryItemRemoved?.Invoke(item);
        items.Remove(item);
    }

}
