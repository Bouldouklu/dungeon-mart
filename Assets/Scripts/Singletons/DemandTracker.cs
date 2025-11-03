using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Statistics for a single item's demand.
/// Tracks how many customers wanted it vs how many were actually sold.
/// </summary>
[System.Serializable]
public class DemandStats
{
    public int wanted;
    public int sold;
    public int Missed => wanted - sold;

    public DemandStats()
    {
        wanted = 0;
        sold = 0;
    }
}

/// <summary>
/// Tracks customer demand for items throughout the day.
/// Records what items customers wanted vs what they actually purchased.
/// Provides statistics for end-of-day reporting and trend analysis.
/// </summary>
public class DemandTracker : MonoBehaviour
{
    public static DemandTracker Instance;

    [Header("Daily Demand Statistics (Read-Only)")]
    [SerializeField] private List<DemandEntry> currentDayDemandDisplay = new List<DemandEntry>();

    private Dictionary<ItemDataSO, DemandStats> dailyDemand = new Dictionary<ItemDataSO, DemandStats>();

    /// <summary>
    /// Event fired when demand statistics are updated.
    /// </summary>
    public event System.Action OnDemandStatsChanged;

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
        // Subscribe to DayManager events to reset daily stats
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }

        // Subscribe to CheckoutCounter to track sold items
        if (CheckoutCounter.Instance != null)
        {
            CheckoutCounter.Instance.OnItemSold += OnItemSold;
        }
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }

        if (CheckoutCounter.Instance != null)
        {
            CheckoutCounter.Instance.OnItemSold -= OnItemSold;
        }
    }

    /// <summary>
    /// Called when game phase changes - resets stats at morning phase.
    /// </summary>
    private void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Morning)
        {
            ResetDailyStats();
        }
    }

    /// <summary>
    /// Records that a customer wanted a specific item.
    /// Called when customer generates specific item request.
    /// </summary>
    public void RecordItemWanted(ItemDataSO item)
    {
        if (item == null) return;

        if (!dailyDemand.ContainsKey(item))
        {
            dailyDemand[item] = new DemandStats();
        }

        dailyDemand[item].wanted++;
        UpdateDemandDisplay();
        OnDemandStatsChanged?.Invoke();
    }

    /// <summary>
    /// Records that an item was sold to a customer.
    /// Called from CheckoutCounter when transaction completes.
    /// </summary>
    private void OnItemSold(ItemDataSO item, int quantity)
    {
        if (item == null) return;

        if (!dailyDemand.ContainsKey(item))
        {
            dailyDemand[item] = new DemandStats();
        }

        dailyDemand[item].sold += quantity;
        UpdateDemandDisplay();
        OnDemandStatsChanged?.Invoke();
    }

    /// <summary>
    /// Gets the demand statistics for a specific item.
    /// Returns null if no demand data exists for this item.
    /// </summary>
    public DemandStats GetDemandStats(ItemDataSO item)
    {
        if (item == null || !dailyDemand.ContainsKey(item))
        {
            return null;
        }

        return dailyDemand[item];
    }

    /// <summary>
    /// Gets all demand statistics for the current day.
    /// Returns a copy of the demand dictionary.
    /// </summary>
    public Dictionary<ItemDataSO, DemandStats> GetAllDemandStats()
    {
        return new Dictionary<ItemDataSO, DemandStats>(dailyDemand);
    }

    /// <summary>
    /// Gets demand statistics only for trending items.
    /// Useful for end-of-day reporting.
    /// </summary>
    public Dictionary<ItemDataSO, DemandStats> GetTrendingItemDemand()
    {
        Dictionary<ItemDataSO, DemandStats> trendingDemand = new Dictionary<ItemDataSO, DemandStats>();

        if (TrendManager.Instance == null) return trendingDemand;

        List<ItemDataSO> trendingItems = TrendManager.Instance.GetTrendingItems();

        foreach (var item in trendingItems)
        {
            // Create stats entry even if no demand (shows 0/0/0)
            if (dailyDemand.ContainsKey(item))
            {
                trendingDemand[item] = dailyDemand[item];
            }
            else
            {
                trendingDemand[item] = new DemandStats(); // 0 wanted, 0 sold
            }
        }

        return trendingDemand;
    }

    /// <summary>
    /// Resets daily demand statistics (called at start of each morning).
    /// </summary>
    private void ResetDailyStats()
    {
        dailyDemand.Clear();
        UpdateDemandDisplay();
        Debug.Log("DemandTracker: Daily stats reset");
    }

    /// <summary>
    /// Updates the inspector display list for debugging.
    /// </summary>
    private void UpdateDemandDisplay()
    {
        currentDayDemandDisplay.Clear();
        foreach (var kvp in dailyDemand)
        {
            currentDayDemandDisplay.Add(new DemandEntry
            {
                item = kvp.Key,
                wanted = kvp.Value.wanted,
                sold = kvp.Value.sold,
                missed = kvp.Value.Missed
            });
        }
    }
}

/// <summary>
/// Helper class for displaying demand statistics in the inspector.
/// </summary>
[System.Serializable]
public class DemandEntry
{
    public ItemDataSO item;
    public int wanted;
    public int sold;
    public int missed;
}
