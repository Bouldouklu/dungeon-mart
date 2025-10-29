using UnityEngine;

/// <summary>
/// Categories for organizing upgrades in the shop UI.
/// </summary>
public enum UpgradeCategory
{
    ShopExpansion,   // Unlock new shop segments
    ShelfCapacity,   // Increase items per shelf slot
    Operations,      // Efficiency improvements (bulk ordering, auto-restock)
    CustomerFlow,    // More/faster customers
    Licenses         // Item category unlock permits (weapons, armor, traps, etc.)
}

/// <summary>
/// Types of effects upgrades can have on game systems.
/// </summary>
public enum UpgradeEffectType
{
    UnlockShopSegment,     // Enable a locked shop area
    IncreaseShelfCapacity, // Add items per slot capacity
    IncreaseCustomerCount, // Add customers per day
    DecreaseCheckoutTime,  // Speed up checkout process
    EnableBulkOrdering,    // Allow 5x orders with discount
    EnableAutoRestock,     // Auto-fill shelves in morning
    UnlockItemCategory     // Unlock a new item category for ordering
}

/// <summary>
/// Defines a shop upgrade that players can purchase to improve their business.
/// Supports shop expansion, efficiency improvements, and capacity increases.
/// </summary>
[CreateAssetMenu(fileName = "New Upgrade", menuName = "DungeonMart/Upgrade")]
public class UpgradeDataSO : ScriptableObject
{
    [Header("Upgrade Identity")]
    [Tooltip("Display name shown in UI")]
    public string upgradeName;

    [Tooltip("Unique ID for checking ownership (auto-generated from name)")]
    public string upgradeID;

    [TextArea(2, 4)]
    [Tooltip("Description shown in upgrade card")]
    public string description;

    [Tooltip("Icon displayed in upgrade card")]
    public Sprite upgradeIcon;

    [Header("Cost & Requirements")]
    [Tooltip("Gold cost to purchase")]
    public int cost;

    [Tooltip("Objective that must be completed to unlock this upgrade")]
    public ObjectiveDataSO requiredObjective;

    [Tooltip("Other upgrades that must be owned before this can be purchased")]
    public UpgradeDataSO[] prerequisites;

    [Header("Upgrade Classification")]
    [Tooltip("Category for UI filtering")]
    public UpgradeCategory category;

    [Tooltip("Type of effect this upgrade provides")]
    public UpgradeEffectType effectType;

    [Tooltip("Numeric value for the effect (e.g., +2 capacity, +50% speed)")]
    public int effectValue;

    [Header("Shop Expansion Settings")]
    [Tooltip("For UnlockShopSegment: which segment index to unlock")]
    public int targetSegmentIndex = -1;

    [Header("Item Category Settings")]
    [Tooltip("For UnlockItemCategory: which item category to unlock")]
    public ItemCategory categoryToUnlock = ItemCategory.Weapons;

    [Header("Repeatability")]
    [Tooltip("Can this upgrade be purchased multiple times?")]
    public bool isRepeatable = false;

    [Tooltip("Maximum number of purchases (only if repeatable)")]
    public int maxPurchases = 1;

    /// <summary>
    /// Validates upgrade data on asset save.
    /// </summary>
    private void OnValidate()
    {
        // Auto-generate upgrade ID from name
        if (string.IsNullOrEmpty(upgradeID) || upgradeID != upgradeName)
        {
            upgradeID = upgradeName;
        }

        // Validate max purchases
        if (!isRepeatable)
        {
            maxPurchases = 1;
        }
        else if (maxPurchases < 1)
        {
            maxPurchases = 1;
        }
    }

    /// <summary>
    /// Gets a formatted display string for the upgrade effect.
    /// </summary>
    public string GetEffectDescription()
    {
        return effectType switch
        {
            UpgradeEffectType.UnlockShopSegment => $"Unlock Shop Segment {targetSegmentIndex}",
            UpgradeEffectType.IncreaseShelfCapacity => $"+{effectValue} items per shelf slot",
            UpgradeEffectType.IncreaseCustomerCount => $"+{effectValue} customers per day",
            UpgradeEffectType.DecreaseCheckoutTime => $"{effectValue}% faster checkout",
            UpgradeEffectType.EnableBulkOrdering => "Order 5x items with 10% discount",
            UpgradeEffectType.EnableAutoRestock => "Auto-fill shelves each morning",
            UpgradeEffectType.UnlockItemCategory => $"Unlock {categoryToUnlock} items",
            _ => "Unknown effect"
        };
    }
}
