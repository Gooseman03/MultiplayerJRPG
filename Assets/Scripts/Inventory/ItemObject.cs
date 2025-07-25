using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Item",menuName = "Items")]
[Serializable]
public class ItemObject : ScriptableObject
{
    public string Description;
    public Sprite Icon;
}