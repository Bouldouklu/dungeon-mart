using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderMenuItem : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameText;

    [SerializeField] private TextMeshProUGUI itemCostText;
    [SerializeField] private TextMeshProUGUI inventoryCountText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button addToCartButton;

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

        UpdateQuantityDisplay();
    }

    public void Initialize(ItemDataSO data) {
        itemData = data;

        if (itemNameText != null) {
            itemNameText.text = itemData.itemName;
        }

        if (itemCostText != null) {
            itemCostText.text = $"${itemData.restockCost}";
        }

        UpdateQuantityDisplay();
        UpdateInventoryCount();

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
            OrderManager.Instance.AddToOrder(itemData, currentQuantity);
            currentQuantity = 1;
            UpdateQuantityDisplay();
        }
    }
}