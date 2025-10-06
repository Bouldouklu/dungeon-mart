using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;

    [SerializeField] private List<ItemDataSO> availableItems = new List<ItemDataSO>();

    private Dictionary<ItemDataSO, int> currentOrder = new Dictionary<ItemDataSO, int>();

    public event System.Action OnOrderChanged;

    public List<ItemDataSO> AvailableItems => availableItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddToOrder(ItemDataSO item, int quantity)
    {
        if (item == null || quantity <= 0) return;

        if (currentOrder.ContainsKey(item))
        {
            currentOrder[item] += quantity;
        }
        else
        {
            currentOrder[item] = quantity;
        }

        Debug.Log($"Added {quantity}x {item.itemName} to order");
        OnOrderChanged?.Invoke();
    }

    public void RemoveFromOrder(ItemDataSO item, int quantity)
    {
        if (item == null || !currentOrder.ContainsKey(item)) return;

        currentOrder[item] -= quantity;

        if (currentOrder[item] <= 0)
        {
            currentOrder.Remove(item);
        }

        OnOrderChanged?.Invoke();
    }

    public void ClearOrder()
    {
        currentOrder.Clear();
        OnOrderChanged?.Invoke();
    }

    public int GetOrderQuantity(ItemDataSO item)
    {
        return currentOrder.ContainsKey(item) ? currentOrder[item] : 0;
    }

    public int GetTotalOrderCost()
    {
        int total = 0;
        foreach (var kvp in currentOrder)
        {
            total += kvp.Key.restockCost * kvp.Value;
        }
        return total;
    }

    public bool ConfirmOrder()
    {
        int totalCost = GetTotalOrderCost();

        if (totalCost == 0)
        {
            Debug.Log("Order is empty!");
            return false;
        }

        if (!GameManager.Instance.SpendMoney(totalCost))
        {
            Debug.Log("Not enough money to complete order!");
            return false;
        }

        // Add items to inventory immediately (will be delivered in morning phase later)
        foreach (var kvp in currentOrder)
        {
            InventoryManager.Instance.AddToInventory(kvp.Key, kvp.Value);
        }

        Debug.Log($"Order confirmed! Total cost: ${totalCost}");
        ClearOrder();
        return true;
    }

    public Dictionary<ItemDataSO, int> GetCurrentOrder()
    {
        return new Dictionary<ItemDataSO, int>(currentOrder);
    }
}
