using System.Collections;
using UnityEngine;

public enum GamePhase
{
    Morning,
    OpenForBusiness,
    EndOfDay
}

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Day Settings")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private GamePhase currentPhase = GamePhase.Morning;

    [Header("Week Tracking")]
    [SerializeField] private int currentWeek = 1;
    [SerializeField] private int dayOfWeek = 1; // 1-7 (Monday-Sunday)

    [Header("Day Statistics")]
    [SerializeField] private int customersServedToday = 0;
    [SerializeField] private int dailyRevenue = 0;

    // Events
    public event System.Action<GamePhase> OnPhaseChanged;
    public event System.Action<int> OnDayStarted;
    public event System.Action<int, int, int> OnDayEnded; // day, customers served, revenue
    public event System.Action<int> OnWeekChanged; // week number

    // Properties
    public GamePhase CurrentPhase => currentPhase;
    public int CurrentDay => currentDay;
    public int CurrentWeek => currentWeek;
    public int DayOfWeek => dayOfWeek;
    public int CustomersServedToday => customersServedToday;
    public int DailyRevenue => dailyRevenue;

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
        // Delay initialization by one frame to ensure all other managers' Start() methods complete first
        // This guarantees TrendManager, ObjectiveManager, etc. are fully initialized before we fire events
        StartCoroutine(InitializeDayManagerCoroutine());
    }

    /// <summary>
    /// Initializes DayManager after waiting one frame for all other managers to complete their Start() methods.
    /// </summary>
    private System.Collections.IEnumerator InitializeDayManagerCoroutine()
    {
        // Wait one frame to ensure all Start() methods have completed
        yield return null;

        // Ensure game is not paused on start (safety fix)
        Time.timeScale = 1f;
        Debug.Log("DayManager.Start(): Set Time.timeScale = 1");

        // Subscribe to game over event to stop coroutines when game ends
        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnGameOver += OnGameOverTriggered;
            Debug.Log("DayManager subscribed to OnGameOver event");
        }

        // Start first day in morning phase (now all managers are ready)
        StartMorningPhase();
    }

    // Debug input handling has been moved to DebugInputManager.cs

    public void StartMorningPhase()
    {
        currentPhase = GamePhase.Morning;

        // Calculate week and day of week
        int previousWeek = currentWeek;
        currentWeek = (currentDay - 1) / 7 + 1;
        dayOfWeek = ((currentDay - 1) % 7) + 1; // 1-7 (Monday=1, Sunday=7)

        Debug.Log($"=== DAY {currentDay} (Week {currentWeek}, Day {dayOfWeek}) - MORNING PHASE ===");
        Debug.Log("Open delivery boxes and restock shelves. Press O to open shop.");

        // Reset daily stats
        customersServedToday = 0;
        dailyRevenue = 0;

        // Fire week changed event if we entered a new week
        if (currentWeek != previousWeek)
        {
            Debug.Log($"*** NEW WEEK {currentWeek} STARTED ***");
            OnWeekChanged?.Invoke(currentWeek);
        }

        OnPhaseChanged?.Invoke(currentPhase);
        OnDayStarted?.Invoke(currentDay);
    }

    public void OpenShop()
    {
        if (currentPhase != GamePhase.Morning)
        {
            Debug.LogWarning("Can only open shop during morning phase!");
            return;
        }

        currentPhase = GamePhase.OpenForBusiness;
        Debug.Log($"=== SHOP IS NOW OPEN - DAY {currentDay} ===");
        Debug.Log("Serve customers and keep shelves stocked!");

        OnPhaseChanged?.Invoke(currentPhase);
    }

    public void EndDay()
    {
        if (currentPhase != GamePhase.OpenForBusiness)
        {
            Debug.LogWarning("Can only end day from business phase!");
            return;
        }

        currentPhase = GamePhase.EndOfDay;
        Debug.Log($"=== DAY {currentDay} COMPLETE ===");
        Debug.Log($"Customers served: {customersServedToday}");
        Debug.Log($"Revenue earned: ${dailyRevenue}");
        Debug.Log("Place orders for tomorrow!");

        OnPhaseChanged?.Invoke(currentPhase);
        OnDayEnded?.Invoke(currentDay, customersServedToday, dailyRevenue);
    }

    public void StartNextDay()
    {
        if (currentPhase != GamePhase.EndOfDay)
        {
            Debug.LogWarning("Can only start next day from end of day phase!");
            return;
        }

        currentDay++;
        StartMorningPhase();
    }

    /// <summary>
    /// Progresses to the next phase intelligently based on current phase.
    /// Morning → Opens shop | Business → Closes shop (waits for customers) | EndOfDay → Next day
    /// </summary>
    public void ProgressToNextPhase()
    {
        StartCoroutine(ProgressToNextPhaseCoroutine());
    }

    private System.Collections.IEnumerator ProgressToNextPhaseCoroutine()
    {
        switch (currentPhase)
        {
            case GamePhase.Morning:
                Debug.Log("Progress button: Opening shop...");
                OpenShop();
                break;

            case GamePhase.OpenForBusiness:
                Debug.Log("Progress button: Closing shop...");

                // Stop new customers from spawning
                if (CustomerSpawner.Instance != null)
                {
                    CustomerSpawner.Instance.StopSpawning();
                }

                // Wait for all active customers to finish checkout
                if (CustomerSpawner.Instance != null)
                {
                    while (!CustomerSpawner.Instance.AreAllCustomersGone())
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }

                Debug.Log("All customers have left. Ending day...");
                EndDay();
                break;

            case GamePhase.EndOfDay:
                Debug.Log("Progress button: Starting next day...");
                StartNextDay();
                break;
        }
    }

    public void RecordCustomerSale(int saleAmount)
    {
        customersServedToday++;
        dailyRevenue += saleAmount;
        Debug.Log($"Sale recorded: ${saleAmount}. Today's revenue: ${dailyRevenue}");
    }

    public void RecordCustomerLeft()
    {
        // Customer left without purchasing - still counts as "served"
        Debug.Log("Customer left empty-handed");
    }

    /// <summary>
    /// Called when game over is triggered. Stops all running coroutines to prevent Unity input freeze bug.
    /// </summary>
    private void OnGameOverTriggered(GameOverReason reason, int daysSurvived, int totalRevenue, int amountOwed)
    {
        Debug.Log("DayManager: Game Over detected - stopping all coroutines to prevent input freeze");
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        // Unsubscribe from game over event
        if (FinancialManager.Instance != null)
        {
            FinancialManager.Instance.OnGameOver -= OnGameOverTriggered;
        }
    }
}
