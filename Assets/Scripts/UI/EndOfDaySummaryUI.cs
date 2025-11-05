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
    [SerializeField] private TextMeshProUGUI rentInfoText;
    [SerializeField] private TextMeshProUGUI loanPaymentText;
    [SerializeField] private Button continueButton;

    [Header("Demand Report Section")]
    [SerializeField] private GameObject demandReportContainer;
    [SerializeField] private Transform demandItemListContainer;
    [SerializeField] private GameObject demandItemRowPrefab;

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

        // Subscribe to rent paid event to show summary after rent is paid
        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnRentPaid += OnRentPaid;
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

        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnRentPaid -= OnRentPaid;
        }
    }

    /// <summary>
    /// Called when the day ends - displays summary with statistics.
    /// If rent is due, waits for rent to be paid before showing summary.
    /// </summary>
    private void OnDayEnded(int day, int customersServed, int revenue)
    {
        if (summaryPanel == null)
        {
            Debug.LogWarning("Summary panel is not assigned!");
            return;
        }

        // Check if rent is due - if so, don't show summary yet
        // It will be shown after rent is paid via OnRentPaid event
        if (FinancialManager.Instance != null && FinancialManager.Instance.RentIsDueNow)
        {
            Debug.Log("EndOfDaySummaryUI: Rent is due - waiting for rent payment before showing summary");
            return;
        }

        Debug.Log($"EndOfDaySummaryUI: Displaying summary for Day {day}");

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

        // Update rent info
        if (rentInfoText != null && FinancialManager.Instance != null)
        {
            int daysUntilRent = FinancialManager.Instance.DaysUntilRentDue;
            int rentAmount = FinancialManager.Instance.MonthlyRentAmount;

            if (daysUntilRent == 1)
            {
                rentInfoText.text = $"⚠ Rent Due Tomorrow: ${rentAmount}";
                rentInfoText.color = Color.yellow;
            }
            else
            {
                rentInfoText.text = $"Rent Due: {daysUntilRent} days (${rentAmount})";
                rentInfoText.color = Color.white;
            }
            rentInfoText.gameObject.SetActive(true);
        }

        // Update loan payment info
        if (loanPaymentText != null && FinancialManager.Instance != null)
        {
            if (FinancialManager.Instance.HasActiveLoan)
            {
                int suggestedPayment = FinancialManager.Instance.GetSuggestedDailyPayment();
                int remaining = FinancialManager.Instance.LoanAmountRemaining;
                int daysLeft = FinancialManager.Instance.DaysUntilLoanDue;

                loanPaymentText.text = $"Loan Balance: ${remaining} (Due in {daysLeft} days)";

                if (daysLeft <= 2)
                {
                    loanPaymentText.color = Color.red;
                }
                else if (daysLeft <= 4)
                {
                    loanPaymentText.color = Color.yellow;
                }
                else
                {
                    loanPaymentText.color = Color.white;
                }

                loanPaymentText.gameObject.SetActive(true);
            }
            else
            {
                loanPaymentText.gameObject.SetActive(false);
            }
        }

        // Populate demand report
        PopulateDemandReport();

        // Show the panel
        summaryPanel.SetActive(true);

        Debug.Log($"End of Day Summary displayed for Day {day}");
    }

    /// <summary>
    /// Populates the demand report section with trending item statistics.
    /// Shows wanted/sold/missed for each trending item, highlighting missed opportunities.
    /// </summary>
    private void PopulateDemandReport()
    {
        // Hide demand report container if references not assigned
        if (demandReportContainer == null || demandItemListContainer == null || demandItemRowPrefab == null)
        {
            if (demandReportContainer != null)
            {
                demandReportContainer.SetActive(false);
            }
            return;
        }

        // Check if we have demand tracking data
        if (DemandTracker.Instance == null || TrendManager.Instance == null)
        {
            demandReportContainer.SetActive(false);
            return;
        }

        // Clear existing demand items
        foreach (Transform child in demandItemListContainer)
        {
            Destroy(child.gameObject);
        }

        // Get trending item demand stats
        var trendingDemand = DemandTracker.Instance.GetTrendingItemDemand();

        if (trendingDemand.Count == 0)
        {
            demandReportContainer.SetActive(false);
            return;
        }

        // Create rows for each trending item
        foreach (var kvp in trendingDemand)
        {
            ItemDataSO item = kvp.Key;
            DemandStats stats = kvp.Value;

            if (item == null) continue;

            GameObject rowObj = Instantiate(demandItemRowPrefab, demandItemListContainer);
            DemandItemRow row = rowObj.GetComponent<DemandItemRow>();

            if (row != null)
            {
                // Highlight missed opportunities (wanted > sold)
                bool missedOpportunity = stats.Missed > 0;
                row.Initialize(item, stats.wanted, stats.sold, stats.Missed, missedOpportunity);
            }
            else
            {
                // Fallback: manually set UI elements if DemandItemRow component doesn't exist
                Image iconImage = rowObj.transform.Find("Icon")?.GetComponent<Image>();
                TextMeshProUGUI nameText = rowObj.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI wantedText = rowObj.transform.Find("Wanted")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI soldText = rowObj.transform.Find("Sold")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI missedText = rowObj.transform.Find("Missed")?.GetComponent<TextMeshProUGUI>();

                if (iconImage != null && item.itemSprite != null)
                {
                    iconImage.sprite = item.itemSprite;
                }

                if (nameText != null)
                {
                    nameText.text = item.itemName;
                }

                if (wantedText != null)
                {
                    wantedText.text = $"Wanted: {stats.wanted}";
                }

                if (soldText != null)
                {
                    soldText.text = $"Sold: {stats.sold}";
                }

                if (missedText != null)
                {
                    missedText.text = $"Missed: {stats.Missed}";
                    // Highlight missed opportunities in red
                    if (stats.Missed > 0)
                    {
                        missedText.color = Color.red;
                    }
                }
            }
        }

        // Show demand report container
        demandReportContainer.SetActive(true);
    }

    /// <summary>
    /// Called when rent is paid - shows the summary with updated money values.
    /// </summary>
    private void OnRentPaid(int amountPaid, int newMonth)
    {
        // Show the summary now that rent has been paid
        // Use current day info from DayManager
        if (DayManager.Instance != null)
        {
            int day = DayManager.Instance.CurrentDay;
            int customersServed = DayManager.Instance.CustomersServedToday;
            int revenue = DayManager.Instance.DailyRevenue;

            Debug.Log($"Rent paid - now showing End of Day Summary for Day {day}");
            OnDayEnded(day, customersServed, revenue);
        }
    }

    /// <summary>
    /// Continue button callback - hides summary and allows player to place orders.
    /// </summary>
    private void OnContinueClicked()
    {
        Debug.Log("Continue to place orders clicked");

        // Play UI click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.UIClick);
        }

        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
        }

        // Note: Player can now open order menu and upgrades via HUD buttons
        Debug.Log("You can now use the HUD buttons to place orders and purchase upgrades");
    }
}
