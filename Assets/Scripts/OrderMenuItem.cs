using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderMenuItem : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;

    [SerializeField] private TextMeshProUGUI itemCostText;
    [SerializeField] private TextMeshProUGUI inventoryCountText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button addToCartButton;

    [Header("Bulk Order (Optional - hidden if not unlocked)")]
    [SerializeField] private Button addBulkToCartButton;
    [SerializeField] private TextMeshProUGUI bulkCostText;

    private ItemDataSO itemData;
    private int currentQuantity = 1;
    private const int MIN_QUANTITY = 1;
    private const int MAX_QUANTITY = 99;

    private void Start() {
        if (decreaseButton != null) {
            decreaseButton.onClick.AddListener(DecreaseQuantity);
        }

        if (increaseButton != null) {
            increaseButton.onClick.AddListener(IncreaseQuantity);
        }

        if (addToCartButton != null) {
            addToCartButton.onClick.AddListener(AddToCart);
        }

        if (addBulkToCartButton != null) {
            addBulkToCartButton.onClick.AddListener(AddBulkToCart);
        }

        UpdateQuantityDisplay();
        UpdateBulkOrderDisplay();
    }

    public void Initialize(ItemDataSO data) {
        itemData = data;

        if (itemNameText != null) {
            itemNameText.text = itemData.itemName;
        }

        if (itemCostText != null) {
            itemCostText.text = $"${itemData.restockCost}";
        }

        if (itemIconImage != null && itemData.itemSprite != null) {
            itemIconImage.sprite = itemData.itemSprite;
            itemIconImage.enabled = true;
        } else if (itemIconImage != null) {
            // Hide icon if no sprite available
            itemIconImage.enabled = false;
        }

        UpdateQuantityDisplay();
        UpdateInventoryCount();
        UpdateBulkOrderDisplay();

        // Subscribe to inventory changes
        if (InventoryManager.Instance != null) {
            InventoryManager.Instance.OnInventoryChanged += UpdateInventoryCount;
        }
    }

    private void OnDestroy() {
        if (InventoryManager.Instance != null) {
            InventoryManager.Instance.OnInventoryChanged -= UpdateInventoryCount;
        }
    }

    private void UpdateInventoryCount() {
        if (inventoryCountText == null) {
            Debug.LogWarning("Inventory Count Text is null in OrderMenuItem!");
            return;
        }

        if (itemData == null) {
            Debug.LogWarning("Item Data is null in OrderMenuItem!");
            return;
        }

        if (InventoryManager.Instance == null) {
            Debug.LogError("InventoryManager.Instance is null!");
            return;
        }

        int count = InventoryManager.Instance.GetItemCount(itemData);
        inventoryCountText.text = $"In Stock: {count}";
        Debug.Log($"Updated inventory count for {itemData.itemName}: {count}");
    }

    private void IncreaseQuantity() {
        if (currentQuantity < MAX_QUANTITY) {
            currentQuantity++;
            UpdateQuantityDisplay();
        }
    }

    private void DecreaseQuantity() {
        if (currentQuantity > MIN_QUANTITY) {
            currentQuantity--;
            UpdateQuantityDisplay();
        }
    }

    private void UpdateQuantityDisplay() {
        if (quantityText != null) {
            quantityText.text = currentQuantity.ToString();
        }

        if (decreaseButton != null) {
            decreaseButton.interactable = currentQuantity > MIN_QUANTITY;
        }

        if (increaseButton != null) {
            increaseButton.interactable = currentQuantity < MAX_QUANTITY;
        }
    }

    private void AddToCart() {
        if (itemData != null) {
            SupplyChainManager.Instance.AddToOrder(itemData, currentQuantity);
            currentQuantity = 1;
            UpdateQuantityDisplay();
        }
    }

    private void AddBulkToCart() {
        if (itemData != null && SupplyChainManager.Instance != null) {
            // Bulk order: 5x the current quantity with 10% discount
            int bulkQuantity = currentQuantity * 5;
            SupplyChainManager.Instance.AddToBulkOrder(itemData, bulkQuantity);
            currentQuantity = 1;
            UpdateQuantityDisplay();

            Debug.Log($"Added {bulkQuantity}x {itemData.itemName} to bulk order (5x multiplier, 10% discount)");
        }
    }

    /// <summary>
    /// Updates the bulk order button visibility and cost display based on upgrade status.
    /// </summary>
    private void UpdateBulkOrderDisplay() {
        if (SupplyChainManager.Instance == null) return;

        bool bulkEnabled = SupplyChainManager.Instance.IsBulkOrderingEnabled;

        // Show/hide bulk button based on upgrade
        if (addBulkToCartButton != null) {
            addBulkToCartButton.gameObject.SetActive(bulkEnabled);
        }

        // Update bulk cost text
        if (bulkCostText != null && itemData != null) {
            int bulkQuantity = currentQuantity * 5;
            int regularCost = itemData.restockCost * bulkQuantity;
            int discountedCost = Mathf.RoundToInt(regularCost * 0.9f); // 10% discount

            bulkCostText.text = $"5x: ${discountedCost} (10% off)";
            bulkCostText.gameObject.SetActive(bulkEnabled);
        }
    }
}