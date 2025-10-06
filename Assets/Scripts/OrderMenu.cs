using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OrderMenu : MonoBehaviour
{
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

    private void Start()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        if (confirmOrderButton != null)
        {
            confirmOrderButton.onClick.AddListener(OnConfirmOrder);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMenu);
        }

        if (clearCartButton != null)
        {
            clearCartButton.onClick.AddListener(OnClearCart);
        }

        OrderManager.Instance.OnOrderChanged += UpdateOrderDisplay;
    }

    private void OnDestroy()
    {
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderChanged -= UpdateOrderDisplay;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Tab key pressed - toggling menu");
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (isMenuOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        Debug.Log("Opening order menu");
        isMenuOpen = true;
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            Debug.Log("Menu panel activated");
        }
        else
        {
            Debug.LogError("Menu panel is null!");
        }

        PopulateItemList();
        UpdateOrderDisplay();
        Time.timeScale = 0f; // Pause game
    }

    public void CloseMenu()
    {
        isMenuOpen = false;
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        Time.timeScale = 1f; // Resume game
    }

    private void PopulateItemList()
    {
        // Clear existing items
        foreach (var item in menuItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        menuItems.Clear();

        // Create new items
        foreach (ItemDataSO itemData in OrderManager.Instance.AvailableItems)
        {
            if (itemData == null || orderMenuItemPrefab == null || itemListContainer == null) continue;

            GameObject itemObj = Instantiate(orderMenuItemPrefab, itemListContainer);
            OrderMenuItem menuItem = itemObj.GetComponent<OrderMenuItem>();

            if (menuItem != null)
            {
                menuItem.Initialize(itemData);
                menuItems.Add(menuItem);
            }
        }
    }

    private void UpdateOrderDisplay()
    {
        int totalCost = OrderManager.Instance.GetTotalOrderCost();
        int currentMoney = GameManager.Instance.CurrentMoney;

        if (totalCostText != null)
        {
            totalCostText.text = $"Total: ${totalCost}";
        }

        if (currentMoneyText != null)
        {
            currentMoneyText.text = $"Money: ${currentMoney}";
        }

        // Update cart details
        if (cartDetailsText != null)
        {
            var cart = OrderManager.Instance.GetCurrentOrder();
            if (cart.Count == 0)
            {
                cartDetailsText.text = "Cart is empty";
            }
            else
            {
                string cartText = "Cart:\n";
                foreach (var kvp in cart)
                {
                    cartText += $"• {kvp.Value}x {kvp.Key.itemName} (${kvp.Key.restockCost * kvp.Value})\n";
                }
                cartDetailsText.text = cartText;
            }
        }

        if (confirmOrderButton != null)
        {
            confirmOrderButton.interactable = totalCost > 0 && currentMoney >= totalCost;
        }

        if (clearCartButton != null)
        {
            clearCartButton.interactable = totalCost > 0;
        }
    }

    private void OnConfirmOrder()
    {
        bool success = OrderManager.Instance.ConfirmOrder();
        if (success)
        {
            UpdateOrderDisplay();
            Debug.Log("Order placed successfully!");
        }
    }

    private void OnClearCart()
    {
        OrderManager.Instance.ClearOrder();
        UpdateOrderDisplay();
        Debug.Log("Cart cleared");
    }
}
