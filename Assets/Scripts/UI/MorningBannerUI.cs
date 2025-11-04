using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Morning banner UI that displays trending items at the start of each week.
/// Shows item icons and names to inform the player what customers will be requesting.
/// </summary>
public class MorningBannerUI : MonoBehaviour
{
    public static MorningBannerUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject bannerPanel;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject itemCardPrefab;
    [SerializeField] private Button dismissButton;

    [Header("Settings")]
    [SerializeField] private string headerTemplate = "📈 Week {0}: Trending Items";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Hide banner by default
        if (bannerPanel != null)
        {
            bannerPanel.SetActive(false);
        }

        // Setup dismiss button
        if (dismissButton != null)
        {
            dismissButton.onClick.AddListener(DismissBanner);
        }

        // Subscribe to DayManager events (must happen before DayManager.Start() fires first event)
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }

        // Subscribe to TrendManager events (for initial display)
        if (TrendManager.Instance != null)
        {
            TrendManager.Instance.OnTrendingItemsChanged += OnTrendingItemsChanged;
        }
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }

        if (TrendManager.Instance != null)
        {
            TrendManager.Instance.OnTrendingItemsChanged -= OnTrendingItemsChanged;
        }

        if (dismissButton != null)
        {
            dismissButton.onClick.RemoveListener(DismissBanner);
        }
    }

    /// <summary>
    /// Called when game phase changes - show banner on Monday mornings.
    /// </summary>
    private void OnPhaseChanged(GamePhase phase)
    {
        // Only show banner at the start of morning phase on Monday (day 1 of week)
        if (phase == GamePhase.Morning && DayManager.Instance != null && DayManager.Instance.DayOfWeek == 1)
        {
            ShowBanner();
        }
    }

    /// <summary>
    /// Called when trending items change (usually start of new week).
    /// </summary>
    private void OnTrendingItemsChanged(List<ItemDataSO> trendingItems)
    {
        // Trending items updated - if we're showing the banner, refresh it
        if (bannerPanel != null && bannerPanel.activeSelf)
        {
            PopulateTrendingItems(trendingItems);
        }
    }

    /// <summary>
    /// Shows the morning banner with current trending items.
    /// </summary>
    private void ShowBanner()
    {
        if (bannerPanel == null)
        {
            Debug.LogError("MorningBannerUI: Banner panel not assigned!");
            return;
        }

        if (TrendManager.Instance == null)
        {
            Debug.LogError("MorningBannerUI: TrendManager not found!");
            return;
        }

        List<ItemDataSO> trendingItems = TrendManager.Instance.GetTrendingItems();

        if (trendingItems.Count == 0)
        {
            Debug.LogWarning("MorningBannerUI: No trending items to display!");
            return;
        }

        // Update header with current week number
        if (headerText != null && DayManager.Instance != null)
        {
            headerText.text = string.Format(headerTemplate, DayManager.Instance.CurrentWeek);
        }

        // Populate trending items
        PopulateTrendingItems(trendingItems);

        // Show banner
        bannerPanel.SetActive(true);

        // Pause game while banner is shown
        Time.timeScale = 0f;

        Debug.Log($"Morning Banner displayed with {trendingItems.Count} trending items");
    }

    /// <summary>
    /// Populates the item list with trending items.
    /// </summary>
    private void PopulateTrendingItems(List<ItemDataSO> trendingItems)
    {
        if (itemListContainer == null || itemCardPrefab == null)
        {
            Debug.LogError("MorningBannerUI: Item list container or card prefab not assigned!");
            return;
        }

        // Clear existing items
        foreach (Transform child in itemListContainer)
        {
            Destroy(child.gameObject);
        }

        // Create item cards
        foreach (ItemDataSO item in trendingItems)
        {
            if (item == null) continue;

            GameObject cardObj = Instantiate(itemCardPrefab, itemListContainer);
            TrendingItemCard card = cardObj.GetComponent<TrendingItemCard>();

            if (card != null)
            {
                card.Initialize(item);
            }
            else
            {
                // Fallback: manually set icon and text if TrendingItemCard component doesn't exist
                Image iconImage = cardObj.GetComponentInChildren<Image>();
                TextMeshProUGUI nameText = cardObj.GetComponentInChildren<TextMeshProUGUI>();

                if (iconImage != null && item.itemSprite != null)
                {
                    iconImage.sprite = item.itemSprite;
                }

                if (nameText != null)
                {
                    nameText.text = item.itemName;
                }
            }
        }
    }

    /// <summary>
    /// Dismisses the banner and resumes game.
    /// </summary>
    public void DismissBanner()
    {
        if (bannerPanel != null)
        {
            bannerPanel.SetActive(false);
        }

        // Resume game
        Time.timeScale = 1f;

        Debug.Log("Morning Banner dismissed");
    }

    /// <summary>
    /// Manually show the banner (useful for testing or special events).
    /// </summary>
    [ContextMenu("Show Banner (Debug)")]
    public void DebugShowBanner()
    {
        ShowBanner();
    }
}
