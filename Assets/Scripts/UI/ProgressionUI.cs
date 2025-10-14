using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays player progression visually with tier info and progress bar.
/// Shows current tier, progress to next tier, and detailed stats on hover.
/// </summary>
public class ProgressionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image tierIconImage;
    [SerializeField] private TextMeshProUGUI tierNameText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Tooltip (Optional)")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("Settings")]
    [SerializeField] private bool showTooltipOnHover = true;

    private void Start()
    {
        // Hide tooltip initially
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        // Subscribe to progression events
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.OnTierReached += OnTierReached;
            ProgressionManager.Instance.OnLifetimeRevenueChanged += OnLifetimeRevenueChanged;

            // Initialize UI with current progression state
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("ProgressionUI: ProgressionManager not found!");
        }
    }

    private void OnDestroy()
    {
        if (ProgressionManager.Instance != null)
        {
            ProgressionManager.Instance.OnTierReached -= OnTierReached;
            ProgressionManager.Instance.OnLifetimeRevenueChanged -= OnLifetimeRevenueChanged;
        }
    }

    /// <summary>
    /// Called when player reaches a new tier.
    /// </summary>
    private void OnTierReached(ProgressionDataSO newTier, int newTierIndex)
    {
        Debug.Log($"ProgressionUI: Tier reached - {newTier.tierName}");
        UpdateUI();

        // Play celebration animation or sound here (future enhancement)
    }

    /// <summary>
    /// Called when lifetime revenue changes.
    /// </summary>
    private void OnLifetimeRevenueChanged(int newLifetimeRevenue)
    {
        UpdateUI();
    }

    /// <summary>
    /// Updates all UI elements based on current progression state.
    /// </summary>
    private void UpdateUI()
    {
        if (ProgressionManager.Instance == null) return;

        ProgressionDataSO currentTier = ProgressionManager.Instance.CurrentTier;
        ProgressionDataSO nextTier = ProgressionManager.Instance.NextTier;

        // Update tier icon
        if (tierIconImage != null && currentTier != null && currentTier.tierIcon != null)
        {
            tierIconImage.sprite = currentTier.tierIcon;
            tierIconImage.enabled = true;
        }
        else if (tierIconImage != null)
        {
            tierIconImage.enabled = false;
        }

        // Update tier name
        if (tierNameText != null && currentTier != null)
        {
            tierNameText.text = currentTier.tierName;
        }

        // Update progress bar
        if (progressBar != null)
        {
            if (ProgressionManager.Instance.IsMaxTier)
            {
                progressBar.value = 1f;
            }
            else
            {
                float progressPercent = ProgressionManager.Instance.GetProgressToNextTier() / 100f;
                progressBar.value = progressPercent;
            }
        }

        // Update progress text
        if (progressText != null)
        {
            if (ProgressionManager.Instance.IsMaxTier)
            {
                progressText.text = "MAX TIER";
            }
            else if (nextTier != null)
            {
                int lifetimeRevenue = ProgressionManager.Instance.LifetimeRevenue;
                int nextTierRequirement = nextTier.requiredLifetimeRevenue;
                progressText.text = $"${lifetimeRevenue} / ${nextTierRequirement}";
            }
        }
    }

    /// <summary>
    /// Shows tooltip with detailed progression stats (called by Unity Event on hover).
    /// </summary>
    public void ShowTooltip()
    {
        if (!showTooltipOnHover || tooltipPanel == null || tooltipText == null) return;
        if (ProgressionManager.Instance == null) return;

        ProgressionDataSO currentTier = ProgressionManager.Instance.CurrentTier;
        ProgressionDataSO nextTier = ProgressionManager.Instance.NextTier;
        int lifetimeRevenue = ProgressionManager.Instance.LifetimeRevenue;
        int daysPlayed = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 0;

        string tooltipContent = $"<b>Current Tier:</b> {currentTier?.tierName ?? "Unknown"}\n";
        tooltipContent += $"<b>Lifetime Revenue:</b> ${lifetimeRevenue}\n";
        tooltipContent += $"<b>Days Played:</b> {daysPlayed}\n";

        if (!ProgressionManager.Instance.IsMaxTier && nextTier != null)
        {
            int revenueToNext = ProgressionManager.Instance.GetRevenueToNextTier();
            float progressPercent = ProgressionManager.Instance.GetProgressToNextTier();
            tooltipContent += $"\n<b>Next Tier:</b> {nextTier.tierName}\n";
            tooltipContent += $"<b>Progress:</b> {progressPercent:F1}%\n";
            tooltipContent += $"<b>Revenue Needed:</b> ${revenueToNext}";
        }
        else
        {
            tooltipContent += "\n<b>You've reached the highest tier!</b>";
        }

        tooltipText.text = tooltipContent;
        tooltipPanel.SetActive(true);
    }

    /// <summary>
    /// Hides tooltip (called by Unity Event on pointer exit).
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}
