using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestockItemButton : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI sellPriceText;
    [SerializeField] private Button button;

    private ItemDataSO itemData;
    private System.Action<ItemDataSO> onClickCallback;

    private void Awake() {
        // Get button component if not assigned
        if (button == null) {
            button = GetComponent<Button>();
        }

        // Add click listener
        if (button != null) {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    /// <summary>
    /// Initialize the button with item data and callback
    /// </summary>
    public void Setup(ItemDataSO itemData, int quantity, System.Action<ItemDataSO> onClickCallback) {
        this.itemData = itemData;
        this.onClickCallback = onClickCallback;

        // Update UI elements
        if (itemIcon != null && itemData.itemSprite != null) {
            itemIcon.sprite = itemData.itemSprite;
            itemIcon.enabled = true;
        } else if (itemIcon != null) {
            itemIcon.enabled = false; // Hide if no sprite
        }

        if (itemNameText != null) {
            itemNameText.text = itemData.itemName;
        }

        if (quantityText != null) {
            quantityText.text = $"x{quantity}";
        }

        if (sellPriceText != null) {
            sellPriceText.text = $"Sells: ${itemData.sellPrice}";
        }
    }

    /// <summary>
    /// Called when button is clicked
    /// </summary>
    private void OnButtonClick() {
        if (itemData != null && onClickCallback != null) {
            onClickCallback.Invoke(itemData);
        }
    }

    private void OnDestroy() {
        // Cleanup listener
        if (button != null) {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}
