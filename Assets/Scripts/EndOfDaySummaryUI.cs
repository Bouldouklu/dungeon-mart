using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the End of Day summary panel UI.
/// Displays day statistics and allows player to continue to ordering phase.
/// </summary>
public class EndOfDaySummaryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject summaryPanel;
    [SerializeField] private TextMeshProUGUI dayNumberText;
    [SerializeField] private TextMeshProUGUI customersServedText;
    [SerializeField] private TextMeshProUGUI dailyRevenueText;
    [SerializeField] private TextMeshProUGUI totalMoneyText;
    [SerializeField] private Button continueButton;

    private void Start()
    {
        // Hide summary panel on start
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
        }

        // Subscribe to day ended event
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded += OnDayEnded;
        }

        // Wire up button event
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded -= OnDayEnded;
        }
    }

    /// <summary>
    /// Called when the day ends - displays summary with statistics.
    /// </summary>
    private void OnDayEnded(int day, int customersServed, int revenue)
    {
        if (summaryPanel == null)
        {
            Debug.LogWarning("Summary panel is not assigned!");
            return;
        }

        // Get total customers for the day
        int totalCustomers = 0;
        if (CustomerSpawner.Instance != null)
        {
            totalCustomers = CustomerSpawner.Instance.TotalCustomersForDay;
        }

        // Get current money
        int currentMoney = 0;
        if (GameManager.Instance != null)
        {
            currentMoney = GameManager.Instance.CurrentMoney;
        }

        // Update UI text elements
        if (dayNumberText != null)
        {
            dayNumberText.text = $"Day {day} Complete!";
        }

        if (customersServedText != null)
        {
            customersServedText.text = $"Customers Served: {customersServed}/{totalCustomers}";
        }

        if (dailyRevenueText != null)
        {
            dailyRevenueText.text = $"Daily Revenue: ${revenue}";
        }

        if (totalMoneyText != null)
        {
            totalMoneyText.text = $"Total Money: ${currentMoney}";
        }

        // Show the panel
        summaryPanel.SetActive(true);

        Debug.Log($"End of Day Summary displayed for Day {day}");
    }

    /// <summary>
    /// Continue button callback - hides summary and allows player to place orders.
    /// </summary>
    private void OnContinueClicked()
    {
        Debug.Log("Continue to place orders clicked");

        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
        }

        // Note: Player can now open order menu with Tab key
        Debug.Log("You can now press Tab to open the order menu");
    }
}
