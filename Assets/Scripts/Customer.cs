using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour {
    [SerializeField] private Transform itemCarryPoint;
    [SerializeField] private Transform visualParent;
    [SerializeField] private GameObject[] visualPrefabs;

    private CustomerTypeDataSO customerType;
    private List<Item> carriedItems = new List<Item>();
    private int desiredItemCount;
    private float currentPatience;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private bool waitingForCheckout = false;
    private CheckoutCounter checkoutCounter;

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

        Debug.Log($"Customer spawned: {customerType.customerTypeName} wants {desiredItemCount} items");

        // Show entry dialogue
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowRandomDialogue(customerType.entryDialogues, transform);
        }

        // Play door bell sound when customer enters
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySound(SoundType.DoorBell);
        }

        StartCoroutine(ShoppingRoutine());
    }

    private IEnumerator ShoppingRoutine() {
        // Wait before starting shopping
        yield return new WaitForSeconds(0.5f);

        Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);

        // Try to collect desired number of items
        for (int i = 0; i < desiredItemCount; i++) {
            if (shelves.Length == 0) break;

            // Pick a random shelf
            Shelf targetShelf = shelves[Random.Range(0, shelves.Length)];

            // Move to shelf
            Vector3 shelfPos = targetShelf.transform.position + new Vector3(0, -1f, 0);
            MoveToPosition(shelfPos);
            while (isMoving) {
                yield return null;
            }

            // Check if shelf has items FIRST, then show appropriate dialogue
            if (targetShelf != null && !targetShelf.IsEmpty) {
                // Shelf has items - show browsing dialogue
                if (DialogueManager.Instance != null && customerType.browsingDialogues.Length > 0) {
                    DialogueManager.Instance.ShowRandomDialogue(customerType.browsingDialogues, transform);
                }
                yield return new WaitForSeconds(customerType.browseTime);

                // Take item (randomly choose from available item types)
                ItemDataSO randomItemType = targetShelf.GetRandomAvailableItemType();
                Item item = targetShelf.TakeItem(randomItemType);
                if (item != null) {
                    carriedItems.Add(item);
                    // Position items in carry point (stack them slightly offset)
                    if (itemCarryPoint != null) {
                        item.transform.SetParent(itemCarryPoint);
                        item.transform.localPosition = new Vector3(0, carriedItems.Count * 0.1f, 0);
                    }
                    Debug.Log($"Customer picked up {item.GetItemName()} - item {i + 1}/{desiredItemCount}");
                }
            } else {
                // Shelf is empty - show disappointed dialogue immediately
                if (DialogueManager.Instance != null && customerType.disappointedDialogues != null && customerType.disappointedDialogues.Length > 0) {
                    DialogueManager.Instance.ShowRandomDialogue(customerType.disappointedDialogues, transform);
                }
                Debug.Log($"Customer found empty shelf - no items to browse");
                yield return new WaitForSeconds(customerType.browseTime * 0.5f); // Shorter wait for empty shelf
            }
        }

        // If didn't get any items, leave disappointed
        if (carriedItems.Count == 0) {
            Debug.Log($"Customer leaving disappointed - no items found in entire store!");

            // Validate disappointed dialogues exist
            if (customerType.disappointedDialogues == null || customerType.disappointedDialogues.Length == 0) {
                Debug.LogWarning($"CustomerTypeDataSO '{customerType.customerTypeName}' has no disappointed dialogues configured!");
            }

            // Show final disappointed dialogue when leaving (they already complained at each empty shelf)
            // This serves as a "giving up" message
            if (DialogueManager.Instance != null && customerType.disappointedDialogues != null && customerType.disappointedDialogues.Length > 0) {
                DialogueManager.Instance.ShowRandomDialogue(customerType.disappointedDialogues, transform);
            }

            DayManager.Instance?.RecordCustomerLeft();
            yield return new WaitForSeconds(2f); // Wait to show dialogue
            NotifySpawnerAndLeave();
            yield break;
        }

        // Find checkout counter and join queue
        checkoutCounter = FindFirstObjectByType<CheckoutCounter>();
        if (checkoutCounter != null) {
            Debug.Log($"Customer heading to checkout with {carriedItems.Count} items");
            if (DialogueManager.Instance != null) {
                DialogueManager.Instance.ShowRandomDialogue(customerType.checkoutDialogues, transform);
            }

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

        // Show exit dialogue
        if (DialogueManager.Instance != null) {
            DialogueManager.Instance.ShowRandomDialogue(customerType.exitDialogues, transform);
        }

        // Leave store
        yield return new WaitForSeconds(1f); // Brief delay to show exit dialogue
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
        foreach (Item item in carriedItems) {
            if (item != null) {
                int salePrice = item.GetSellPrice();
                totalPrice += salePrice;
                Destroy(item.gameObject);
            }
        }

        if (totalPrice > 0) {
            GameManager.Instance.AddMoney(totalPrice);
            DayManager.Instance?.RecordCustomerSale(totalPrice);
            Debug.Log($"Transaction complete: ${totalPrice} for {carriedItems.Count} items");
        }

        carriedItems.Clear();

        // Signal we're done waiting
        waitingForCheckout = false;
    }

    private void MoveToPosition(Vector3 target) {
        targetPosition = target;
        isMoving = true;
    }

    private void Update() {
        if (isMoving && customerType != null) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, customerType.moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f) {
                isMoving = false;
            }
        }
    }
}