using UnityEngine;

/// <summary>
/// Defines reasons for game over state.
/// </summary>
public enum GameOverReason
{
    RentUnpaid,
    LoanDefault,
    Bankruptcy
}

/// <summary>
/// Consolidated financial management system handling rent, loans, and fail states.
/// Manages all player financial obligations and game over conditions.
/// </summary>
public class FinancialManager : MonoBehaviour
{
    public static FinancialManager Instance;

    #region Rent System
    [Header("Rent Settings")]
    [SerializeField] private int baseRentAmount = 300;
    [SerializeField] private int daysPerMonth = 7;

    [Header("Rent State")]
    [SerializeField] private int daysUntilRentDue = 7;
    [SerializeField] private int currentMonth = 1;
    [SerializeField] private bool rentIsDueNow = false;

    /// <summary>
    /// Event triggered when rent becomes due.
    /// Parameters: rent amount, current month
    /// </summary>
    public event System.Action<int, int> OnRentDue;

    /// <summary>
    /// Event triggered when rent is successfully paid.
    /// Parameters: amount paid, new month number
    /// </summary>
    public event System.Action<int, int> OnRentPaid;

    /// <summary>
    /// Event triggered when player cannot pay rent.
    /// Parameters: rent amount owed
    /// </summary>
    public event System.Action<int> OnRentFailed;

    /// <summary>
    /// Event triggered when days until rent changes (for UI updates).
    /// Parameters: days remaining
    /// </summary>
    public event System.Action<int> OnRentCountdownChanged;
    #endregion

    #region Loan System
    [Header("Loan Settings")]
    [SerializeField] private int[] availableLoanAmounts = new int[] { 300, 500, 1000 };
    [SerializeField] private float interestRate = 0.15f; // 15% interest
    [SerializeField] private int repaymentDays = 7;

    [Header("Loan State")]
    [SerializeField] private bool hasActiveLoan = false;
    [SerializeField] private int loanPrincipal = 0; // Original amount borrowed
    [SerializeField] private int totalOwed = 0; // Principal + interest
    [SerializeField] private int amountPaid = 0;
    [SerializeField] private int daysUntilLoanDue = 0;

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
    #endregion

    #region Fail State System
    [Header("Game Over State")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private GameOverReason gameOverReason;
    [SerializeField] private int amountOwed = 0;

    /// <summary>
    /// Event triggered when game over occurs.
    /// Parameters: reason, days survived, total revenue, amount owed
    /// </summary>
    public event System.Action<GameOverReason, int, int, int> OnGameOver;
    #endregion

    #region Properties
    // Rent Properties
    public int MonthlyRentAmount
    {
        get
        {
            int segmentContribution = 0;
            if (ShopSegmentManager.Instance != null)
            {
                segmentContribution = ShopSegmentManager.Instance.GetRentContribution();
            }
            return baseRentAmount + segmentContribution;
        }
    }
    public int BaseRentAmount => baseRentAmount;
    public int DaysUntilRentDue => daysUntilRentDue;
    public int CurrentMonth => currentMonth;
    public bool RentIsDueNow => rentIsDueNow;

    // Loan Properties
    public bool HasActiveLoan => hasActiveLoan;
    public int LoanPrincipal => loanPrincipal;
    public int TotalOwed => totalOwed;
    public int AmountPaid => amountPaid;
    public int LoanAmountRemaining => UnityEngine.Mathf.Max(0, totalOwed - amountPaid);
    public int DaysUntilLoanDue => daysUntilLoanDue;
    public int[] AvailableLoanAmounts => availableLoanAmounts;
    public float InterestRate => interestRate;

    // Fail State Properties
    public bool IsGameOver => isGameOver;
    public GameOverReason CurrentGameOverReason => gameOverReason;
    public int AmountOwedOnGameOver => amountOwed;
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Validate rent countdown initialization
        if (daysUntilRentDue <= 0)
        {
            daysUntilRentDue = daysPerMonth;
            Debug.LogWarning($"FinancialManager: daysUntilRentDue was invalid, resetting to {daysPerMonth}");
        }
    }

    private void Start()
    {
        // Subscribe to day manager events
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded += OnDayEnded;
        }

        Debug.Log($"FinancialManager initialized: Rent ${MonthlyRentAmount} (Base: ${baseRentAmount} + Segments: ${MonthlyRentAmount - baseRentAmount}) due in {daysUntilRentDue} days");
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded -= OnDayEnded;
        }
    }

    #region Day End Handling
    /// <summary>
    /// Called when a day ends - handles rent countdown, loan payments, and deadlines.
    /// </summary>
    private void OnDayEnded(int day, int customersServed, int revenue)
    {
        // Handle rent countdown
        ProcessRentCountdown();

        // Handle loan auto-payment
        if (hasActiveLoan)
        {
            ProcessLoanPayment();
        }
    }

    private void ProcessRentCountdown()
    {
        // Don't decrement if rent is already due and unpaid
        if (rentIsDueNow)
        {
            Debug.Log("Rent is still due - blocking day progression");
            return;
        }

        daysUntilRentDue--;
        Debug.Log($"Days until rent due: {daysUntilRentDue}");

        OnRentCountdownChanged?.Invoke(daysUntilRentDue);

        // Check if rent is now due
        if (daysUntilRentDue <= 0)
        {
            TriggerRentDue();
        }
    }

    private void ProcessLoanPayment()
    {
        // Decrement loan deadline
        daysUntilLoanDue--;
        Debug.Log($"Loan days remaining: {daysUntilLoanDue}");
        OnLoanDeadlineChanged?.Invoke(daysUntilLoanDue);

        // Calculate suggested daily payment
        int remainingBalance = LoanAmountRemaining;
        int suggestedPayment = Mathf.CeilToInt((float)remainingBalance / Mathf.Max(1, daysUntilLoanDue));

        // Try to auto-deduct payment from player's money
        if (GameManager.Instance.CurrentMoney >= suggestedPayment)
        {
            AttemptLoanPayment(suggestedPayment, true);
        }
        else
        {
            // Player doesn't have enough for even a partial payment
            Debug.LogWarning($"Cannot make loan payment of ${suggestedPayment}. Current money: ${GameManager.Instance.CurrentMoney}");

            // Check if loan deadline has passed
            if (daysUntilLoanDue <= 0)
            {
                Debug.Log("Loan deadline passed - DEFAULTING!");
                DefaultOnLoan();
            }
        }
    }
    #endregion

    #region Rent System Methods
    /// <summary>
    /// Triggers the rent due event and blocks progression.
    /// </summary>
    private void TriggerRentDue()
    {
        rentIsDueNow = true;
        int totalRent = MonthlyRentAmount;
        int segmentContribution = totalRent - baseRentAmount;
        Debug.Log($"💰 Rent is due! Total: ${totalRent} (Base: ${baseRentAmount} + Segments: ${segmentContribution})");
        OnRentDue?.Invoke(totalRent, currentMonth);
    }

    /// <summary>
    /// Attempts to pay rent. Returns true if successful.
    /// </summary>
    public bool PayRent()
    {
        if (!rentIsDueNow)
        {
            Debug.LogWarning("Rent is not due yet!");
            return false;
        }

        int totalRent = MonthlyRentAmount;

        // Check if player has enough money
        if (GameManager.Instance.CurrentMoney < totalRent)
        {
            Debug.Log($"Cannot pay rent - insufficient funds. Need ${totalRent}, have ${GameManager.Instance.CurrentMoney}");
            OnRentPaymentFailed(totalRent);
            return false;
        }

        // Deduct rent payment
        bool success = GameManager.Instance.SpendMoney(totalRent);
        if (success)
        {
            rentIsDueNow = false;
            currentMonth++;
            daysUntilRentDue = daysPerMonth; // Reset countdown

            int segmentContribution = totalRent - baseRentAmount;
            Debug.Log($"✅ Rent paid! Amount: ${totalRent} (Base: ${baseRentAmount} + Segments: ${segmentContribution}). Starting month {currentMonth}. Next rent due in {daysUntilRentDue} days.");
            OnRentPaid?.Invoke(totalRent, currentMonth);
            OnRentCountdownChanged?.Invoke(daysUntilRentDue);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if player can afford rent with current money.
    /// </summary>
    public bool CanAffordRent()
    {
        return GameManager.Instance.CurrentMoney >= MonthlyRentAmount;
    }

    /// <summary>
    /// Gets the amount of money needed to pay rent (shortfall).
    /// Returns 0 if player can afford rent.
    /// </summary>
    public int GetRentShortfall()
    {
        int shortfall = MonthlyRentAmount - GameManager.Instance.CurrentMoney;
        return Mathf.Max(0, shortfall);
    }

    /// <summary>
    /// Gets a detailed breakdown of rent calculation for UI display.
    /// </summary>
    public string GetRentBreakdown()
    {
        int segmentContribution = MonthlyRentAmount - baseRentAmount;
        if (segmentContribution > 0)
        {
            return $"Base Rent: ${baseRentAmount}\nShop Expansions: +${segmentContribution}\nTotal: ${MonthlyRentAmount}";
        }
        return $"Total Rent: ${MonthlyRentAmount}";
    }

    /// <summary>
    /// Called when rent payment fails.
    /// Checks if player can take a loan to pay rent, otherwise triggers game over.
    /// </summary>
    private void OnRentPaymentFailed(int rentAmount)
    {
        Debug.Log($"Rent payment failed: ${rentAmount} owed");
        OnRentFailed?.Invoke(rentAmount);

        // Check if player can take a loan to cover rent
        if (CanTakeLoan())
        {
            Debug.Log("Player can take a loan to pay rent - not game over yet");
            // Don't trigger game over yet - let player decide whether to take loan
            return;
        }

        // Player cannot afford rent and has no loan option - game over
        Debug.Log("Cannot pay rent and no loan available - GAME OVER!");
        TriggerGameOver(GameOverReason.RentUnpaid, rentAmount);
    }
    #endregion

    #region Loan System Methods
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
        daysUntilLoanDue = repaymentDays;
        hasActiveLoan = true;

        // Give player the money
        GameManager.Instance.AddMoney(amount);

        Debug.Log($"Loan taken: ${amount}. Total owed (with {interestRate * 100}% interest): ${totalOwed}. Due in {daysUntilLoanDue} days.");
        OnLoanTaken?.Invoke(amount, totalOwed, daysUntilLoanDue);

        return true;
    }

    /// <summary>
    /// Attempts to make a loan payment.
    /// Returns true if payment was successful.
    /// </summary>
    /// <param name="amount">Amount to pay</param>
    /// <param name="isAutoPayment">Whether this is an automatic daily payment</param>
    public bool AttemptLoanPayment(int amount, bool isAutoPayment = false)
    {
        if (!hasActiveLoan)
        {
            Debug.LogWarning("No active loan to pay!");
            return false;
        }

        int remainingBalance = LoanAmountRemaining;
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
            int newRemaining = LoanAmountRemaining;

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
        daysUntilLoanDue = 0;

        OnLoanFullyRepaid?.Invoke(totalPaid);
    }

    /// <summary>
    /// Handles loan default (game over condition).
    /// </summary>
    private void DefaultOnLoan()
    {
        Debug.Log($"LOAN DEFAULTED! Owed: ${LoanAmountRemaining}");
        OnLoanDefaulted?.Invoke(LoanAmountRemaining);
        TriggerGameOver(GameOverReason.LoanDefault, LoanAmountRemaining);
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
        if (!hasActiveLoan || daysUntilLoanDue <= 0) return 0;
        return Mathf.CeilToInt((float)LoanAmountRemaining / daysUntilLoanDue);
    }
    #endregion

    #region Fail State Methods
    /// <summary>
    /// Triggers game over state.
    /// </summary>
    /// <param name="reason">Reason for game over</param>
    /// <param name="debtAmount">Amount of money owed (if applicable)</param>
    public void TriggerGameOver(GameOverReason reason, int debtAmount = 0)
    {
        if (isGameOver)
        {
            Debug.LogWarning("Game over already triggered!");
            return;
        }

        isGameOver = true;
        gameOverReason = reason;
        amountOwed = debtAmount;

        // Get stats
        int daysSurvived = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 0;
        int totalRevenue = DayManager.Instance != null ? DayManager.Instance.DailyRevenue : 0;

        Debug.Log($"=== GAME OVER ===");
        Debug.Log($"Reason: {reason}");
        Debug.Log($"Days Survived: {daysSurvived}");
        Debug.Log($"Amount Owed: ${debtAmount}");

        // NOTE: Don't pause game here - Time.timeScale = 0 blocks Unity UI input processing
        // The Game Over UI needs input to be responsive for buttons to work
        // Player movement is already blocked by the modal UI

        // Trigger event for UI
        OnGameOver?.Invoke(reason, daysSurvived, totalRevenue, debtAmount);
    }

    /// <summary>
    /// Resets game over state (for restarting game).
    /// </summary>
    public void ResetGameOverState()
    {
        isGameOver = false;
        gameOverReason = GameOverReason.Bankruptcy;
        amountOwed = 0;
    }

    /// <summary>
    /// Gets a descriptive message for the game over reason.
    /// </summary>
    public string GetGameOverMessage()
    {
        switch (gameOverReason)
        {
            case GameOverReason.RentUnpaid:
                return "You've been evicted! The space is now a Spirit Halloween store.";
            case GameOverReason.LoanDefault:
                return "The corporate overlords have repossessed your shop! Should've read the fine print.";
            case GameOverReason.Bankruptcy:
                return "Bankruptcy declared! Time to start a new life as a dungeon crawler.";
            default:
                return "Game Over!";
        }
    }
    #endregion
}
