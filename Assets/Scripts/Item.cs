using UnityEngine;

/// <summary>
/// Data container for items in the game world.
/// Visual representation is handled by the prefab structure (3D models, materials, etc.)
/// </summary>
public class Item : MonoBehaviour {
    [SerializeField] private ItemDataSO itemData;

    /// <summary>
    /// Initialize the item with its data.
    /// Visual setup is handled by the prefab itself.
    /// </summary>
    public void Initialize(ItemDataSO data) {
        itemData = data;
    }

    public int GetSellPrice() {
        return itemData != null ? itemData.sellPrice : 0;
    }

    public string GetItemName() {
        return itemData != null ? itemData.itemName : "Unknown";
    }

    public ItemDataSO GetItemData() {
        return itemData;
    }
}