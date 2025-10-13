using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the rent payment UI modal.
/// Displays rent amount, current money, and payment/loan options.
/// </summary>
public class RentPaymentUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject rentPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI rentAmountText;
    [SerializeField] private TextMeshProUGUI currentMoneyText;
    [SerializeField] private TextMeshProUGUI monthText;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private Button payRentButton;
    [SerializeField] private Button takeLoanButton;

    private void Start()
    {
        // Hide panel initially
        if (rentPanel != null)
        {
            rentPanel.SetActive(false);
        }

        // Subscribe to rent due event
        if (ExpenseManager.Instance != null)
        {
            ExpenseManager.Instance.OnRentDue += OnRentDue;
            ExpenseManager.Instance.OnRentPaid += OnRentPaid;
        }

        // Wire up button events
        if (payRentButton != null)
        {
            payRentButton.onClick.AddListener(OnPayRentClicked);
        }

        if (takeLoanButton != null)
        {
            takeLoanButton.onClick.AddListener(OnTakeLoanClicked);
        }
    }

    private void OnDestroy()
    {
        if (ExpenseManager.Instance != null)
        {
            ExpenseManager.Instance.OnRentDue -= OnRentDue;
            ExpenseManager.Instance.OnRentPaid -= OnRentPaid;
        }
    }

    /// <summary>
    /// Called when rent becomes due.
    /// </summary>
    private void OnRentDue(int rentAmount, int month)
    {
        ShowRentPanel(rentAmount, month);
    }

    /// <summary>
    /// Called when rent is successfully paid.
    /// </summary>
    private void OnRentPaid(int amountPaid, int newMonth)
    {
        HideRentPanel();
        Debug.Log($"Rent paid: ${amountPaid}. Starting month {newMonth}");
    }

    /// <summary>
    /// Shows the rent payment panel.
    /// </summary>
    private void ShowRentPanel(int rentAmount, int month)
    {
        if (rentPanel == null)
        {
            Debug.LogError("Rent panel is not assigned!");
            return;
        }

        // Don't show rent panel if game is already over
        if (FailStateManager.Instance != null && FailStateManager.Instance.IsGameOver)
        {
            Debug.Log("Rent due but game is already over - not showing rent panel");
            return;
        }

        rentPanel.SetActive(true);

        // Pause player movement
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetCanMove(false);
        }

        // Update UI elements
        if (titleText != null)
        {
            titleText.text = "Rent Due!";
        }

        if (monthText != null)
        {
            monthText.text = $"Month {month} Complete";
        }

        if (rentAmountText != null)
        {
            rentAmountText.text = $"Rent: ${rentAmount}";
        }

        int currentMoney = GameManager.Instance.CurrentMoney;
        if (currentMoneyText != null)
        {
            currentMoneyText.text = $"Your Money: ${currentMoney}";
        }

        // Check if player can afford rent
        bool canAfford = currentMoney >= rentAmount;

        // Update button states
        if (payRentButton != null)
        {
            payRentButton.interactable = canAfford;
        }

        // Show loan button only if player can take a loan
        bool canTakeLoan = LoanManager.Instance != null && LoanManager.Instance.CanTakeLoan();
        if (takeLoanButton != null)
        {
            takeLoanButton.gameObject.SetActive(!canAfford && canTakeLoan);
        }

        // Show warning if can't afford
        if (warningText != null)
        {
            if (!canAfford)
            {
                if (canTakeLoan)
                {
                    warningText.text = "Insufficient funds! Take a loan to pay rent.";
                    warningText.color = Color.yellow;
                }
                else
                {
                    warningText.text = "GAME OVER: Cannot pay rent!";
                    warningText.color = Color.red;

                    // Don't trigger game over here - FailStateManager handles this automatically
                    // when OnRentFailed event fires from ExpenseManager
                    Debug.Log("Cannot pay rent and no loan available - waiting for FailStateManager");
                }
            }
            else
            {
                warningText.text = "Your landlord demands tribute!";
                warningText.color = Color.white;
            }

            warningText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the rent payment panel.
    /// </summary>
    private void HideRentPanel()
    {
        if (rentPanel != null)
        {
            rentPanel.SetActive(false);
        }

        // Resume player movement
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetCanMove(true);
        }
    }

    /// <summary>
    /// Called when pay rent button is clicked.
    /// </summary>
    private void OnPayRentClicked()
    {
        if (ExpenseManager.Instance != null)
        {
            bool success = ExpenseManager.Instance.PayRent();
            if (success)
            {
                Debug.Log("Rent paid successfully!");
                // Panel will be hidden by OnRentPaid event
            }
            else
            {
                Debug.LogWarning("Failed to pay rent!");
            }
        }
    }

    /// <summary>
    /// Called when take loan button is clicked.
    /// Opens the loan UI.
    /// </summary>
    private void OnTakeLoanClicked()
    {
        // Find and open loan UI
        LoanUI loanUI = FindFirstObjectByType<LoanUI>();
        if (loanUI != null)
        {
            // Calculate how much is needed to pay rent
            int rentAmount = ExpenseManager.Instance.MonthlyRentAmount;
            int shortfall = ExpenseManager.Instance.GetRentShortfall();

            loanUI.ShowLoanPanel(shortfall);

            // Keep rent panel open in background (loan UI will be on top)
        }
        else
        {
            Debug.LogError("LoanUI not found in scene!");
        }
    }
}
