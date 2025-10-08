using UnityEngine;

[CreateAssetMenu(fileName = "New Shelf Type", menuName = "DungeonMart/Shelf Type")]
public class ShelfTypeDataSO : ScriptableObject {
    [Header("Shelf Identity")]
    public string shelfTypeName;

    [Header("Capacity")]
    public int totalSlots = 6;
    public ItemSize allowedItemSize = ItemSize.Medium;

    [Header("Visual Layout")]
    public Vector2 slotSpacing = new Vector2(0.5f, 0.5f);
    public int slotsPerRow = 3;
    public bool horizontalLayout = true;

    [Header("Display Settings")]
    [Tooltip("Visual offset for items placed on this shelf")]
    public Vector3 itemDisplayOffset = Vector3.zero;
    public float itemScale = 1f;

    /// <summary>
    /// Check if this shelf type can hold items of the given size
    /// </summary>
    public bool CanHoldItemSize(ItemSize size) {
        return allowedItemSize == size;
    }

    /// <summary>
    /// Calculate position for an item in a specific slot
    /// </summary>
    public Vector3 GetSlotPosition(int slotIndex) {
        if (horizontalLayout) {
            int row = slotIndex / slotsPerRow;
            int col = slotIndex % slotsPerRow;
            return new Vector3(
                col * slotSpacing.x + itemDisplayOffset.x,
                -row * slotSpacing.y + itemDisplayOffset.y,
                itemDisplayOffset.z
            );
        } else {
            // Vertical/grid layout
            int col = slotIndex / slotsPerRow;
            int row = slotIndex % slotsPerRow;
            return new Vector3(
                col * slotSpacing.x + itemDisplayOffset.x,
                row * slotSpacing.y + itemDisplayOffset.y,
                itemDisplayOffset.z
            );
        }
    }
}
