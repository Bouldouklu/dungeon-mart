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
    [SerializeField] private float spawnInterval = 3f;

    private int bonusCustomers = 0;
    private int customersSpawned = 0;
    private int customersLeft = 0;
    private int totalCustomersForDay = 0;
    private bool isSpawning = false;

    public int CustomersSpawned => customersSpawned;
    public int CustomersLeft => customersLeft;
    public int TotalCustomersForDay => totalCustomersForDay;
    public int CustomersPerDay => baseCustomersPerDay + bonusCustomers;

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
        StartCoroutine(SpawnWaveCoroutine());
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
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        isSpawning = false;
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
}