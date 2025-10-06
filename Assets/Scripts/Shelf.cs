using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    [SerializeField] private ItemDataSO itemToStock;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int maxCapacity = 5;

    private List<Item> currentStock = new List<Item>();

    public bool IsEmpty => currentStock.Count == 0;
    public bool IsFull => currentStock.Count >= maxCapacity;

    public bool RestockShelf(int quantity = 1)
    {
        if (itemToStock == null || itemPrefab == null) return false;
        if (IsFull) return false;

        int itemsToAdd = Mathf.Min(quantity, maxCapacity - currentStock.Count);
        int totalCost = itemsToAdd * itemToStock.restockCost;

        if (!GameManager.Instance.SpendMoney(totalCost))
        {
            return false;
        }

        for (int i = 0; i < itemsToAdd; i++)
        {
            GameObject itemObj = Instantiate(itemPrefab, transform);
            Item item = itemObj.GetComponent<Item>();

            if (item != null)
            {
                item.Initialize(itemToStock);
                currentStock.Add(item);
            }
        }

        Debug.Log($"Restocked {itemsToAdd}x {itemToStock.itemName}");
        return true;
    }

    public Item TakeItem()
    {
        if (IsEmpty) return null;

        Item item = currentStock[currentStock.Count - 1];
        currentStock.RemoveAt(currentStock.Count - 1);
        return item;
    }
}
