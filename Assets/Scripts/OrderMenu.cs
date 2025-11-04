using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OrderMenu : MonoBehaviour {
    public static OrderMenu Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject menuPanel;

    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject orderMenuItemPrefab;
    [SerializeField] private TextMeshProUGUI totalCostText;
    [SerializeField] private TextMeshProUGUI currentMoneyText;
    [SerializeField] private TextMeshProUGUI cartDetailsText;
    [SerializeField] private Button confirmOrderButton;
    [SerializeField] private Button clearCartButton;
    [SerializeField] private Button closeButton;

    private List<OrderMenuItem> menuItems = new List<OrderMenuItem>();
    private bool isMenuOpen = false;

    private void Awake() {
        // Singleton pattern
        if (Instance == null) {
            Instance = this;
        }
        else {
            Debug.LogWarning("Multiple OrderMenu instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start() {
        if (menuPanel != null) {
            menuPanel.SetActive(false);
        }

        if (confirmOrderButton != null) {
            confirmOrderButton.onClick.AddListener(OnConfirmOrder);
        }

        if (closeButton != null) {
            closeButton.onClick.AddListener(CloseMenu);
        }

        if (clearCartButton != null) {
            clearCartButton.onClick.AddListener(OnClearCart);
        }

        SupplyChainManager.Instance.OnOrderChanged += UpdateOrderDisplay;
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }

        if (SupplyChainManager.Instance != null) {
            SupplyChainManager.Instance.OnOrderChanged -= UpdateOrderDisplay;
        }
    }

    private bool CanOpenMenu() {
        if (DayManager.Instance == null) return true;

        GamePhase currentPhase = DayManager.Instance.CurrentPhase;

        // Only allow ordering during End of Day phase
        return currentPhase == GamePhase.EndOfDay;
    }

    private void ShowPhaseRestrictionMessage() {
        if (DayManager.Instance == null) return;

        GamePhase phase = DayManager.Instance.CurrentPhase;

        switch (phase) {
            case GamePhase.Morning:
                Debug.Log("Cannot order during morning. Open delivery boxes and prepare for the day!");
                break;
            case GamePhase.OpenForBusiness:
                Debug.Log("Cannot order while shop is open. Wait until end of day!");
                break;
        }
    }

    public void ToggleMenu() {
        if (isMenuOpen) {
            CloseMenu();
        }
        else {
            if (!CanOpenMenu()) {
                ShowPhaseRestrictionMessage();
                return;
            }

            OpenMenu();
        }
    }

    public void OpenMenu() {
        Debug.Log("Opening order menu");
        isMenuOpen = true;
        if (menuPanel != null) {
            menuPanel.SetActive(true);
            Debug.Log("Menu panel activated");
        }
        else {
            Debug.LogError("Menu panel is null!");
        }

        PopulateItemList();
        UpdateOrderDisplay();

        // Don't pause during End of Day phase - it's already a natural pause in gameplay
        bool isEndOfDay = DayManager.Instance != null && DayManager.Instance.CurrentPhase == GamePhase.EndOfDay;

        if (!isEndOfDay && PauseManager.Instance != null) {
            PauseManager.Instance.PauseGame();
        }
    }

    public void CloseMenu() {
        isMenuOpen = false;
        if (menuPanel != null) {
            menuPanel.SetActive(false);
        }

        // Play UI click sound
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySound(SoundType.UIClick);
        }

        // Only resume if we actually paused (not during End of Day phase)
        bool isEndOfDay = DayManager.Instance != null && DayManager.Instance.CurrentPhase == GamePhase.EndOfDay;

        if (!isEndOfDay && PauseManager.Instance != null) {
            PauseManager.Instance.ResumeGame();
        }
    }

    private void PopulateItemList() {
        // Clear existing items
        foreach (var item in menuItems) {
            if (item != null) {
                Destroy(item.gameObject);
            }
        }

        menuItems.Clear();

        // Create new items
        foreach (ItemDataSO itemData in SupplyChainManager.Instance.AvailableItems) {
            if (itemData == null || orderMenuItemPrefab == null || itemListContainer == null) continue;

            GameObject itemObj = Instantiate(orderMenuItemPrefab, itemListContainer);
            OrderMenuItem menuItem = itemObj.GetComponent<OrderMenuItem>();

            if (menuItem != null) {
                menuItem.Initialize(itemData);
                menuItems.Add(menuItem);
            }
        }
    }

    private void UpdateOrderDisplay() {
        int totalCost = SupplyChainManager.Instance.GetTotalOrderCost();
        int currentMoney = GameManager.Instance.CurrentMoney;

        if (totalCostText != null) {
            totalCostText.text = $"Total: ${totalCost}";
        }

        if (currentMoneyText != null) {
            currentMoneyText.text = $"Money: ${currentMoney}";
        }

        // Update cart details
        if (cartDetailsText != null) {
            var regularCart = SupplyChainManager.Instance.GetCurrentOrder();
            var bulkCart = SupplyChainManager.Instance.GetBulkOrder();

            if (regularCart.Count == 0 && bulkCart.Count == 0) {
                cartDetailsText.text = "Cart is empty";
            }
            else {
                string cartText = "Cart:\n";

                // Show regular items
                foreach (var kvp in regularCart) {
                    int itemCost = kvp.Key.restockCost * kvp.Value;
                    cartText += $"• {kvp.Value}x {kvp.Key.itemName} (${itemCost})\n";
                }

                // Show bulk items with discount indicator
                foreach (var kvp in bulkCart) {
                    int regularCost = kvp.Key.restockCost * kvp.Value;
                    int discountedCost = Mathf.RoundToInt(regularCost * 0.9f);
                    cartText += $"• {kvp.Value}x {kvp.Key.itemName} (${discountedCost}) <color=green>-10%</color>\n";
                }

                cartDetailsText.text = cartText;
            }
        }

        if (confirmOrderButton != null) {
            confirmOrderButton.interactable = totalCost > 0 && currentMoney >= totalCost;
        }

        if (clearCartButton != null) {
            clearCartButton.interactable = totalCost > 0;
        }
    }

    private void OnConfirmOrder() {
        bool success = SupplyChainManager.Instance.ConfirmOrder();
        if (success) {
            UpdateOrderDisplay();
            Debug.Log("Order placed successfully!");

            // Play UI confirm sound
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySound(SoundType.UIConfirm);
            }
        } else {
            // Play error sound if order failed
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySound(SoundType.UIError);
            }
        }
    }

    private void OnClearCart() {
        SupplyChainManager.Instance.ClearOrder();
        UpdateOrderDisplay();
        Debug.Log("Cart cleared");

        // Play UI click sound
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySound(SoundType.UIClick);
        }
    }
}