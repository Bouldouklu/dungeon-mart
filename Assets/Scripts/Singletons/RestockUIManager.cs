using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestockUIManager : MonoBehaviour {
    public static RestockUIManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject restockPanel;
    [SerializeField] private Transform itemButtonContainer;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Button closeButton;

    [Header("Optional UI Elements")]
    [SerializeField] private TextMeshProUGUI messageText; // Optional: for "No compatible items" message

    private Shelf currentShelf;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Ensure UI is hidden on start
        if (restockPanel != null) {
            restockPanel.SetActive(false);
        }

        // Setup close button
        if (closeButton != null) {
            closeButton.onClick.AddListener(HideRestockUI);
        }
    }

    /// <summary>
    /// Show the restock UI for a specific shelf, filtered by compatible items.
    /// Triggered by clicking on a shelf in the game world.
    /// </summary>
    public void ShowRestockUI(Shelf shelf) {
        if (shelf == null || restockPanel == null || itemButtonContainer == null) {
            Debug.LogWarning("RestockUIManager: Missing required references");
            return;
        }

        currentShelf = shelf;

        // Clear any existing buttons
        ClearItemButtons();

        // Get filtered inventory items
        Dictionary<ItemDataSO, int> inventory = InventoryManager.Instance.GetAllInventory();
        var allowedCategories = shelf.ShelfType.allowedCategories;

        // Filter items by shelf's allowed categories
        var compatibleItems = inventory
            .Where(kvp => allowedCategories.Contains(kvp.Key.itemCategory) && kvp.Value > 0)
            .ToList();

        if (compatibleItems.Count == 0) {
            // No compatible items - show message or just close
            if (messageText != null) {
                string categoriesStr = string.Join(", ", allowedCategories);
                messageText.text = $"No {categoriesStr} items in inventory";
                messageText.gameObject.SetActive(true);
            }
            string categoriesLog = string.Join(", ", allowedCategories);
            Debug.Log($"No {categoriesLog} items available for {shelf.ShelfType.shelfTypeName}");
        } else {
            // Hide message if exists
            if (messageText != null) {
                messageText.gameObject.SetActive(false);
            }

            // Create buttons for each compatible item
            foreach (var kvp in compatibleItems) {
                CreateItemButton(kvp.Key, kvp.Value);
            }
        }

        // Show the panel
        restockPanel.SetActive(true);

        // Disable player interactions while UI is open (backwards compatible with old movement system)
        DisablePlayerMovement();
    }

    /// <summary>
    /// Hide the restock UI and clean up
    /// </summary>
    public void HideRestockUI() {
        if (restockPanel != null) {
            restockPanel.SetActive(false);
        }

        ClearItemButtons();
        currentShelf = null;

        // Hide message if exists
        if (messageText != null) {
            messageText.gameObject.SetActive(false);
        }

        // Re-enable player movement when UI closes
        EnablePlayerMovement();
    }

    /// <summary>
    /// Check if the restock UI is currently open
    /// </summary>
    public bool IsUIOpen() {
        return restockPanel != null && restockPanel.activeSelf;
    }

    /// <summary>
    /// Create a button for a specific inventory item
    /// </summary>
    private void CreateItemButton(ItemDataSO itemData, int quantity) {
        if (itemButtonPrefab == null || itemButtonContainer == null) return;

        GameObject buttonObj = Instantiate(itemButtonPrefab, itemButtonContainer);
        spawnedButtons.Add(buttonObj);

        // Setup the button component
        RestockItemButton itemButton = buttonObj.GetComponent<RestockItemButton>();
        if (itemButton != null) {
            itemButton.Setup(itemData, quantity, OnItemButtonClicked);
        } else {
            Debug.LogWarning("RestockItemButton component not found on prefab!");
        }
    }

    /// <summary>
    /// Called when player clicks an item button
    /// </summary>
    private void OnItemButtonClicked(ItemDataSO itemData) {
        if (currentShelf == null || itemData == null) return;

        // Attempt to restock the shelf with this item
        bool success = currentShelf.RestockShelf(itemData, 1);

        if (success) {
            Debug.Log($"Successfully restocked {itemData.itemName}");
            // Refresh UI to show updated inventory (keep UI open for multiple restocks)
            RefreshUI();
        } else {
            Debug.LogWarning($"Failed to restock {itemData.itemName} - shelf may be full");
            // Refresh the UI to show updated inventory
            RefreshUI();
        }
    }

    /// <summary>
    /// Refresh the UI without closing (useful if restock fails due to full shelf)
    /// </summary>
    private void RefreshUI() {
        if (currentShelf != null) {
            ShowRestockUI(currentShelf);
        }
    }

    /// <summary>
    /// Clear all spawned item buttons
    /// </summary>
    private void ClearItemButtons() {
        foreach (GameObject button in spawnedButtons) {
            if (button != null) {
                Destroy(button);
            }
        }
        spawnedButtons.Clear();
    }

    /// <summary>
    /// Disable player movement (called when UI opens)
    /// </summary>
    private void DisablePlayerMovement() {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null) {
            player.SetCanMove(false);
        }
    }

    /// <summary>
    /// Enable player movement (called when UI closes)
    /// </summary>
    private void EnablePlayerMovement() {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null) {
            player.SetCanMove(true);
        }
    }

    private void OnDestroy() {
        // Cleanup button listener
        if (closeButton != null) {
            closeButton.onClick.RemoveListener(HideRestockUI);
        }

        // Ensure player movement is re-enabled if UI is destroyed
        EnablePlayerMovement();
    }
}
