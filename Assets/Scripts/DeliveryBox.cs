using UnityEngine;

public class DeliveryBox : MonoBehaviour
{
    [Header("Box Contents")]
    private ItemDataSO itemData;
    private int quantity;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private GameObject promptUI;

    private Transform playerTransform;
    private bool playerNearby = false;

    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Hide prompt initially
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    public void Initialize(ItemDataSO data, int qty)
    {
        itemData = data;
        quantity = qty;
        Debug.Log($"Delivery box initialized: {quantity}x {itemData.itemName}");
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Check if player is nearby
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        playerNearby = distance <= interactionRange;

        // Show/hide prompt
        if (promptUI != null)
        {
            promptUI.SetActive(playerNearby);
        }

        // Handle interaction
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            OpenBox();
        }
    }

    private void OpenBox()
    {
        if (itemData == null)
        {
            Debug.LogWarning("Cannot open box - no item data!");
            return;
        }

        // Add items to inventory
        InventoryManager.Instance.AddToInventory(itemData, quantity);
        Debug.Log($"Opened delivery box: {quantity}x {itemData.itemName} added to inventory");

        // Notify delivery manager
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnBoxOpened(this);
        }

        // Destroy box
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize interaction range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
