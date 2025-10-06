using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private ItemDataSO itemData;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Initialize(ItemDataSO data)
    {
        itemData = data;
        if (itemData != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = itemData.itemSprite;
        }
    }

    public int GetSellPrice()
    {
        return itemData != null ? itemData.sellPrice : 0;
    }

    public string GetItemName()
    {
        return itemData != null ? itemData.itemName : "Unknown";
    }
}
