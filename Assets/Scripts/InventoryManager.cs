using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private Dictionary<ItemDataSO, int> inventory = new Dictionary<ItemDataSO, int>();

    public event System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddToInventory(ItemDataSO itemData, int quantity)
    {
        if (itemData == null || quantity <= 0) return;

        if (inventory.ContainsKey(itemData))
        {
            inventory[itemData] += quantity;
        }
        else
        {
            inventory[itemData] = quantity;
        }

        Debug.Log($"Added {quantity}x {itemData.itemName} to inventory. Total: {inventory[itemData]}");
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveFromInventory(ItemDataSO itemData, int quantity)
    {
        if (itemData == null || quantity <= 0) return false;

        if (!inventory.ContainsKey(itemData) || inventory[itemData] < quantity)
        {
            Debug.Log($"Not enough {itemData.itemName} in inventory!");
            return false;
        }

        inventory[itemData] -= quantity;

        if (inventory[itemData] == 0)
        {
            inventory.Remove(itemData);
        }

        Debug.Log($"Removed {quantity}x {itemData.itemName} from inventory");
        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetItemCount(ItemDataSO itemData)
    {
        if (itemData == null) return 0;
        return inventory.ContainsKey(itemData) ? inventory[itemData] : 0;
    }

    public bool HasItem(ItemDataSO itemData, int quantity = 1)
    {
        return GetItemCount(itemData) >= quantity;
    }

    public Dictionary<ItemDataSO, int> GetAllInventory()
    {
        return new Dictionary<ItemDataSO, int>(inventory);
    }

    [SerializeField] private List<ItemDataSO> debugItemsToAdd = new List<ItemDataSO>();
    [SerializeField] private int debugQuantityPerItem = 10;

    // Debug method to add starting inventory for testing
    [ContextMenu("Add Debug Inventory")]
    public void AddDebugInventory()
    {
        if (debugItemsToAdd == null || debugItemsToAdd.Count == 0)
        {
            Debug.LogWarning("No debug items configured! Add ItemDataSO references to InventoryManager's debugItemsToAdd list in the Inspector.");
            return;
        }

        foreach (ItemDataSO item in debugItemsToAdd)
        {
            if (item != null)
            {
                AddToInventory(item, debugQuantityPerItem);
            }
        }

        Debug.Log($"Added debug inventory: {debugItemsToAdd.Count} item types x{debugQuantityPerItem} each");
    }
}
