using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Helper class for defining starting delivery items in the inspector.
/// </summary>
[System.Serializable]
public class StartingDeliveryItem
{
    public ItemDataSO item;
    public int quantity;
}

/// <summary>
/// Unified supply chain management system handling ordering and deliveries.
/// Manages the full cycle: order placement → delivery scheduling → box spawning.
/// Handles item category unlocking and progression-based item availability.
/// </summary>
public class SupplyChainManager : MonoBehaviour
{
    public static SupplyChainManager Instance;

    #region Order System
    [Header("Order System")]
    [SerializeField] private List<ItemDataSO> allItems = new List<ItemDataSO>();

    private HashSet<ItemCategory> unlockedCategories = new HashSet<ItemCategory>();
    private Dictionary<ItemDataSO, int> currentOrder = new Dictionary<ItemDataSO, int>();
    private int currentUnlockedTier = 1; // Start with Tier 1 unlocked

    /// <summary>
    /// Event triggered when the current order is modified (item added/removed).
    /// </summary>
    public event System.Action OnOrderChanged;

    /// <summary>
    /// Event triggered when a new item category is unlocked.
    /// </summary>
    public event System.Action<ItemCategory> OnCategoryUnlocked;

    /// <summary>
    /// Event triggered when a new tier is unlocked.
    /// </summary>
    public event System.Action<int> OnTierUnlocked;

    /// <summary>
    /// Gets the current unlocked tier (1, 2, or 3).
    /// </summary>
    public int CurrentTier => currentUnlockedTier;

    /// <summary>
    /// Returns list of items available based on unlocked categories and tier progression.
    /// </summary>
    public List<ItemDataSO> AvailableItems
    {
        get
        {
            return allItems.Where(item =>
            {
                // Check if unlocked by default OR category is unlocked
                bool categoryUnlocked = item.isUnlockedByDefault || unlockedCategories.Contains(item.itemCategory);

                // Check if player has unlocked this item's quality tier
                bool tierUnlocked = item.tier <= currentUnlockedTier;

                return categoryUnlocked && tierUnlocked;
            }).ToList();
        }
    }
    #endregion

    #region Delivery System
    [Header("Delivery System")]
    [SerializeField] private GameObject deliveryBoxPrefab;
    [SerializeField] private Transform deliverySpawnPoint;

    [Header("Box Grid Layout")]
    [Tooltip("Number of boxes per row before wrapping to next row")]
    [SerializeField] private int boxesPerRow = 4;
    [Tooltip("Horizontal spacing between boxes")]
    [SerializeField] private float boxSpacingX = 0.8f;
    [Tooltip("Vertical spacing between rows (Z-axis in world space)")]
    [SerializeField] private float boxSpacingZ = 0.8f;
    [Tooltip("Number of boxes per layer before stacking vertically")]
    [SerializeField] private int boxesPerLayer = 10;
    [Tooltip("Height offset for stacked boxes (Y-axis)")]
    [SerializeField] private float stackHeightY = 1.0f;

    [Header("Starting Delivery (Day 1)")]
    [SerializeField] private List<StartingDeliveryItem> startingDelivery = new List<StartingDeliveryItem>();

    private Dictionary<ItemDataSO, int> pendingDelivery = new Dictionary<ItemDataSO, int>();
    private List<DeliveryBox> activeBoxes = new List<DeliveryBox>();
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Initialize starting categories (Weapons, Shields, Potions)
        InitializeStartingCategories();
    }

    private void Start()
    {
        // Subscribe to day manager phase changes
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnPhaseChanged += OnPhaseChanged;

            // If already in morning phase when we start, spawn boxes immediately
            if (DayManager.Instance.CurrentPhase == GamePhase.Morning)
            {
                SpawnDeliveryBoxes();
            }
        }
    }

    /// <summary>
    /// Initialize the categories that are unlocked from the start of the game.
    /// </summary>
    private void InitializeStartingCategories()
    {
        unlockedCategories.Add(ItemCategory.Weapons);
        unlockedCategories.Add(ItemCategory.Shields);
        unlockedCategories.Add(ItemCategory.Potions);

        Debug.Log("Starting categories unlocked: Weapons, Shields, Potions");
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    #region Category Unlock System
    /// <summary>
    /// Unlocks a new item category, making all items in that category available for ordering.
    /// </summary>
    public void UnlockCategory(ItemCategory category)
    {
        if (unlockedCategories.Contains(category))
        {
            Debug.LogWarning($"Category {category} is already unlocked!");
            return;
        }

        unlockedCategories.Add(category);
        Debug.Log($"Unlocked item category: {category}");

        OnCategoryUnlocked?.Invoke(category);
        OnOrderChanged?.Invoke(); // Refresh order menu to show new items
    }

    /// <summary>
    /// Checks if a specific category is unlocked.
    /// </summary>
    public bool IsCategoryUnlocked(ItemCategory category)
    {
        return unlockedCategories.Contains(category);
    }

    /// <summary>
    /// Gets all currently unlocked categories.
    /// </summary>
    public HashSet<ItemCategory> GetUnlockedCategories()
    {
        return new HashSet<ItemCategory>(unlockedCategories);
    }
    #endregion

    #region Tier Unlock System
    /// <summary>
    /// Unlocks a new quality tier, making all items of that tier or lower available for ordering.
    /// </summary>
    public void UnlockTier(int tier)
    {
        if (tier < 1 || tier > 3)
        {
            Debug.LogError($"Invalid tier {tier}. Tier must be between 1 and 3.");
            return;
        }

        if (currentUnlockedTier >= tier)
        {
            Debug.LogWarning($"Tier {tier} is already unlocked (current tier: {currentUnlockedTier})!");
            return;
        }

        currentUnlockedTier = tier;
        Debug.Log($"Unlocked item tier: {tier}");

        OnTierUnlocked?.Invoke(tier);
        OnOrderChanged?.Invoke(); // Refresh order menu to show new items
    }

    /// <summary>
    /// Checks if a specific tier is unlocked.
    /// </summary>
    public bool IsTierUnlocked(int tier)
    {
        return tier <= currentUnlockedTier;
    }
    #endregion

    #region Phase Management
    /// <summary>
    /// Called when game phase changes - spawns delivery boxes on morning phase.
    /// </summary>
    private void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Morning)
        {
            SpawnDeliveryBoxes();
        }
    }
    #endregion

    #region Order System Methods
    /// <summary>
    /// Adds an item to the current order.
    /// </summary>
    public void AddToOrder(ItemDataSO item, int quantity)
    {
        if (item == null || quantity <= 0) return;

        if (currentOrder.ContainsKey(item))
        {
            currentOrder[item] += quantity;
        }
        else
        {
            currentOrder[item] = quantity;
        }

        Debug.Log($"Added {quantity}x {item.itemName} to order");
        OnOrderChanged?.Invoke();
    }

    /// <summary>
    /// Removes an item from the current order.
    /// </summary>
    public void RemoveFromOrder(ItemDataSO item, int quantity)
    {
        if (item == null || !currentOrder.ContainsKey(item)) return;

        currentOrder[item] -= quantity;

        if (currentOrder[item] <= 0)
        {
            currentOrder.Remove(item);
        }

        OnOrderChanged?.Invoke();
    }

    /// <summary>
    /// Clears the current order.
    /// </summary>
    public void ClearOrder()
    {
        currentOrder.Clear();
        OnOrderChanged?.Invoke();
    }

    /// <summary>
    /// Gets the quantity of a specific item in the current order.
    /// </summary>
    public int GetOrderQuantity(ItemDataSO item)
    {
        return currentOrder.ContainsKey(item) ? currentOrder[item] : 0;
    }

    /// <summary>
    /// Calculates the total cost of the current order.
    /// </summary>
    public int GetTotalOrderCost()
    {
        int total = 0;
        foreach (var kvp in currentOrder)
        {
            total += kvp.Key.restockCost * kvp.Value;
        }

        return total;
    }

    /// <summary>
    /// Confirms and places the order, deducting money and scheduling delivery.
    /// Returns true if successful.
    /// </summary>
    public bool ConfirmOrder()
    {
        int totalCost = GetTotalOrderCost();

        if (totalCost == 0)
        {
            Debug.Log("Order is empty!");
            return false;
        }

        if (!GameManager.Instance.SpendMoney(totalCost))
        {
            Debug.Log("Not enough money to complete order!");
            return false;
        }

        // Schedule order for next morning delivery
        AddPendingDelivery(GetCurrentOrder());

        Debug.Log($"Order confirmed! Total cost: ${totalCost}. Items will be delivered tomorrow morning.");
        ClearOrder();
        return true;
    }

    /// <summary>
    /// Gets a copy of the current order dictionary.
    /// </summary>
    public Dictionary<ItemDataSO, int> GetCurrentOrder()
    {
        return new Dictionary<ItemDataSO, int>(currentOrder);
    }
    #endregion

    #region Delivery System Methods
    /// <summary>
    /// Stores an order for next morning delivery.
    /// </summary>
    private void AddPendingDelivery(Dictionary<ItemDataSO, int> orderItems)
    {
        // Accumulate orders instead of replacing them
        foreach (var kvp in orderItems)
        {
            if (pendingDelivery.ContainsKey(kvp.Key))
            {
                pendingDelivery[kvp.Key] += kvp.Value; // Add to existing quantity
            }
            else
            {
                pendingDelivery[kvp.Key] = kvp.Value; // New item
            }
        }

        // Calculate total items for debug log
        int totalItems = pendingDelivery.Sum(kvp => kvp.Value);
        Debug.Log($"Order added to delivery queue. {pendingDelivery.Count} item types ({totalItems} total items) scheduled for delivery.");
    }

    /// <summary>
    /// Spawns delivery boxes at the start of morning phase.
    /// Handles Day 1 starting delivery and regular deliveries.
    /// </summary>
    private void SpawnDeliveryBoxes()
    {
        // Clear any existing boxes
        foreach (var box in activeBoxes)
        {
            if (box != null)
            {
                Destroy(box.gameObject);
            }
        }
        activeBoxes.Clear();

        // Check if Day 1 and no pending delivery - use starting delivery
        if (DayManager.Instance != null && DayManager.Instance.CurrentDay == 1 && pendingDelivery.Count == 0)
        {
            foreach (var startingItem in startingDelivery)
            {
                if (startingItem.item != null && startingItem.quantity > 0)
                {
                    pendingDelivery[startingItem.item] = startingItem.quantity;
                }
            }
            Debug.Log($"Day 1: Loaded {pendingDelivery.Count} starting delivery items.");
        }

        // If no pending delivery, nothing to spawn
        if (pendingDelivery.Count == 0)
        {
            Debug.Log("No deliveries to spawn this morning.");
            return;
        }

        // Spawn boxes in a 3D grid layout with vertical stacking
        Vector3 spawnPos = deliverySpawnPoint != null ? deliverySpawnPoint.position : Vector3.zero;
        int boxIndex = 0;

        foreach (var kvp in pendingDelivery)
        {
            if (deliveryBoxPrefab == null) continue;

            // Calculate which layer (vertical stack level) this box is on
            int layer = boxIndex / boxesPerLayer;

            // Calculate position within this layer
            int positionInLayer = boxIndex % boxesPerLayer;
            int column = positionInLayer % boxesPerRow;
            int row = positionInLayer / boxesPerRow;

            // Calculate 3D offset for grid layout
            // X-axis: columns (left to right)
            // Y-axis: layers (ground to top)
            // Z-axis: rows (back to front, negative to spawn towards camera)
            Vector3 offset = new Vector3(
                column * boxSpacingX,
                layer * stackHeightY,
                -row * boxSpacingZ
            );
            GameObject boxObj = Instantiate(deliveryBoxPrefab, spawnPos + offset, Quaternion.identity);

            DeliveryBox box = boxObj.GetComponent<DeliveryBox>();
            if (box != null)
            {
                box.Initialize(kvp.Key, kvp.Value);
                activeBoxes.Add(box);
                boxIndex++;
            }
        }

        Debug.Log($"Spawned {activeBoxes.Count} delivery boxes");

        // Clear pending delivery after spawning to prevent re-spawning on next day
        pendingDelivery.Clear();
    }

    /// <summary>
    /// Called when a delivery box is opened by the player.
    /// </summary>
    public void OnBoxOpened(DeliveryBox box)
    {
        activeBoxes.Remove(box);

        // Check if all boxes have been opened
        if (activeBoxes.Count == 0)
        {
            Debug.Log("All delivery boxes opened!");
        }
    }

    /// <summary>
    /// Checks if there are any unopened delivery boxes.
    /// </summary>
    public bool HasPendingDeliveries()
    {
        return activeBoxes.Count > 0;
    }
    #endregion
}
