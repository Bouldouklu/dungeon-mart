using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Persistent HUD element that displays active loan status.
/// Shows loan balance, days remaining, and warning indicators.
/// </summary>
public class LoanStatusUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loanStatusPanel;
    [SerializeField] private TextMeshProUGUI loanBalanceText;
    [SerializeField] private TextMeshProUGUI daysRemainingText;
    [SerializeField] private Image warningIndicator;

    [Header("Warning Settings")]
    [SerializeField] private int warningThresholdDays = 2;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color urgentColor = Color.red;

    private void Start()
    {
        // Hide panel initially (no loan active)
        if (loanStatusPanel != null)
        {
            loanStatusPanel.SetActive(false);
        }

        // Subscribe to financial manager loan events
        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnLoanTaken += OnLoanTaken;
            FinancialManager.Instance.OnLoanPayment += OnLoanPayment;
            FinancialManager.Instance.OnLoanFullyRepaid += OnLoanFullyRepaid;
            FinancialManager.Instance.OnLoanDeadlineChanged += OnLoanDeadlineChanged;
        }

        // Initialize display if loan already active
        if (FinancialManager.Instance != null && FinancialManager.Instance.HasActiveLoan)
        {
            UpdateDisplay();
        }
    }

    private void OnDestroy()
    {
        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnLoanTaken -= OnLoanTaken;
            FinancialManager.Instance.OnLoanPayment -= OnLoanPayment;
            FinancialManager.Instance.OnLoanFullyRepaid -= OnLoanFullyRepaid;
            FinancialManager.Instance.OnLoanDeadlineChanged -= OnLoanDeadlineChanged;
        }
    }

    /// <summary>
    /// Called when a loan is taken.
    /// </summary>
    private void OnLoanTaken(int amount, int totalOwed, int daysUntilDue)
    {
        ShowLoanStatus();
        UpdateDisplay();
    }

    /// <summary>
    /// Called when a loan payment is made.
    /// </summary>
    private void OnLoanPayment(int paymentAmount, int remainingBalance)
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Called when loan is fully repaid.
    /// </summary>
    private void OnLoanFullyRepaid(int totalPaid)
    {
        HideLoanStatus();
    }

    /// <summary>
    /// Called when loan deadline changes.
    /// </summary>
    private void OnLoanDeadlineChanged(int daysRemaining)
    {
        UpdateDisplay();
    }

    /// <summary>
    /// Shows the loan status panel.
    /// </summary>
    private void ShowLoanStatus()
    {
        if (loanStatusPanel != null)
        {
            loanStatusPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the loan status panel.
    /// </summary>
    private void HideLoanStatus()
    {
        if (loanStatusPanel != null)
        {
            loanStatusPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the loan status display.
    /// </summary>
    private void UpdateDisplay()
    {
        if (FinancialManager.Instance == null || !FinancialManager.Instance.HasActiveLoan)
        {
            HideLoanStatus();
            return;
        }

        int amountPaid = FinancialManager.Instance.AmountPaid;
        int totalOwed = FinancialManager.Instance.TotalOwed;
        int remaining = FinancialManager.Instance.LoanAmountRemaining;
        int daysLeft = FinancialManager.Instance.DaysUntilLoanDue;

        // Update balance text
        if (loanBalanceText != null)
        {
            loanBalanceText.text = $"Loan: ${amountPaid}/${totalOwed}";
        }

        // Update days remaining text with color coding
        if (daysRemainingText != null)
        {
            daysRemainingText.text = $"Due in: {daysLeft} days";

            // Color code based on urgency
            if (daysLeft <= 1)
            {
                daysRemainingText.color = urgentColor;
            }
            else if (daysLeft <= warningThresholdDays)
            {
                daysRemainingText.color = warningColor;
            }
            else
            {
                daysRemainingText.color = normalColor;
            }
        }

        // Update warning indicator
        if (warningIndicator != null)
        {
            if (daysLeft <= 1)
            {
                warningIndicator.color = urgentColor;
                warningIndicator.gameObject.SetActive(true);
            }
            else if (daysLeft <= warningThresholdDays)
            {
                warningIndicator.color = warningColor;
                warningIndicator.gameObject.SetActive(true);
            }
            else
            {
                warningIndicator.gameObject.SetActive(false);
            }
        }
    }
}
