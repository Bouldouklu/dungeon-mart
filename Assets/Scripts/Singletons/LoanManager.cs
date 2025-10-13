using UnityEngine;

/// <summary>
/// Manages player loans with interest and repayment system.
/// Handles loan taking, auto-deduction of payments, and loan defaults.
/// </summary>
public class LoanManager : MonoBehaviour
{
    public static LoanManager Instance;

    [Header("Loan Settings")]
    [SerializeField] private int[] availableLoanAmounts = new int[] { 300, 500, 1000 };
    [SerializeField] private float interestRate = 0.20f; // 20% interest
    [SerializeField] private int repaymentDays = 7;

    [Header("Current Loan State")]
    [SerializeField] private bool hasActiveLoan = false;
    [SerializeField] private int loanPrincipal = 0; // Original amount borrowed
    [SerializeField] private int totalOwed = 0; // Principal + interest
    [SerializeField] private int amountPaid = 0;
    [SerializeField] private int daysUntilDue = 0;

    /// <summary>
    /// Event triggered when a loan is taken.
    /// Parameters: loan amount, total owed with interest, days until due
    /// </summary>
    public event System.Action<int, int, int> OnLoanTaken;

    /// <summary>
    /// Event triggered when a loan payment is made (manual or auto).
    /// Parameters: payment amount, remaining balance
    /// </summary>
    public event System.Action<int, int> OnLoanPayment;

    /// <summary>
    /// Event triggered when loan is fully repaid.
    /// Parameters: total amount repaid
    /// </summary>
    public event System.Action<int> OnLoanFullyRepaid;

    /// <summary>
    /// Event triggered when player defaults on loan (can't make payment).
    /// Parameters: amount owed
    /// </summary>
    public event System.Action<int> OnLoanDefaulted;

    /// <summary>
    /// Event triggered when days until loan due changes.
    /// Parameters: days remaining
    /// </summary>
    public event System.Action<int> OnLoanDeadlineChanged;

    // Properties
    public bool HasActiveLoan => hasActiveLoan;
    public int LoanPrincipal => loanPrincipal;
    public int TotalOwed => totalOwed;
    public int AmountPaid => amountPaid;
    public int AmountRemaining => Mathf.Max(0, totalOwed - amountPaid);
    public int DaysUntilDue => daysUntilDue;
    public int[] AvailableLoanAmounts => availableLoanAmounts;
    public float InterestRate => interestRate;

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
        // Subscribe to day manager events for auto-deduction
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded += OnDayEnded;
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
    /// Called when a day ends - handles auto-deduction and deadline tracking.
    /// </summary>
    private void OnDayEnded(int day, int customersServed, int revenue)
    {
        if (!hasActiveLoan) return;

        // Decrement loan deadline
        daysUntilDue--;
        Debug.Log($"Loan days remaining: {daysUntilDue}");
        OnLoanDeadlineChanged?.Invoke(daysUntilDue);

        // Calculate suggested daily payment
        int remainingBalance = AmountRemaining;
        int suggestedPayment = Mathf.CeilToInt((float)remainingBalance / Mathf.Max(1, daysUntilDue));

        // Try to auto-deduct payment from player's money
        if (GameManager.Instance.CurrentMoney >= suggestedPayment)
        {
            AttemptPayment(suggestedPayment, true);
        }
        else
        {
            // Player doesn't have enough for even a partial payment
            Debug.LogWarning($"Cannot make loan payment of ${suggestedPayment}. Current money: ${GameManager.Instance.CurrentMoney}");

            // Check if loan deadline has passed
            if (daysUntilDue <= 0)
            {
                Debug.LogError("Loan deadline passed - DEFAULTING!");
                DefaultOnLoan();
            }
        }
    }

    /// <summary>
    /// Takes out a loan for the specified amount.
    /// Returns true if successful.
    /// </summary>
    public bool TakeLoan(int amount)
    {
        if (hasActiveLoan)
        {
            Debug.LogWarning("Cannot take loan - already have an active loan!");
            return false;
        }

        // Validate loan amount
        bool isValidAmount = false;
        foreach (int validAmount in availableLoanAmounts)
        {
            if (amount == validAmount)
            {
                isValidAmount = true;
                break;
            }
        }

        if (!isValidAmount)
        {
            Debug.LogError($"Invalid loan amount: ${amount}");
            return false;
        }

        // Calculate total owed with interest
        loanPrincipal = amount;
        totalOwed = Mathf.RoundToInt(amount * (1f + interestRate));
        amountPaid = 0;
        daysUntilDue = repaymentDays;
        hasActiveLoan = true;

        // Give player the money
        GameManager.Instance.AddMoney(amount);

        Debug.Log($"Loan taken: ${amount}. Total owed (with {interestRate * 100}% interest): ${totalOwed}. Due in {daysUntilDue} days.");
        OnLoanTaken?.Invoke(amount, totalOwed, daysUntilDue);

        return true;
    }

    /// <summary>
    /// Attempts to make a loan payment.
    /// Returns true if payment was successful.
    /// </summary>
    /// <param name="amount">Amount to pay</param>
    /// <param name="isAutoPayment">Whether this is an automatic daily payment</param>
    public bool AttemptPayment(int amount, bool isAutoPayment = false)
    {
        if (!hasActiveLoan)
        {
            Debug.LogWarning("No active loan to pay!");
            return false;
        }

        int remainingBalance = AmountRemaining;
        int actualPayment = Mathf.Min(amount, remainingBalance); // Don't overpay

        // Check if player has enough money
        if (GameManager.Instance.CurrentMoney < actualPayment)
        {
            Debug.LogWarning($"Insufficient funds for loan payment. Need ${actualPayment}, have ${GameManager.Instance.CurrentMoney}");
            return false;
        }

        // Deduct payment
        bool success = GameManager.Instance.SpendMoney(actualPayment);
        if (success)
        {
            amountPaid += actualPayment;
            int newRemaining = AmountRemaining;

            string paymentType = isAutoPayment ? "Auto-payment" : "Manual payment";
            Debug.Log($"{paymentType}: ${actualPayment}. Loan balance: ${newRemaining}/${totalOwed}");

            OnLoanPayment?.Invoke(actualPayment, newRemaining);

            // Check if loan is fully paid
            if (newRemaining <= 0)
            {
                CompleteLoanRepayment();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Completes loan repayment and clears loan state.
    /// </summary>
    private void CompleteLoanRepayment()
    {
        Debug.Log($"Loan fully repaid! Total paid: ${amountPaid}");

        int totalPaid = amountPaid;

        // Clear loan state
        hasActiveLoan = false;
        loanPrincipal = 0;
        totalOwed = 0;
        amountPaid = 0;
        daysUntilDue = 0;

        OnLoanFullyRepaid?.Invoke(totalPaid);
    }

    /// <summary>
    /// Handles loan default (game over condition).
    /// </summary>
    private void DefaultOnLoan()
    {
        Debug.LogError($"LOAN DEFAULTED! Owed: ${AmountRemaining}");

        // Trigger game over via event - FailStateManager will handle the actual game over trigger
        // Don't call TriggerGameOver directly to avoid duplicate triggers
        OnLoanDefaulted?.Invoke(AmountRemaining);
    }

    /// <summary>
    /// Calculates total amount owed for a potential loan.
    /// </summary>
    public int CalculateLoanWithInterest(int loanAmount)
    {
        return Mathf.RoundToInt(loanAmount * (1f + interestRate));
    }

    /// <summary>
    /// Checks if player can take a loan (no active loan).
    /// </summary>
    public bool CanTakeLoan()
    {
        return !hasActiveLoan;
    }

    /// <summary>
    /// Gets the suggested daily payment amount.
    /// </summary>
    public int GetSuggestedDailyPayment()
    {
        if (!hasActiveLoan || daysUntilDue <= 0) return 0;
        return Mathf.CeilToInt((float)AmountRemaining / daysUntilDue);
    }
}
