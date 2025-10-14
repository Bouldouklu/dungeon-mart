using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages shop upgrades: purchasing, ownership tracking, and effect application.
/// Central hub for all upgrade-related logic and progression.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Available Upgrades")]
    [Tooltip("All upgrades that can be purchased (assign in inspector or load from Resources)")]
    [SerializeField] private UpgradeDataSO[] availableUpgrades;

    [Header("Owned Upgrades")]
    [Tooltip("Upgrades the player currently owns")]
    [SerializeField] private List<UpgradeDataSO> ownedUpgrades = new List<UpgradeDataSO>();

    [Tooltip("Purchase count for repeatable upgrades (upgradeID -> count)")]
    private Dictionary<string, int> purchaseCount = new Dictionary<string, int>();

    /// <summary>
    /// Event triggered when an upgrade is purchased.
    /// Parameters: upgrade data, purchase count (for repeatables)
    /// </summary>
    public event System.Action<UpgradeDataSO, int> OnUpgradePurchased;

    // Properties
    public int OwnedUpgradeCount => ownedUpgrades.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Load available upgrades from Resources if not assigned
        if (availableUpgrades == null || availableUpgrades.Length == 0)
        {
            availableUpgrades = Resources.LoadAll<UpgradeDataSO>("Upgrades");
            Debug.Log($"UpgradeManager: Loaded {availableUpgrades.Length} upgrades from Resources/Upgrades");
        }

        Debug.Log($"UpgradeManager initialized: {availableUpgrades.Length} available upgrades");
    }

    /// <summary>
    /// Checks if player can purchase an upgrade (all requirements met).
    /// </summary>
    public bool CanPurchaseUpgrade(UpgradeDataSO upgrade, out string reason)
    {
        reason = "";

        if (upgrade == null)
        {
            reason = "Invalid upgrade";
            return false;
        }

        // Check if already owned (non-repeatable)
        if (!upgrade.isRepeatable && HasUpgrade(upgrade.upgradeID))
        {
            reason = "Already owned";
            return false;
        }

        // Check purchase limit (repeatable)
        if (upgrade.isRepeatable)
        {
            int currentPurchases = GetPurchaseCount(upgrade.upgradeID);
            if (currentPurchases >= upgrade.maxPurchases)
            {
                reason = $"Max purchases reached ({upgrade.maxPurchases})";
                return false;
            }
        }

        // Check tier requirement
        if (ProgressionManager.Instance != null)
        {
            int currentTier = ProgressionManager.Instance.CurrentTierIndex;
            if (currentTier < upgrade.tierRequirement)
            {
                reason = $"Requires Tier {upgrade.tierRequirement}";
                return false;
            }
        }

        // Check prerequisites
        if (upgrade.prerequisites != null && upgrade.prerequisites.Length > 0)
        {
            foreach (UpgradeDataSO prereq in upgrade.prerequisites)
            {
                if (prereq != null && !HasUpgrade(prereq.upgradeID))
                {
                    reason = $"Requires: {prereq.upgradeName}";
                    return false;
                }
            }
        }

        // Check money
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentMoney < upgrade.cost)
            {
                reason = $"Insufficient funds (need ${upgrade.cost})";
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Purchases an upgrade if all requirements are met.
    /// </summary>
    public bool PurchaseUpgrade(UpgradeDataSO upgrade)
    {
        if (!CanPurchaseUpgrade(upgrade, out string reason))
        {
            Debug.LogWarning($"Cannot purchase {upgrade.upgradeName}: {reason}");
            return false;
        }

        // Deduct money
        if (!GameManager.Instance.SpendMoney(upgrade.cost))
        {
            Debug.LogError($"Failed to spend money for {upgrade.upgradeName}");
            return false;
        }

        // Add to owned upgrades (for non-repeatable or first purchase)
        if (!ownedUpgrades.Contains(upgrade))
        {
            ownedUpgrades.Add(upgrade);
        }

        // Track purchase count
        string upgradeID = upgrade.upgradeID;
        if (!purchaseCount.ContainsKey(upgradeID))
        {
            purchaseCount[upgradeID] = 0;
        }
        purchaseCount[upgradeID]++;

        int currentPurchases = purchaseCount[upgradeID];
        Debug.Log($"✅ Purchased: {upgrade.upgradeName} (Purchase #{currentPurchases})");

        // Apply upgrade effect
        ApplyUpgradeEffect(upgrade);

        // Fire event
        OnUpgradePurchased?.Invoke(upgrade, currentPurchases);

        // Play purchase sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.Purchase);
        }

        return true;
    }

    /// <summary>
    /// Applies the effect of an upgrade to the relevant game systems.
    /// </summary>
    private void ApplyUpgradeEffect(UpgradeDataSO upgrade)
    {
        switch (upgrade.effectType)
        {
            case UpgradeEffectType.UnlockShopSegment:
                if (ShopSegmentManager.Instance != null)
                {
                    ShopSegmentManager.Instance.UnlockSegment(upgrade.targetSegmentIndex);
                }
                else
                {
                    Debug.LogError("ShopSegmentManager not found!");
                }
                break;

            case UpgradeEffectType.IncreaseShelfCapacity:
                IncreaseAllShelfCapacity(upgrade.effectValue);
                break;

            case UpgradeEffectType.IncreaseCustomerCount:
                if (CustomerSpawner.Instance != null)
                {
                    CustomerSpawner.Instance.AddBonusCustomers(upgrade.effectValue);
                }
                else
                {
                    Debug.LogError("CustomerSpawner not found!");
                }
                break;

            case UpgradeEffectType.DecreaseCheckoutTime:
                Debug.Log($"TODO: Implement checkout speed increase ({upgrade.effectValue}%)");
                // Will be implemented when CheckoutCounter modifications are added
                break;

            case UpgradeEffectType.EnableBulkOrdering:
                Debug.Log("TODO: Implement bulk ordering feature");
                // Will be implemented when OrderManager modifications are added
                break;

            case UpgradeEffectType.EnableAutoRestock:
                Debug.Log("TODO: Implement auto-restock feature");
                // Will be implemented when DeliveryManager modifications are added
                break;

            default:
                Debug.LogWarning($"Unknown upgrade effect type: {upgrade.effectType}");
                break;
        }
    }

    /// <summary>
    /// Increases capacity for all shelves in the scene.
    /// </summary>
    private void IncreaseAllShelfCapacity(int amount)
    {
        Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
        foreach (Shelf shelf in shelves)
        {
            shelf.IncreaseCapacity(amount);
        }
        Debug.Log($"Increased capacity for {shelves.Length} shelves by {amount}");
    }

    /// <summary>
    /// Checks if player owns a specific upgrade by ID.
    /// </summary>
    public bool HasUpgrade(string upgradeID)
    {
        return ownedUpgrades.Any(u => u.upgradeID == upgradeID);
    }

    /// <summary>
    /// Gets the number of times a repeatable upgrade has been purchased.
    /// </summary>
    public int GetPurchaseCount(string upgradeID)
    {
        return purchaseCount.ContainsKey(upgradeID) ? purchaseCount[upgradeID] : 0;
    }

    /// <summary>
    /// Gets all upgrades available at the current tier (including owned).
    /// </summary>
    public List<UpgradeDataSO> GetAvailableUpgrades()
    {
        if (ProgressionManager.Instance == null) return new List<UpgradeDataSO>();

        int currentTier = ProgressionManager.Instance.CurrentTierIndex;

        return availableUpgrades
            .Where(u => u != null && u.tierRequirement <= currentTier)
            .ToList();
    }

    /// <summary>
    /// Gets upgrades filtered by category.
    /// </summary>
    public List<UpgradeDataSO> GetUpgradesByCategory(UpgradeCategory category)
    {
        return GetAvailableUpgrades()
            .Where(u => u.category == category)
            .ToList();
    }

    /// <summary>
    /// Gets the display state for an upgrade (Locked, Available, Owned, Maxed).
    /// </summary>
    public string GetUpgradeState(UpgradeDataSO upgrade)
    {
        if (upgrade == null) return "Invalid";

        // Check if maxed out
        if (upgrade.isRepeatable)
        {
            int purchases = GetPurchaseCount(upgrade.upgradeID);
            if (purchases >= upgrade.maxPurchases)
            {
                return "Maxed";
            }
        }

        // Check if owned (non-repeatable)
        if (!upgrade.isRepeatable && HasUpgrade(upgrade.upgradeID))
        {
            return "Owned";
        }

        // Check if can purchase
        if (CanPurchaseUpgrade(upgrade, out string reason))
        {
            return "Available";
        }

        // Otherwise locked
        return "Locked";
    }
}
