using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    [Header("Delivery Settings")]
    [SerializeField] private GameObject deliveryBoxPrefab;
    [SerializeField] private Transform deliverySpawnPoint;

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
