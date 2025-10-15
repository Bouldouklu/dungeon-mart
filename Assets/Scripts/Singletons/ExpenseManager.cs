using UnityEngine;

/// <summary>
/// Manages monthly rent expenses and rent payment enforcement.
/// Tracks days until rent is due and triggers rent payment events.
/// </summary>
public class ExpenseManager : MonoBehaviour
{
    public static ExpenseManager Instance;

    [Header("Rent Settings")]
    [SerializeField] private int baseRentAmount = 500;
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
    /// <summary>
    /// Total monthly rent amount including base rent + shop segment contributions.
    /// </summary>
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

        Debug.Log($"ExpenseManager initialized: Rent ${MonthlyRentAmount} (Base: ${baseRentAmount} + Segments: ${MonthlyRentAmount - baseRentAmount}) due in {daysUntilRentDue} days");
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
            OnRentFailed?.Invoke(totalRent);
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
}
