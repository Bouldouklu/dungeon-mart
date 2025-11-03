using UnityEngine;

/// <summary>
/// ScriptableObject defining an item's properties, category, and unlock requirements.
/// Items are filtered by category for shelf placement and availability in the order menu.
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "DungeonMart/Item Data")]
public class ItemDataSO : ScriptableObject {
    [Header("Basic Properties")]
    public string itemName;
    public Sprite itemSprite;
    public GameObject itemPrefab;
    public int sellPrice = 10;
    public int restockCost = 5;

    [Header("Category & Unlock")]
    [Tooltip("The category this item belongs to (determines which shelves can hold it)")]
    public ItemCategory itemCategory = ItemCategory.Weapons;

    [Tooltip("Item quality tier (1 = cheap/starting, 2 = normal/mid-game, 3 = premium/late-game). Auto-assigned based on sell price.")]
    [Range(1, 3)]
    public int tier = 1;

    [Tooltip("If true, this item is available from the beginning regardless of category unlock status")]
    public bool isUnlockedByDefault = false;

    [Header("Flavor & Description")]
    [Tooltip("Item description/lore shown in UI")]
    [TextArea(2, 3)]
    public string description = "";
}