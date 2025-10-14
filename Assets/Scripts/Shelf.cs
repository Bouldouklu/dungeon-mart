using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shelf : MonoBehaviour {
    [Header("Shelf Configuration")]
    [SerializeField] private ShelfTypeDataSO shelfType;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform slotsContainer;

    [Header("Slot Settings")]
    [SerializeField] private int itemsPerSlot = 5;

    private List<ShelfSlot> slots = new List<ShelfSlot>();
    private bool isInitialized = false;

    public ShelfTypeDataSO ShelfType => shelfType;
    public bool IsEmpty => slots.All(slot => slot.IsEmpty);
    public bool IsFull => slots.All(slot => slot.IsFull);
    public int TotalItems => slots.Sum(slot => slot.ItemCount);

    private void Awake() {
        if (!isInitialized) {
            InitializeShelf();
        }
    }

    /// <summary>
    /// Initialize shelf with slots based on shelf type configuration
    /// </summary>
    private void InitializeShelf() {
        if (shelfType == null) {
            Debug.LogError($"Shelf {gameObject.name} has no ShelfTypeDataSO assigned!");
            return;
        }

        if (slotsContainer == null) {
            slotsContainer = transform;
        }

        // Create slots
        for (int i = 0; i < shelfType.totalSlots; i++) {
            GameObject slotObj = new GameObject($"Slot_{i}");
            slotObj.transform.SetParent(slotsContainer);
            slotObj.transform.localPosition = shelfType.GetSlotPosition(i);

            ShelfSlot slot = slotObj.AddComponent<ShelfSlot>();
            slot.Initialize(i, itemsPerSlot);
            slots.Add(slot);
        }

        isInitialized = true;
        Debug.Log($"Initialized {shelfType.shelfTypeName} with {slots.Count} slots");
    }

    /// <summary>
    /// Restock shelf with specific item type
    /// </summary>
    public bool RestockShelf(ItemDataSO itemData, int quantity = 1) {
        if (itemData == null || itemPrefab == null) return false;
        if (shelfType == null) return false;

        // Check if shelf can hold this item size
        if (!shelfType.CanHoldItemSize(itemData.itemSize)) {
            Debug.Log($"Shelf type {shelfType.shelfTypeName} cannot hold {itemData.itemSize} items!");
            return false;
        }

        // Check inventory
        if (!InventoryManager.Instance.HasItem(itemData, quantity)) {
            Debug.Log($"Not enough {itemData.itemName} in inventory!");
            return false;
        }

        // Find slots that can accept this item
        List<ShelfSlot> availableSlots = slots
            .Where(slot => slot.CanAcceptItem(itemData) && !slot.IsFull)
            .ToList();

        if (availableSlots.Count == 0) {
            Debug.Log($"No available slots for {itemData.itemName}!");
            return false;
        }

        int itemsPlaced = 0;
        int itemsToPlace = quantity;

        // Fill slots with items
        foreach (ShelfSlot slot in availableSlots) {
            if (itemsToPlace <= 0) break;

            int spaceInSlot = slot.AvailableSpace;
            int itemsForThisSlot = Mathf.Min(itemsToPlace, spaceInSlot);

            for (int i = 0; i < itemsForThisSlot; i++) {
                GameObject itemObj = Instantiate(itemPrefab, slot.transform);
                Item item = itemObj.GetComponent<Item>();

                if (item != null) {
                    item.Initialize(itemData);
                    if (slot.AddItem(item, itemData)) {
                        itemsPlaced++;
                        itemsToPlace--;
                    } else {
                        Destroy(itemObj);
                    }
                }
            }
        }

        // Deduct from inventory
        if (itemsPlaced > 0) {
            InventoryManager.Instance.RemoveFromInventory(itemData, itemsPlaced);
            Debug.Log($"Restocked {itemsPlaced}x {itemData.itemName} on {shelfType.shelfTypeName}");

            // Play shelf restock sound
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySound(SoundType.ShelfRestock);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Take an item from the shelf (prefers taking from slots with matching item type)
    /// </summary>
    public Item TakeItem(ItemDataSO preferredItemType = null) {
        // If preferred type specified, try to find it first
        if (preferredItemType != null) {
            ShelfSlot preferredSlot = slots
                .FirstOrDefault(slot => !slot.IsEmpty && slot.ItemType == preferredItemType);

            if (preferredSlot != null) {
                return preferredSlot.TakeItem();
            }
        }

        // Otherwise take from any non-empty slot
        ShelfSlot availableSlot = slots.FirstOrDefault(slot => !slot.IsEmpty);
        return availableSlot?.TakeItem();
    }

    /// <summary>
    /// Get a random item type currently on this shelf
    /// </summary>
    public ItemDataSO GetRandomAvailableItemType() {
        List<ItemDataSO> availableTypes = slots
            .Where(slot => !slot.IsEmpty)
            .Select(slot => slot.ItemType)
            .Distinct()
            .ToList();

        return availableTypes.Count > 0 ? availableTypes[Random.Range(0, availableTypes.Count)] : null;
    }

    /// <summary>
    /// Get all unique item types on this shelf
    /// </summary>
    public List<ItemDataSO> GetAvailableItemTypes() {
        return slots
            .Where(slot => !slot.IsEmpty)
            .Select(slot => slot.ItemType)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Check if shelf has specific item type available
    /// </summary>
    public bool HasItemType(ItemDataSO itemData) {
        return slots.Any(slot => !slot.IsEmpty && slot.ItemType == itemData);
    }

    /// <summary>
    /// Get total count of specific item type on shelf
    /// </summary>
    public int GetItemCount(ItemDataSO itemData) {
        return slots
            .Where(slot => slot.ItemType == itemData)
            .Sum(slot => slot.ItemCount);
    }
}