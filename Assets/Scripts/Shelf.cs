using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Shelf component that holds items for sale.
/// Implements IInteractable to allow mouse-click based restocking.
/// </summary>
public class Shelf : MonoBehaviour, IInteractable {
    [Header("Shelf Configuration")]
    [SerializeField] private ShelfTypeDataSO shelfType;

    [Header("Slot Positions")]
    [SerializeField] private Transform[] slotPositions;

    [Header("Slot Settings")]
    [SerializeField] private int baseItemsPerSlot = 5;
    [SerializeField] private int capacityBonus = 0;

    [Header("Quantity Badge UI")]
    [SerializeField] private GameObject quantityBadgePrefab;
    [SerializeField] private Vector3 badgeOffset = new Vector3(0, 0.5f, 0);

    private Canvas badgeCanvas; // Auto-found at runtime

    [Header("Interaction Visual Feedback")]
    [SerializeField] private OutlineEffect outlineEffect;

    private List<ShelfSlot> slots = new List<ShelfSlot>();
    private bool isInitialized = false;
    private bool wasLowStock = false; // Track previous low stock state to prevent spam

    // Phase 1: Events for visual urgency feedback
    public event Action OnLowStock;  // Fired when capacity drops below 30%
    public event Action OnStockNormal; // Fired when capacity returns above 30%
    public event Action OnShelfEmpty; // Fired when shelf becomes completely empty

    public ShelfTypeDataSO ShelfType => shelfType;
    public bool IsEmpty => slots.All(slot => slot.IsEmpty);
    public bool IsFull => slots.All(slot => slot.IsFull);
    public int TotalItems => slots.Sum(slot => slot.ItemCount);
    public int ItemsPerSlot => baseItemsPerSlot + capacityBonus;

    /// <summary>
    /// Phase 1: Returns current capacity as percentage (0-100%)
    /// </summary>
    public float CapacityPercentage {
        get {
            int totalCapacity = slots.Count * ItemsPerSlot;
            if (totalCapacity == 0) return 0f;
            return (TotalItems / (float)totalCapacity) * 100f;
        }
    }

    private void Awake() {
        if (!isInitialized) {
            InitializeShelf();
        }
    }

    /// <summary>
    /// Initialize shelf with slots based on assigned slot position transforms
    /// </summary>
    private void InitializeShelf() {
        if (shelfType == null) {
            Debug.LogError($"Shelf {gameObject.name} has no ShelfTypeDataSO assigned!");
            return;
        }

        if (slotPositions == null || slotPositions.Length == 0) {
            Debug.LogError($"Shelf {gameObject.name} has no slot positions assigned! Please assign transforms in the inspector.");
            return;
        }

        // Auto-find WorldSpaceUICanvas in scene for quantity badges
        if (badgeCanvas == null && quantityBadgePrefab != null) {
            // Find canvas with name starting with "WorldSpaceUICanvas" (supports hyphens like WorldSpaceUICanvas----)
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in allCanvases) {
                if (canvas.gameObject.name.StartsWith("WorldSpaceUICanvas")) {
                    badgeCanvas = canvas;
                    break;
                }
            }

            if (badgeCanvas == null) {
                Debug.LogWarning($"Shelf {gameObject.name}: No canvas starting with 'WorldSpaceUICanvas' found in scene. Quantity badges will not be created.");
            }
        }

        // Create ShelfSlot components at each assigned transform position
        for (int i = 0; i < slotPositions.Length; i++) {
            Transform slotTransform = slotPositions[i];

            if (slotTransform == null) {
                Debug.LogWarning($"Shelf {gameObject.name}: Slot position {i} is null, skipping...");
                continue;
            }

            // Add ShelfSlot component if it doesn't exist
            ShelfSlot slot = slotTransform.GetComponent<ShelfSlot>();
            if (slot == null) {
                slot = slotTransform.gameObject.AddComponent<ShelfSlot>();
            }

            // Create quantity badge for this slot if prefab and canvas are assigned
            QuantityBadge badge = null;
            if (quantityBadgePrefab != null && badgeCanvas != null) {
                GameObject badgeObj = Instantiate(quantityBadgePrefab, badgeCanvas.transform);
                badge = badgeObj.GetComponent<QuantityBadge>();

                if (badge != null) {
                    badge.Initialize(slotTransform, badgeOffset);
                } else {
                    Debug.LogWarning($"Shelf {gameObject.name}: Quantity badge prefab missing QuantityBadge component!");
                    Destroy(badgeObj);
                }
            }

            slot.Initialize(i, ItemsPerSlot, badge);
            slots.Add(slot);
        }

        isInitialized = true;
        Debug.Log($"Initialized {shelfType.shelfTypeName} with {slots.Count} slots at custom positions");
    }

    /// <summary>
    /// Increases the capacity of all slots on this shelf (called by UpgradeManager).
    /// </summary>
    public void IncreaseCapacity(int amount)
    {
        capacityBonus += amount;
        Debug.Log($"{shelfType?.shelfTypeName ?? gameObject.name}: Capacity increased by {amount}. New capacity: {ItemsPerSlot} items/slot");

        // Update existing slots with new capacity
        foreach (ShelfSlot slot in slots)
        {
            slot.UpdateMaxCapacity(ItemsPerSlot);
        }
    }

    /// <summary>
    /// Restock shelf with specific item type
    /// </summary>
    public bool RestockShelf(ItemDataSO itemData, int quantity = 1) {
        if (itemData == null || itemData.itemPrefab == null) return false;
        if (shelfType == null) return false;

        // Check if shelf can hold this item category
        if (!shelfType.CanHoldCategory(itemData.itemCategory)) {
            Debug.Log($"Shelf type {shelfType.shelfTypeName} cannot hold {itemData.itemCategory} items!");
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
                GameObject itemObj = Instantiate(itemData.itemPrefab, slot.transform);
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

            // Phase 1: Check capacity after restocking
            CheckCapacityAndTriggerEvents();

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
                Item item = preferredSlot.TakeItem();
                // Phase 1: Check capacity after taking item
                CheckCapacityAndTriggerEvents();
                return item;
            }
        }

        // Otherwise take from any non-empty slot
        ShelfSlot availableSlot = slots.FirstOrDefault(slot => !slot.IsEmpty);
        Item takenItem = availableSlot?.TakeItem();

        // Phase 1: Check capacity after taking item
        if (takenItem != null) {
            CheckCapacityAndTriggerEvents();
        }

        return takenItem;
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

        return availableTypes.Count > 0 ? availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)] : null;
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

    /// <summary>
    /// Phase 1: Check shelf capacity and trigger appropriate events for visual feedback
    /// </summary>
    private void CheckCapacityAndTriggerEvents() {
        float capacity = CapacityPercentage;
        bool isCurrentlyLowStock = capacity < 30f && capacity > 0f;
        bool isCurrentlyEmpty = IsEmpty;

        // Trigger empty event first (highest priority)
        if (isCurrentlyEmpty) {
            OnShelfEmpty?.Invoke();

            // Play empty shelf sound
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySound(SoundType.ShelfEmpty);
            }

            Debug.Log($"{shelfType?.shelfTypeName ?? gameObject.name} is now EMPTY!");
            wasLowStock = false; // Reset low stock tracking when empty
            return;
        }

        // Trigger low stock events (only when state changes to prevent spam)
        if (isCurrentlyLowStock && !wasLowStock) {
            OnLowStock?.Invoke();
            Debug.Log($"{shelfType?.shelfTypeName ?? gameObject.name} is LOW on stock ({capacity:F1}%)");
            wasLowStock = true;
        } else if (!isCurrentlyLowStock && wasLowStock) {
            OnStockNormal?.Invoke();
            Debug.Log($"{shelfType?.shelfTypeName ?? gameObject.name} stock is normal ({capacity:F1}%)");
            wasLowStock = false;
        }
    }

    #region IInteractable Implementation

    /// <summary>
    /// Called when mouse cursor hovers over the shelf.
    /// Shows visual feedback via outline effect.
    /// </summary>
    public void OnHoverEnter()
    {
        if (outlineEffect != null)
        {
            outlineEffect.ShowOutline();
        }
    }

    /// <summary>
    /// Called when mouse cursor leaves the shelf.
    /// Hides visual feedback.
    /// </summary>
    public void OnHoverExit()
    {
        if (outlineEffect != null)
        {
            outlineEffect.HideOutline();
        }
    }

    /// <summary>
    /// Called when the shelf is clicked.
    /// Opens the restock UI for this shelf.
    /// </summary>
    public void OnClick()
    {
        if (RestockUIManager.Instance != null)
        {
            // If UI is already open for this shelf, close it
            if (RestockUIManager.Instance.IsUIOpen())
            {
                RestockUIManager.Instance.HideRestockUI();
            }
            else
            {
                // Open restock UI for this shelf
                RestockUIManager.Instance.ShowRestockUI(this);
            }
        }
        else
        {
            Debug.LogError("RestockUIManager instance not found!");
        }
    }

    /// <summary>
    /// Returns the GameObject this interactable is attached to.
    /// Required by IInteractable interface.
    /// </summary>
    public GameObject GetGameObject()
    {
        return gameObject;
    }

    #endregion
}