using UnityEngine;

/// <summary>
/// Represents a storage slot on a shelf that can hold a single item.
/// Simplified from multi-item stacking to 1 item per slot design.
/// </summary>
public class ShelfSlot : MonoBehaviour {
    [SerializeField] private Transform itemContainer;

    private ItemDataSO itemType;
    private Item currentItem;
    private int slotIndex;

    public bool IsEmpty => currentItem == null;
    public bool IsFull => currentItem != null;
    public ItemDataSO ItemType => itemType;
    public int ItemCount => currentItem != null ? 1 : 0;
    public int AvailableSpace => currentItem == null ? 1 : 0;

    /// <summary>
    /// Initialize this slot with its index
    /// </summary>
    public void Initialize(int index) {
        slotIndex = index;

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
    /// Add an item to this slot (only 1 item allowed)
    /// </summary>
    public bool AddItem(Item item, ItemDataSO data) {
        if (IsFull) return false;
        if (!CanAcceptItem(data)) return false;

        // Set item type and store reference
        itemType = data;
        currentItem = item;

        // Disable colliders on items to prevent raycast interference with shelf clicking
        Collider itemCollider = item.GetComponent<Collider>();
        if (itemCollider != null) {
            itemCollider.enabled = false;
        }

        // Position the item
        item.transform.SetParent(itemContainer);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;
        item.gameObject.SetActive(true);

        return true;
    }

    /// <summary>
    /// Remove and return the item from this slot
    /// </summary>
    public Item TakeItem() {
        if (IsEmpty) return null;

        Item item = currentItem;
        currentItem = null;
        itemType = null;

        return item;
    }

    /// <summary>
    /// Clear the item from this slot
    /// </summary>
    public void ClearSlot() {
        if (currentItem != null) {
            Destroy(currentItem.gameObject);
            currentItem = null;
        }
        itemType = null;
    }
}
