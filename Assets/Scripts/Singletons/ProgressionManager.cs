using System.Linq;
using UnityEngine;

/// <summary>
/// Manages player progression through business tiers based on lifetime revenue.
/// Tracks current tier, evaluates tier-ups, and fires events for UI updates.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;

    [Header("Progression Configuration")]
    [SerializeField] private ProgressionDataSO[] progressionTiers;

    [Header("Current State")]
    [SerializeField] private int currentTierIndex = 0;
    [SerializeField] private int lifetimeRevenue = 0;

    /// <summary>
    /// Event triggered when player reaches a new tier.
    /// Parameters: new tier data, new tier index
    /// </summary>
    public event System.Action<ProgressionDataSO, int> OnTierReached;

    /// <summary>
    /// Event triggered when lifetime revenue changes (for UI updates).
    /// Parameters: new lifetime revenue
    /// </summary>
    public event System.Action<int> OnLifetimeRevenueChanged;

    // Properties
    public int CurrentTierIndex => currentTierIndex;
    public int LifetimeRevenue => lifetimeRevenue;
    public ProgressionDataSO CurrentTier => GetCurrentTier();
    public ProgressionDataSO NextTier => GetNextTier();
    public bool IsMaxTier => currentTierIndex >= progressionTiers.Length - 1;

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
        // Validate progression tiers are assigned
        if (progressionTiers == null || progressionTiers.Length == 0)
        {
            Debug.LogError("ProgressionManager: No progression tiers assigned! Please assign ProgressionDataSO assets in the inspector.");
            return;
        }

        // Sort tiers by required revenue (ascending)
        progressionTiers = progressionTiers.OrderBy(tier => tier.requiredLifetimeRevenue).ToArray();

        // Subscribe to GameManager money events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyAdded += OnMoneyAdded;
        }

        Debug.Log($"ProgressionManager initialized. Current tier: {CurrentTier?.tierName ?? "None"}");
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyAdded -= OnMoneyAdded;
        }
    }

    /// <summary>
    /// Called when player earns money - updates lifetime revenue and checks for tier-ups.
    /// </summary>
    private void OnMoneyAdded(int amountAdded, int newTotal)
    {
        lifetimeRevenue += amountAdded;
        OnLifetimeRevenueChanged?.Invoke(lifetimeRevenue);

        // Check if we've reached a new tier
        CheckForTierUp();
    }

    /// <summary>
    /// Checks if player has met requirements for next tier and triggers tier-up event.
    /// </summary>
    private void CheckForTierUp()
    {
        if (IsMaxTier) return;

        ProgressionDataSO nextTier = GetNextTier();
        if (nextTier != null && lifetimeRevenue >= nextTier.requiredLifetimeRevenue)
        {
            currentTierIndex++;
            Debug.Log($"🎉 TIER UP! Reached {nextTier.tierName} (Tier {currentTierIndex})");
            OnTierReached?.Invoke(nextTier, currentTierIndex);

            // Check if we immediately qualify for another tier (edge case)
            CheckForTierUp();
        }
    }

    /// <summary>
    /// Gets the current tier data.
    /// </summary>
    private ProgressionDataSO GetCurrentTier()
    {
        if (progressionTiers == null || progressionTiers.Length == 0) return null;
        if (currentTierIndex < 0 || currentTierIndex >= progressionTiers.Length) return null;
        return progressionTiers[currentTierIndex];
    }

    /// <summary>
    /// Gets the next tier data (null if at max tier).
    /// </summary>
    private ProgressionDataSO GetNextTier()
    {
        if (IsMaxTier) return null;
        int nextIndex = currentTierIndex + 1;
        if (nextIndex >= progressionTiers.Length) return null;
        return progressionTiers[nextIndex];
    }

    /// <summary>
    /// Gets progress percentage to next tier (0-100).
    /// Returns 100 if at max tier.
    /// </summary>
    public float GetProgressToNextTier()
    {
        if (IsMaxTier) return 100f;

        ProgressionDataSO currentTier = GetCurrentTier();
        ProgressionDataSO nextTier = GetNextTier();

        if (currentTier == null || nextTier == null) return 0f;

        int currentRequirement = currentTier.requiredLifetimeRevenue;
        int nextRequirement = nextTier.requiredLifetimeRevenue;
        int revenueInTier = lifetimeRevenue - currentRequirement;
        int revenueNeededForTier = nextRequirement - currentRequirement;

        if (revenueNeededForTier <= 0) return 100f;

        return Mathf.Clamp01((float)revenueInTier / revenueNeededForTier) * 100f;
    }

    /// <summary>
    /// Gets revenue needed to reach next tier.
    /// Returns 0 if at max tier.
    /// </summary>
    public int GetRevenueToNextTier()
    {
        if (IsMaxTier) return 0;

        ProgressionDataSO nextTier = GetNextTier();
        if (nextTier == null) return 0;

        int remaining = nextTier.requiredLifetimeRevenue - lifetimeRevenue;
        return Mathf.Max(0, remaining);
    }

    /// <summary>
    /// Manually set lifetime revenue (for debugging/testing).
    /// </summary>
    public void SetLifetimeRevenue(int amount)
    {
        lifetimeRevenue = amount;
        OnLifetimeRevenueChanged?.Invoke(lifetimeRevenue);
        CheckForTierUp();
        Debug.Log($"Lifetime revenue set to: ${lifetimeRevenue}");
    }

    /// <summary>
    /// Get all unlocked upgrades for current tier.
    /// </summary>
    public UpgradeDataSO[] GetCurrentTierUpgrades()
    {
        ProgressionDataSO currentTier = GetCurrentTier();
        return currentTier?.unlockedUpgrades ?? new UpgradeDataSO[0];
    }
}
