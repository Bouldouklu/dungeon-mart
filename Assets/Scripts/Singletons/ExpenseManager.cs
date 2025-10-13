using UnityEngine;

/// <summary>
/// Manages monthly rent expenses and rent payment enforcement.
/// Tracks days until rent is due and triggers rent payment events.
/// </summary>
public class ExpenseManager : MonoBehaviour
{
    public static ExpenseManager Instance;

    [Header("Rent Settings")]
    [SerializeField] private int monthlyRentAmount = 500;
    [SerializeField] private int daysPerMonth = 7;

    [Header("Current State")]
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

    // Properties
    public int MonthlyRentAmount => monthlyRentAmount;
    public int DaysUntilRentDue => daysUntilRentDue;
    public int CurrentMonth => currentMonth;
    public bool RentIsDueNow => rentIsDueNow;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Ensure valid countdown initialization (handles Unity Inspector edge cases)
        if (daysUntilRentDue <= 0)
        {
            daysUntilRentDue = daysPerMonth;
            Debug.LogWarning($"ExpenseManager: daysUntilRentDue was invalid ({daysUntilRentDue}), resetting to {daysPerMonth}");
        }
    }

    private void Start()
    {
        // Subscribe to day manager events
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded += OnDayEnded;
        }

        Debug.Log($"ExpenseManager initialized: Rent ${monthlyRentAmount} due in {daysUntilRentDue} days");
    }

    private void OnDestroy()
    {
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded -= OnDayEnded;
        }
    }

    /// <summary>
    /// Called when a day ends - decrements rent countdown.
    /// </summary>
    private void OnDayEnded(int day, int customersServed, int revenue)
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

    /// <summary>
    /// Triggers the rent due event and blocks progression.
    /// </summary>
    private void TriggerRentDue()
    {
        rentIsDueNow = true;
        Debug.Log($"Rent is due! Amount: ${monthlyRentAmount}");
        OnRentDue?.Invoke(monthlyRentAmount, currentMonth);
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

        // Check if player has enough money
        if (GameManager.Instance.CurrentMoney < monthlyRentAmount)
        {
            Debug.Log($"Cannot pay rent - insufficient funds. Need ${monthlyRentAmount}, have ${GameManager.Instance.CurrentMoney}");
            OnRentFailed?.Invoke(monthlyRentAmount);
            return false;
        }

        // Deduct rent payment
        bool success = GameManager.Instance.SpendMoney(monthlyRentAmount);
        if (success)
        {
            rentIsDueNow = false;
            currentMonth++;
            daysUntilRentDue = daysPerMonth; // Reset countdown

            Debug.Log($"Rent paid! Starting month {currentMonth}. Next rent due in {daysUntilRentDue} days.");
            OnRentPaid?.Invoke(monthlyRentAmount, currentMonth);
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
        return GameManager.Instance.CurrentMoney >= monthlyRentAmount;
    }

    /// <summary>
    /// Gets the amount of money needed to pay rent (shortfall).
    /// Returns 0 if player can afford rent.
    /// </summary>
    public int GetRentShortfall()
    {
        int shortfall = monthlyRentAmount - GameManager.Instance.CurrentMoney;
        return Mathf.Max(0, shortfall);
    }
}
