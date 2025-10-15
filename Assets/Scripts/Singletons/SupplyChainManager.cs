using System.Collections.Generic;
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
/// </summary>
public class SupplyChainManager : MonoBehaviour
{
    public static SupplyChainManager Instance;

    #region Order System
    [Header("Order System")]
    [SerializeField] private List<ItemDataSO> availableItems = new List<ItemDataSO>();

    private Dictionary<ItemDataSO, int> currentOrder = new Dictionary<ItemDataSO, int>();

    /// <summary>
    /// Event triggered when the current order is modified (item added/removed).
    /// </summary>
    public event System.Action OnOrderChanged;

    // Properties
    public List<ItemDataSO> AvailableItems => availableItems;
    #endregion

    #region Delivery System
    [Header("Delivery System")]
    [SerializeField] private GameObject deliveryBoxPrefab;
    [SerializeField] private Transform deliverySpawnPoint;

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

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

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
        // Clear previous pending delivery
        pendingDelivery.Clear();

        // Store new order for next morning
        foreach (var kvp in orderItems)
        {
            pendingDelivery[kvp.Key] = kvp.Value;
        }

        Debug.Log($"Order stored for delivery. {pendingDelivery.Count} item types to deliver.");
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

        // Spawn a box for each item type
        Vector3 spawnPos = deliverySpawnPoint != null ? deliverySpawnPoint.position : Vector3.zero;
        int boxIndex = 0;

        foreach (var kvp in pendingDelivery)
        {
            if (deliveryBoxPrefab == null) continue;

            // Offset boxes slightly so they don't overlap
            Vector3 offset = new Vector3(boxIndex * 1.5f, 0, 0);
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
            pendingDelivery.Clear();
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
