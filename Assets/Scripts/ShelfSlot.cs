using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a storage slot on a shelf that can hold multiple items of the same type.
/// Uses a quantity badge to display count instead of visual stacking.
/// </summary>
public class ShelfSlot : MonoBehaviour {
    [SerializeField] private Transform itemContainer;

    private ItemDataSO itemType;
    private List<Item> storedItems = new List<Item>();
    private int maxCapacityPerSlot = 5;
    private int slotIndex;
    private QuantityBadge quantityBadge;
    private Item displayedItem; // The single visual item shown in the slot

    public bool IsEmpty => storedItems.Count == 0;
    public bool IsFull => storedItems.Count >= maxCapacityPerSlot;
    public ItemDataSO ItemType => itemType;
    public int ItemCount => storedItems.Count;
    public int AvailableSpace => maxCapacityPerSlot - storedItems.Count;

    /// <summary>
    /// Initialize this slot with its index and capacity
    /// </summary>
    public void Initialize(int index, int maxCapacity, QuantityBadge badge = null) {
        slotIndex = index;
        maxCapacityPerSlot = maxCapacity;
        quantityBadge = badge;

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

        // Disable colliders on items to prevent raycast interference with shelf clicking
        Collider itemCollider = item.GetComponent<Collider>();
        if (itemCollider != null) {
            itemCollider.enabled = false;
        }

        // Only show the first item visually, hide all others
        if (storedItems.Count == 1) {
            // First item - make it visible
            displayedItem = item;
            item.transform.SetParent(itemContainer);
            item.transform.localPosition = Vector3.zero; // No offset needed
            item.transform.localScale = Vector3.one;
        } else {
            // Additional items - hide them but keep in list for inventory tracking
            item.gameObject.SetActive(false);
            item.transform.SetParent(itemContainer);
        }

        UpdateQuantityDisplay();
        return true;
    }

    /// <summary>
    /// Remove and return the top item from this slot
    /// </summary>
    public Item TakeItem() {
        if (IsEmpty) return null;

        Item item = storedItems[storedItems.Count - 1];
        storedItems.RemoveAt(storedItems.Count - 1);

        // If we took the displayed item, make the next one visible
        if (item == displayedItem) {
            displayedItem = null;
            if (!IsEmpty) {
                // Show the next item
                displayedItem = storedItems[storedItems.Count - 1];
                displayedItem.gameObject.SetActive(true);
                displayedItem.transform.localPosition = Vector3.zero;
            }
        }

        // Clear item type if slot is now empty
        if (IsEmpty) {
            itemType = null;
        }

        UpdateQuantityDisplay();
        return item;
    }

    /// <summary>
    /// Updates the quantity badge display based on current item count.
    /// Shows badge only when count > 1.
    /// </summary>
    private void UpdateQuantityDisplay() {
        if (quantityBadge != null) {
            quantityBadge.UpdateQuantity(storedItems.Count);
        }
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
        displayedItem = null;
        UpdateQuantityDisplay();
    }

    /// <summary>
    /// Updates the maximum capacity of this slot (called when shelf capacity upgrades are purchased).
    /// </summary>
    public void UpdateMaxCapacity(int newMaxCapacity) {
        maxCapacityPerSlot = newMaxCapacity;
    }
}
