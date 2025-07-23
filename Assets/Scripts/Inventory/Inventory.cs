using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new();
    
    public event Action OnInventoryItemAdded;
    public event Action OnInventoryItemRemoved;

    public void AddLog(ItemObject BaseItem)
    {
        var item = new Item(BaseItem);
        items.Add(item);
        OnInventoryItemAdded?.Invoke();
    }

    public void RemoveLog(Item item)
    {
        items.Remove(item);
        OnInventoryItemRemoved?.Invoke();
    }
}
