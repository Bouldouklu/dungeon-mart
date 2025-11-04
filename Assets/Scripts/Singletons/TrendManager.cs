using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages weekly trending items that customers will specifically request.
/// Trends refresh every 7 days (on Monday/start of week).
/// Trending items are randomly selected from all currently unlocked items.
/// </summary>
public class TrendManager : MonoBehaviour
{
    public static TrendManager Instance;

    [Header("Trend Settings")]
    [SerializeField] private int minTrendingItems = 4;
    [SerializeField] private int maxTrendingItems = 5;

    [Header("Current Trending Items (Read-Only)")]
    [SerializeField] private List<ItemDataSO> currentTrendingItems = new List<ItemDataSO>();

    /// <summary>
    /// Event fired when trending items change (start of new week).
    /// </summary>
    public event System.Action<List<ItemDataSO>> OnTrendingItemsChanged;

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
        // Initialize trending items for Day 1 (safe to access other managers in Start())
        RefreshTrendingItems();

        // Subscribe to week changes for future weeks
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnWeekChanged += OnWeekChanged;
        }
        else
        {
            Debug.LogError("TrendManager: DayManager.Instance is null! Cannot subscribe to week changes.");
        }
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnWeekChanged -= OnWeekChanged;
        }
    }

    /// <summary>
    /// Called when a new week starts - refreshes trending items.
    /// </summary>
    private void OnWeekChanged(int newWeek)
    {
        Debug.Log($"TrendManager: Week {newWeek} started, refreshing trending items...");
        RefreshTrendingItems();
    }

    /// <summary>
    /// Selects new trending items randomly from all currently unlocked items.
    /// </summary>
    private void RefreshTrendingItems()
    {
        currentTrendingItems.Clear();

        if (SupplyChainManager.Instance == null)
        {
            Debug.LogError("TrendManager: SupplyChainManager.Instance is null! Cannot select trending items.");
            return;
        }

        List<ItemDataSO> availableItems = SupplyChainManager.Instance.AvailableItems;

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("TrendManager: No available items to select as trending!");
            return;
        }

        // Determine how many trending items to select (random between min and max)
        int trendingCount = Random.Range(minTrendingItems, maxTrendingItems + 1);
        trendingCount = Mathf.Min(trendingCount, availableItems.Count); // Don't exceed available items

        // Randomly select trending items (without duplicates)
        List<ItemDataSO> shuffledItems = availableItems.OrderBy(x => Random.value).ToList();
        currentTrendingItems = shuffledItems.Take(trendingCount).ToList();

        Debug.Log($"TrendManager: Selected {currentTrendingItems.Count} trending items for this week:");
        foreach (var item in currentTrendingItems)
        {
            Debug.Log($"  - {item.itemName} ({item.itemCategory})");
        }

        // Fire event to notify UI systems
        OnTrendingItemsChanged?.Invoke(GetTrendingItems());
    }

    /// <summary>
    /// Checks if a specific item is currently trending.
    /// </summary>
    public bool IsTrending(ItemDataSO item)
    {
        if (item == null) return false;
        return currentTrendingItems.Contains(item);
    }

    /// <summary>
    /// Gets a copy of the current trending items list.
    /// </summary>
    public List<ItemDataSO> GetTrendingItems()
    {
        return new List<ItemDataSO>(currentTrendingItems);
    }

    /// <summary>
    /// Gets a random trending item (useful for customer requests).
    /// Returns null if no trending items available.
    /// </summary>
    public ItemDataSO GetRandomTrendingItem()
    {
        if (currentTrendingItems.Count == 0) return null;
        return currentTrendingItems[Random.Range(0, currentTrendingItems.Count)];
    }

    /// <summary>
    /// Manually refresh trending items (useful for debugging or special events).
    /// </summary>
    [ContextMenu("Force Refresh Trending Items")]
    public void ForceRefreshTrendingItems()
    {
        RefreshTrendingItems();
    }
}
