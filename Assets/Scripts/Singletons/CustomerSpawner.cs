using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour {
    public static CustomerSpawner Instance;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Customer Types")]
    [SerializeField] private CustomerTypeDataSO[] customerTypes;

    [Header("Day Settings")]
    [SerializeField] private int baseCustomersPerDay = 6;
    [SerializeField, Tooltip("Minimum time between customer spawns (seconds)")]
    private float minSpawnInterval = 1.0f;
    [SerializeField, Tooltip("Maximum time between customer spawns (seconds)")]
    private float maxSpawnInterval = 2.0f;
    [SerializeField] private int bonusCustomers = 0;
    private int customersSpawned = 0;
    private int customersLeft = 0;
    private int totalCustomersForDay = 0;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    public int CustomersSpawned => customersSpawned;
    public int CustomersLeft => customersLeft;
    public int TotalCustomersForDay => totalCustomersForDay;
    public int CustomersPerDay => GetBaseCustomersForSegments() + bonusCustomers;

    /// <summary>
    /// Gets the base number of customers based on how many shop segments are unlocked.
    /// Auto-scales customer count with shop expansion to maintain 60-80% turnover.
    /// </summary>
    private int GetBaseCustomersForSegments()
    {
        // Count unlocked segments (1-4, since segment 0 is always unlocked)
        int unlockedSegments = 1; // Default to 1 (segment 0)
        if (ShopSegmentManager.Instance != null)
        {
            unlockedSegments = ShopSegmentManager.Instance.UnlockedSegmentCount;
        }

        // Scale customers with shop expansion
        // Segment 0 only (1 segment): 6 customers (20 slots × 70% = 14 items ÷ 2.5 items/customer = 6)
        // Segments 0-1 (2 segments): 12 customers (45 slots × 70% = 31 items ÷ 2.5 = 12)
        // Segments 0-2 (3 segments): 17 customers (62 slots × 70% = 43 items ÷ 2.5 = 17)
        // Segments 0-3 (4 segments): 22 customers (80 slots × 70% = 56 items ÷ 2.5 = 22)
        switch (unlockedSegments)
        {
            case 1: return 6;  // Segment 0 only
            case 2: return 12; // Segments 0-1
            case 3: return 17; // Segments 0-2
            case 4: return 22; // Segments 0-3 (all segments)
            default: return baseCustomersPerDay; // Fallback to base value
        }
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start() {
        if (DayManager.Instance != null) {
            DayManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }
    }

    private void OnDestroy() {
        if (DayManager.Instance != null) {
            DayManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    private void OnPhaseChanged(GamePhase phase) {
        if (phase == GamePhase.OpenForBusiness && !isSpawning) {
            StartSpawningWave();
        }
    }

    private void StartSpawningWave() {
        // Reset counters for new day
        customersSpawned = 0;
        customersLeft = 0;
        totalCustomersForDay = CustomersPerDay;

        Debug.Log($"Starting customer wave: {totalCustomersForDay} customers (Base: {baseCustomersPerDay} + Bonus: {bonusCustomers})");
        spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());
    }

    /// <summary>
    /// Adds bonus customers (called by UpgradeManager when Extended Hours upgrade is purchased).
    /// </summary>
    public void AddBonusCustomers(int count)
    {
        bonusCustomers += count;
        Debug.Log($"Customer bonus increased by {count}. New total: {CustomersPerDay} customers/day");
    }

    private IEnumerator SpawnWaveCoroutine() {
        isSpawning = true;

        for (int i = 0; i < totalCustomersForDay; i++) {
            SpawnCustomer();
            customersSpawned++;

            // Wait before spawning next customer (except for last one)
            if (i < totalCustomersForDay - 1) {
                float randomDelay = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(randomDelay);
            }
        }

        isSpawning = false;
        spawnCoroutine = null;
        Debug.Log($"Wave complete: {customersSpawned} customers spawned");
    }

    private void SpawnCustomer() {
        if (customerPrefab == null) {
            Debug.LogError("Customer prefab not assigned!");
            return;
        }

        // Pick a random customer type
        CustomerTypeDataSO customerType = GetRandomCustomerType();
        if (customerType == null) {
            Debug.LogError("No customer types assigned to spawner!");
            return;
        }

        // Use spawn point if assigned, otherwise fall back to this transform
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject customerObj = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);
        Customer customer = customerObj.GetComponent<Customer>();

        if (customer != null) {
            customer.Initialize(customerType);
            Debug.Log($"Customer {customersSpawned + 1}/{totalCustomersForDay} spawned: {customerType.customerTypeName}");
        }
        else {
            Debug.LogError("Customer prefab missing Customer component!");
            Destroy(customerObj);
        }
    }

    /// <summary>
    /// Gets a random customer type from the available types array.
    /// </summary>
    private CustomerTypeDataSO GetRandomCustomerType() {
        if (customerTypes == null || customerTypes.Length == 0) {
            return null;
        }

        return customerTypes[Random.Range(0, customerTypes.Length)];
    }

    public void OnCustomerLeft() {
        customersLeft++;
        Debug.Log($"Customer left. Progress: {customersLeft}/{totalCustomersForDay}");

        // Check if all customers have left
        if (customersLeft >= totalCustomersForDay) {
            Debug.Log("All customers have left - ending day");
            if (DayManager.Instance != null) {
                DayManager.Instance.EndDay();
            }
        }
    }

    /// <summary>
    /// Stops spawning new customers immediately (used for manual shop closure).
    /// </summary>
    public void StopSpawning()
    {
        if (isSpawning && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            isSpawning = false;
            Debug.Log($"Customer spawning stopped early. Spawned {customersSpawned}/{totalCustomersForDay} customers.");
        }
    }

    /// <summary>
    /// Checks if all spawned customers have left the shop.
    /// </summary>
    public bool AreAllCustomersGone()
    {
        return customersLeft >= customersSpawned;
    }
}