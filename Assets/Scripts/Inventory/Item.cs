using System;
using Unity.Properties;
using UnityEngine;

[Serializable]
public class Item
{
    public string Name;
    public string Description;
    public Sprite Icon;
    public Item(ItemObject baseObject)
    {
        Name = baseObject.name;
        Icon = baseObject.Icon;
    }
}
