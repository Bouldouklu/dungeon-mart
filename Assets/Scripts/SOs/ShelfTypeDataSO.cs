using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject defining a shelf type's properties and allowed item categories.
/// Shelves can accept multiple categories for flexible item placement.
/// </summary>
[CreateAssetMenu(fileName = "New Shelf Type", menuName = "DungeonMart/Shelf Type")]
public class ShelfTypeDataSO : ScriptableObject {
    [Header("Shelf Identity")]
    public string shelfTypeName;

    [Header("Allowed Categories")]
    [Tooltip("List of item categories this shelf can hold. Can be one or multiple categories.")]
    public List<ItemCategory> allowedCategories = new List<ItemCategory>();

    /// <summary>
    /// Check if this shelf type can hold items of the given category
    /// </summary>
    public bool CanHoldCategory(ItemCategory category) {
        return allowedCategories.Contains(category);
    }
}
