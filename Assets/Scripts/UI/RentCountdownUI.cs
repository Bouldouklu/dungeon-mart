using UnityEngine;
using TMPro;

/// <summary>
/// Persistent HUD element that displays rent countdown.
/// Shows days until rent is due with warning colors.
/// </summary>
public class RentCountdownUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI rentCountdownText;

    [Header("Warning Settings")]
    [SerializeField] private int warningThresholdDays = 2;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color urgentColor = Color.red;

    private void Start()
    {
        // Subscribe to financial manager events
        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnRentCountdownChanged += OnRentCountdownChanged;
            FinancialManager.Instance.OnRentPaid += OnRentPaid;
        }

        // Initialize display
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnRentCountdownChanged -= OnRentCountdownChanged;
            FinancialManager.Instance.OnRentPaid -= OnRentPaid;
        }
    }

    /// <summary>
    /// Called when rent countdown changes.
    /// </summary>
    private void OnRentCountdownChanged(int daysRemaining)
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Called when rent is paid.
    /// </summary>
    private void OnRentPaid(int amountPaid, int newMonth)
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Updates the rent countdown display.
    /// </summary>
    private void UpdateDisplay()
    {
        if (FinancialManager.Instance == null || rentCountdownText == null)
        {
            return;
        }

        int daysLeft = FinancialManager.Instance.DaysUntilRentDue;
        int rentAmount = FinancialManager.Instance.MonthlyRentAmount;
        int currentMonth = FinancialManager.Instance.CurrentMonth;

        // Update text
        rentCountdownText.text = $"Rent Due: {daysLeft} days (${rentAmount})";

        // Color code based on urgency
        if (daysLeft <= 1)
        {
            rentCountdownText.color = urgentColor;
        }
        else if (daysLeft <= warningThresholdDays)
        {
            rentCountdownText.color = warningColor;
        }
        else
        {
            rentCountdownText.color = normalColor;
        }
    }
}
