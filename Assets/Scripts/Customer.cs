using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour {
    [SerializeField] private Transform itemCarryPoint;
    [SerializeField] private Transform visualParent;
    [SerializeField] private GameObject[] visualPrefabs;

    [Header("Demand Bubble System")]
    [SerializeField] private GameObject demandBubblePrefab;
    [SerializeField] private Canvas demandBubbleCanvas;
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0, 2.5f, 0);
    [SerializeField] private float bubbleDuration = 2.5f;

    private CustomerTypeDataSO customerType;
    private List<Item> carriedItems = new List<Item>();
    private int desiredItemCount;
    private List<ItemDataSO> specificItemRequests = new List<ItemDataSO>(); // Trending items customer wants
    private int randomItemCount; // Remaining slots for random browsing
    private float currentPatience;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private bool waitingForCheckout = false;
    private CheckoutCounter checkoutCounter;
    private NavMeshAgent agent;

    public bool IsMoving => isMoving;
    public CustomerTypeDataSO CustomerType => customerType;

    public void Initialize(CustomerTypeDataSO type) {
        customerType = type;

        if (customerType == null)
        {
            Debug.LogError("Customer initialized without CustomerTypeDataSO!");
            Destroy(gameObject);
            return;
        }

        // Initialize NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = customerType.moveSpeed;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;
            agent.autoBraking = true;
            agent.radius = 0.3f;
            agent.height = 2.0f;
            // Phase 1: Disable obstacle avoidance so customers can walk through each other
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
        else
        {
            Debug.LogError("Customer missing NavMeshAgent component!");
        }

        // Spawn random visual prefab
        if (visualPrefabs != null && visualPrefabs.Length > 0 && visualParent != null)
        {
            int randomIndex = Random.Range(0, visualPrefabs.Length);
            GameObject visualInstance = Instantiate(visualPrefabs[randomIndex], visualParent);
            visualInstance.transform.localPosition = Vector3.zero;
            visualInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("Customer visual prefabs array is empty or visualParent is not assigned!");
        }

        // Initialize behavior parameters
        desiredItemCount = customerType.GetRandomItemCount();
        currentPatience = customerType.initialPatience;

        // Generate specific item requests based on specificItemRatio
        GenerateSpecificItemRequests();

        // Calculate remaining slots for random browsing
        randomItemCount = desiredItemCount - specificItemRequests.Count;

        Debug.Log($"Customer spawned: {customerType.customerTypeName} wants {desiredItemCount} items ({specificItemRequests.Count} specific, {randomItemCount} random)");

        // Auto-find demand bubble canvas if not assigned
        if (demandBubbleCanvas == null)
        {
            // Find WorldSpaceUICanvas (supports hyphens like WorldSpaceUICanvas----)
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.gameObject.name.StartsWith("WorldSpaceUICanvas"))
                {
                    demandBubbleCanvas = canvas;
                    break;
                }
            }

            if (demandBubbleCanvas == null)
            {
                Debug.LogWarning("Customer: No canvas starting with 'WorldSpaceUICanvas' found in scene. Demand bubbles will not work.");
            }
        }

        // Play door bell sound when customer enters
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySound(SoundType.DoorBell);
        }

        StartCoroutine(ShoppingRoutine());
    }

    /// <summary>
    /// Generates specific item requests from trending items based on customer type's specificItemRatio.
    /// Falls back to random browsing if TrendManager is not available.
    /// </summary>
    private void GenerateSpecificItemRequests()
    {
        specificItemRequests.Clear();

        // Safety check: If TrendManager not in scene, skip specific requests (fall back to random browsing)
        if (TrendManager.Instance == null)
        {
            Debug.LogWarning("TrendManager not found - customer will browse randomly (demand system disabled)");
            return;
        }

        // Calculate how many items should be specific requests
        int specificCount = Mathf.RoundToInt(desiredItemCount * customerType.specificItemRatio);
        specificCount = Mathf.Clamp(specificCount, 0, desiredItemCount);

        List<ItemDataSO> trendingItems = TrendManager.Instance.GetTrendingItems();

        if (trendingItems == null || trendingItems.Count == 0)
        {
            Debug.LogWarning("No trending items available - customer will browse randomly");
            return;
        }

        // Randomly select specific items from trending items (allow duplicates)
        for (int i = 0; i < specificCount; i++)
        {
            ItemDataSO randomTrendingItem = trendingItems[Random.Range(0, trendingItems.Count)];
            specificItemRequests.Add(randomTrendingItem);

            // Record demand for this item (optional - skip if DemandTracker not available)
            if (DemandTracker.Instance != null)
            {
                DemandTracker.Instance.RecordItemWanted(randomTrendingItem);
            }
        }

        if (specificItemRequests.Count > 0)
        {
            Debug.Log($"Customer wants specific items: {string.Join(", ", specificItemRequests.Select(item => item.itemName))}");
        }
    }

    private IEnumerator ShoppingRoutine() {
        // Wait before starting shopping
        yield return new WaitForSeconds(0.5f);

        Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);

        if (shelves == null || shelves.Length == 0)
        {
            Debug.LogWarning("Customer: No shelves found in scene!");
            yield break;
        }

        // ===== PHASE A: Search for Specific Items =====
        for (int i = 0; i < specificItemRequests.Count; i++) {
            if (shelves.Length == 0) break;

            ItemDataSO wantedItem = specificItemRequests[i];
            if (wantedItem == null) continue;

            // Show demand bubble with wanted item icon
            ShowDemandBubble(wantedItem.itemSprite, DemandBubble.BubbleState.Wanted);

            // Find shelves that have this specific item
            List<Shelf> shelvesWithItem = shelves
                .Where(shelf => shelf.HasItemType(wantedItem))
                .ToList();

            Shelf targetShelf = null;

            if (shelvesWithItem.Count > 0) {
                // Found shelves with the item - pick one randomly
                targetShelf = shelvesWithItem[Random.Range(0, shelvesWithItem.Count)];
            } else {
                // Item not on any shelf - pick random shelf to check anyway
                targetShelf = shelves[Random.Range(0, shelves.Length)];
            }

            // Move to shelf
            Vector3 shelfPos = targetShelf.transform.position + new Vector3(0, 0, -1f);
            MoveToPosition(shelfPos);
            while (isMoving) {
                yield return null;
            }

            // Check if shelf has the wanted item
            if (targetShelf.HasItemType(wantedItem)) {
                // Browse shelf
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));

                // Take the specific item
                Item item = targetShelf.TakeItem(wantedItem);
                if (item != null) {
                    carriedItems.Add(item);
                    // Position items in carry point (stack them slightly offset)
                    if (itemCarryPoint != null) {
                        item.transform.SetParent(itemCarryPoint);
                        item.transform.localPosition = new Vector3(0, carriedItems.Count * 0.1f, 0);
                    }

                    // Show positive bubble (got the item!)
                    ShowDemandBubble(wantedItem.itemSprite, DemandBubble.BubbleState.Positive);

                    Debug.Log($"Customer found wanted item: {item.GetItemName()} ({i + 1}/{specificItemRequests.Count} specific items)");
                }
            } else {
                // Shelf doesn't have wanted item - show negative bubble
                ShowDemandBubble(wantedItem.itemSprite, DemandBubble.BubbleState.Negative);
                Debug.Log($"Customer couldn't find wanted item: {wantedItem.itemName}");

                // Short disappointed pause
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            }
        }

        // ===== PHASE B: Fill Remaining Basket with Random Browsing =====
        for (int i = 0; i < randomItemCount; i++) {
            if (shelves.Length == 0) break;

            // Pick a random shelf
            Shelf targetShelf = shelves[Random.Range(0, shelves.Length)];

            // Move to shelf
            Vector3 shelfPos = targetShelf.transform.position + new Vector3(0, 0, -1f);
            MoveToPosition(shelfPos);
            while (isMoving) {
                yield return null;
            }

            // Check if shelf has items
            if (targetShelf != null && !targetShelf.IsEmpty) {
                // Browse shelf
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));

                // Take random item from shelf
                ItemDataSO randomItemType = targetShelf.GetRandomAvailableItemType();
                Item item = targetShelf.TakeItem(randomItemType);
                if (item != null) {
                    carriedItems.Add(item);
                    // Position items in carry point
                    if (itemCarryPoint != null) {
                        item.transform.SetParent(itemCarryPoint);
                        item.transform.localPosition = new Vector3(0, carriedItems.Count * 0.1f, 0);
                    }
                    Debug.Log($"Customer randomly picked up {item.GetItemName()} ({carriedItems.Count}/{desiredItemCount} total items)");
                }
            } else {
                // Shelf is empty - short pause
                Debug.Log($"Customer found empty shelf during random browsing");
                yield return new WaitForSeconds(Random.Range(0.25f, 0.5f));
            }
        }

        // If didn't get any items, leave disappointed
        if (carriedItems.Count == 0) {
            Debug.Log($"Customer leaving disappointed - no items found in entire store!");
            DayManager.Instance?.RecordCustomerLeft();
            yield return new WaitForSeconds(2f);
            NotifySpawnerAndLeave();
            yield break;
        }

        // Find checkout counter and join queue
        checkoutCounter = FindFirstObjectByType<CheckoutCounter>();
        if (checkoutCounter != null) {
            Debug.Log($"Customer heading to checkout with {carriedItems.Count} items");

            waitingForCheckout = true;
            checkoutCounter.JoinQueue(this);

            // Wait for our turn at checkout (with patience system)
            while (waitingForCheckout) {
                currentPatience -= customerType.patienceDrainRate * Time.deltaTime;

                // Could add impatient behavior here if patience runs out
                if (currentPatience <= 0) {
                    Debug.Log($"Customer lost patience and left!");
                    // For now, just continue waiting
                }

                yield return null;
            }
        }
        else {
            Debug.LogWarning("No CheckoutCounter found in scene!");
        }

        // Leave store
        yield return new WaitForSeconds(1f);
        NotifySpawnerAndLeave();
    }

    private void NotifySpawnerAndLeave() {
        if (CustomerSpawner.Instance != null) {
            CustomerSpawner.Instance.OnCustomerLeft();
        }

        Destroy(gameObject);
    }

    public void ApproachCheckout(Vector3 checkoutPosition) {
        Debug.Log("Approaching checkout counter");
        MoveToPosition(checkoutPosition);
    }

    public void CompleteTransaction() {
        Debug.Log($"Completing transaction for {carriedItems.Count} items");

        // Process payment for all carried items
        int totalPrice = 0;
        Dictionary<ItemDataSO, int> itemCounts = new Dictionary<ItemDataSO, int>();

        foreach (Item item in carriedItems) {
            if (item != null) {
                int salePrice = item.GetSellPrice();
                totalPrice += salePrice;

                // Track items sold by type for objectives
                ItemDataSO itemData = item.GetItemData();
                if (itemData != null) {
                    if (itemCounts.ContainsKey(itemData)) {
                        itemCounts[itemData]++;
                    } else {
                        itemCounts[itemData] = 1;
                    }
                }

                Destroy(item.gameObject);
            }
        }

        if (totalPrice > 0) {
            GameManager.Instance.AddMoney(totalPrice);
            DayManager.Instance?.RecordCustomerSale(totalPrice);
            Debug.Log($"Transaction complete: ${totalPrice} for {carriedItems.Count} items");

            // Report items sold to CheckoutCounter for objective tracking
            if (CheckoutCounter.Instance != null) {
                foreach (var kvp in itemCounts) {
                    CheckoutCounter.Instance.ReportItemSold(kvp.Key, kvp.Value);
                }
            }
        }

        carriedItems.Clear();

        // Signal we're done waiting
        waitingForCheckout = false;
    }

    private void MoveToPosition(Vector3 target) {
        targetPosition = target;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target);
            isMoving = true;
        }
        else
        {
            Debug.LogWarning("Customer NavMeshAgent is not on NavMesh!");
            isMoving = false;
        }
    }

    /// <summary>
    /// Shows a demand bubble above the customer with the specified item icon and state.
    /// </summary>
    private void ShowDemandBubble(Sprite itemSprite, DemandBubble.BubbleState state)
    {
        if (demandBubblePrefab == null || demandBubbleCanvas == null)
        {
            Debug.LogWarning("DemandBubble prefab or canvas not assigned on Customer!");
            return;
        }

        if (itemSprite == null)
        {
            Debug.LogWarning("Cannot show demand bubble - item sprite is null!");
            return;
        }

        // Instantiate demand bubble as child of canvas
        GameObject bubbleObj = Instantiate(demandBubblePrefab, demandBubbleCanvas.transform);
        DemandBubble bubble = bubbleObj.GetComponent<DemandBubble>();

        if (bubble != null)
        {
            bubble.Initialize(itemSprite, state, transform, bubbleOffset, bubbleDuration);
        }
        else
        {
            Debug.LogWarning("DemandBubble component not found on prefab!");
            Destroy(bubbleObj);
        }
    }

    private void Update() {
        if (isMoving && agent != null && agent.isOnNavMesh) {
            // Check if NavMeshAgent reached destination
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isMoving = false;
            }
        }
    }
}