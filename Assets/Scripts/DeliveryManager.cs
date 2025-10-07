using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StartingDeliveryItem
{
    public ItemDataSO item;
    public int quantity;
}

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    [Header("Delivery Settings")]
    [SerializeField] private GameObject deliveryBoxPrefab;
    [SerializeField] private Transform deliverySpawnPoint;

    [Header("Starting Delivery (Day 1)")]
    [SerializeField] private List<StartingDeliveryItem> startingDelivery = new List<StartingDeliveryItem>();

    private Dictionary<ItemDataSO, int> pendingDelivery = new Dictionary<ItemDataSO, int>();
    private List<DeliveryBox> activeBoxes = new List<DeliveryBox>();

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

    private void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Morning)
        {
            SpawnDeliveryBoxes();
        }
    }

    public void AddPendingDelivery(Dictionary<ItemDataSO, int> orderItems)
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

    public bool HasPendingDeliveries()
    {
        return activeBoxes.Count > 0;
    }
}
