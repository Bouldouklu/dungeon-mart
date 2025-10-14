using UnityEngine;

[CreateAssetMenu(fileName = "New Shelf Type", menuName = "DungeonMart/Shelf Type")]
public class ShelfTypeDataSO : ScriptableObject {
    [Header("Shelf Identity")]
    public string shelfTypeName;

    [Header("Capacity")]
    public ItemSize allowedItemSize = ItemSize.Medium;

    [Header("Display Settings")]
    public float itemScale = 1f;

    /// <summary>
    /// Check if this shelf type can hold items of the given size
    /// </summary>
    public bool CanHoldItemSize(ItemSize size) {
        return allowedItemSize == size;
    }
}
