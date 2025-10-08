using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a storage slot on a shelf that can hold multiple items of the same type
/// </summary>
public class ShelfSlot : MonoBehaviour {
    [SerializeField] private Transform itemContainer;

    private ItemDataSO itemType;
    private List<Item> storedItems = new List<Item>();
    private int maxCapacityPerSlot = 5;
    private int slotIndex;

    public bool IsEmpty => storedItems.Count == 0;
    public bool IsFull => storedItems.Count >= maxCapacityPerSlot;
    public ItemDataSO ItemType => itemType;
    public int ItemCount => storedItems.Count;
    public int AvailableSpace => maxCapacityPerSlot - storedItems.Count;

    /// <summary>
    /// Initialize this slot with its index and capacity
    /// </summary>
    public void Initialize(int index, int maxCapacity) {
        slotIndex = index;
        maxCapacityPerSlot = maxCapacity;
        if (itemContainer == null) {
            itemContainer = transform;
        }
    }

    /// <summary>
    /// Check if this slot can accept the given item type
    /// </summary>
    public bool CanAcceptItem(ItemDataSO data) {
        // Empty slots can accept any item
        if (IsEmpty) return true;
        // Non-empty slots can only accept matching item types
        return itemType == data;
    }

    /// <summary>
    /// Add an item to this slot
    /// </summary>
    public bool AddItem(Item item, ItemDataSO data) {
        if (IsFull) return false;
        if (!CanAcceptItem(data)) return false;

        // Set item type if this is the first item
        if (IsEmpty) {
            itemType = data;
        }

        storedItems.Add(item);
        item.transform.SetParent(itemContainer);
        item.transform.localPosition = GetItemStackPosition(storedItems.Count - 1);
        item.transform.localScale = Vector3.one;

        return true;
    }

    /// <summary>
    /// Remove and return the top item from this slot
    /// </summary>
    public Item TakeItem() {
        if (IsEmpty) return null;

        Item item = storedItems[storedItems.Count - 1];
        storedItems.RemoveAt(storedItems.Count - 1);

        // Clear item type if slot is now empty
        if (IsEmpty) {
            itemType = null;
        }

        return item;
    }

    /// <summary>
    /// Get the local position for an item based on its stack index
    /// </summary>
    private Vector3 GetItemStackPosition(int stackIndex) {
        // Stack items slightly offset to show depth
        return new Vector3(
            stackIndex * 0.02f,  // Slight horizontal offset
            stackIndex * 0.05f,  // Slight vertical offset to show stacking
            -stackIndex * 0.01f  // Z-offset for depth
        );
    }

    /// <summary>
    /// Get all items in this slot (useful for inventory operations)
    /// </summary>
    public List<Item> GetAllItems() {
        return new List<Item>(storedItems);
    }

    /// <summary>
    /// Clear all items from this slot
    /// </summary>
    public void ClearSlot() {
        foreach (Item item in storedItems) {
            if (item != null) {
                Destroy(item.gameObject);
            }
        }
        storedItems.Clear();
        itemType = null;
    }
}
