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
/// Manages game over / fail state conditions.
/// Handles triggering game over UI and tracking failure reasons.
/// </summary>
public class FailStateManager : MonoBehaviour
{
    public static FailStateManager Instance;

    [Header("Current State")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private GameOverReason gameOverReason;
    [SerializeField] private int amountOwed = 0;

    /// <summary>
    /// Event triggered when game over occurs.
    /// Parameters: reason, days survived, total revenue, amount owed
    /// </summary>
    public event System.Action<GameOverReason, int, int, int> OnGameOver;

    // Properties
    public bool IsGameOver => isGameOver;
    public GameOverReason CurrentGameOverReason => gameOverReason;
    public int AmountOwed => amountOwed;

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
        // Subscribe to failure events from ExpenseManager and LoanManager
        if (ExpenseManager.Instance != null)
        {
            ExpenseManager.Instance.OnRentFailed += OnRentPaymentFailed;
        }

        if (LoanManager.Instance != null)
        {
            LoanManager.Instance.OnLoanDefaulted += OnLoanDefaulted;
        }
    }

    private void OnDestroy()
    {
        if (ExpenseManager.Instance != null)
        {
            ExpenseManager.Instance.OnRentFailed -= OnRentPaymentFailed;
        }

        if (LoanManager.Instance != null)
        {
            LoanManager.Instance.OnLoanDefaulted -= OnLoanDefaulted;
        }
    }

    /// <summary>
    /// Called when rent payment fails.
    /// Checks if player can take a loan to pay rent, otherwise triggers game over.
    /// </summary>
    private void OnRentPaymentFailed(int rentAmount)
    {
        Debug.Log($"Rent payment failed: ${rentAmount} owed");

        // Check if player can take a loan to cover rent
        if (LoanManager.Instance != null && LoanManager.Instance.CanTakeLoan())
        {
            Debug.Log("Player can take a loan to pay rent - not game over yet");
            // Don't trigger game over yet - let player decide whether to take loan
            return;
        }

        // Player cannot afford rent and has no loan option - game over
        Debug.LogError("Cannot pay rent and no loan available - GAME OVER!");
        TriggerGameOver(GameOverReason.RentUnpaid, rentAmount);
    }

    /// <summary>
    /// Called when player defaults on a loan.
    /// </summary>
    private void OnLoanDefaulted(int amountOwed)
    {
        Debug.LogError($"Loan defaulted: ${amountOwed} owed - GAME OVER!");
        TriggerGameOver(GameOverReason.LoanDefault, amountOwed);
    }

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
}
