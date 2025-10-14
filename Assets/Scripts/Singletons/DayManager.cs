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

    [Header("Day Statistics")]
    [SerializeField] private int customersServedToday = 0;
    [SerializeField] private int dailyRevenue = 0;

    // Events
    public event System.Action<GamePhase> OnPhaseChanged;
    public event System.Action<int> OnDayStarted;
    public event System.Action<int, int, int> OnDayEnded; // day, customers served, revenue

    // Properties
    public GamePhase CurrentPhase => currentPhase;
    public int CurrentDay => currentDay;
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
        // Start first day in morning phase
        StartMorningPhase();
    }

    private void Update()
    {
        // Debug keys for manual phase transitions
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartNextDay();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            OpenShop();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            EndDay();
        }

        // Time scale controls for testing
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1f;
            Debug.Log("Time scale: 1x (Normal)");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 2f;
            Debug.Log("Time scale: 2x");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 3f;
            Debug.Log("Time scale: 3x");
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Time.timeScale = 5f;
            Debug.Log("Time scale: 5x");
        }

        // Debug keys for progression testing
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            GameManager.Instance?.AddMoney(500);
            Debug.Log("DEBUG: Added $500 for progression testing");
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            GameManager.Instance?.AddMoney(1500);
            Debug.Log("DEBUG: Added $1500 for progression testing (Tier 1 threshold)");
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            GameManager.Instance?.AddMoney(5000);
            Debug.Log("DEBUG: Added $5000 for progression testing");
        }

        // Debug keys for shop segment testing
        if (Input.GetKeyDown(KeyCode.F4))
        {
            if (ShopSegmentManager.Instance != null)
            {
                ShopSegmentManager.Instance.UnlockSegment(1);
                Debug.Log("DEBUG: Attempted to unlock Segment 1");
            }
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (ShopSegmentManager.Instance != null)
            {
                ShopSegmentManager.Instance.UnlockSegment(2);
                Debug.Log("DEBUG: Attempted to unlock Segment 2");
            }
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            if (ShopSegmentManager.Instance != null)
            {
                ShopSegmentManager.Instance.UnlockSegment(3);
                Debug.Log("DEBUG: Attempted to unlock Segment 3");
            }
        }

        // Debug key for shelf capacity upgrade testing
        if (Input.GetKeyDown(KeyCode.F7))
        {
            if (UpgradeManager.Instance != null)
            {
                // Find all shelves and increase capacity by 2
                Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
                foreach (Shelf shelf in shelves)
                {
                    shelf.IncreaseCapacity(2);
                }
                Debug.Log($"DEBUG: Increased capacity for {shelves.Length} shelves by 2");
            }
        }

        // Debug key for customer bonus testing
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (CustomerSpawner.Instance != null)
            {
                CustomerSpawner.Instance.AddBonusCustomers(3);
                Debug.Log($"DEBUG: Added 3 bonus customers. New total: {CustomerSpawner.Instance.CustomersPerDay} customers/day");
            }
        }

        // Debug key for rent contribution testing
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (ShopSegmentManager.Instance != null)
            {
                int rent = ShopSegmentManager.Instance.GetRentContribution();
                int unlockedCount = ShopSegmentManager.Instance.UnlockedSegmentCount;
                Debug.Log($"DEBUG: Rent Contribution = ${rent} ({unlockedCount} segments unlocked)");
                Debug.Log(ShopSegmentManager.Instance.GetSegmentStatusDebug());
            }
        }

        // Debug key for ExpenseManager status
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (ExpenseManager.Instance != null)
            {
                Debug.Log($"DEBUG: Rent Status - Due: {ExpenseManager.Instance.RentIsDueNow}, Days Until: {ExpenseManager.Instance.DaysUntilRentDue}, Month: {ExpenseManager.Instance.CurrentMonth}");
                if (ExpenseManager.Instance.RentIsDueNow && ExpenseManager.Instance.CanAffordRent())
                {
                    Debug.Log("DEBUG: Attempting to pay rent...");
                    ExpenseManager.Instance.PayRent();
                }
            }
        }
    }

    public void StartMorningPhase()
    {
        currentPhase = GamePhase.Morning;
        Debug.Log($"=== DAY {currentDay} - MORNING PHASE ===");
        Debug.Log("Open delivery boxes and restock shelves. Press O to open shop.");

        // Reset daily stats
        customersServedToday = 0;
        dailyRevenue = 0;

        // Play morning music
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlayMusic(MusicType.MorningMusic, fadeIn: true);
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

        // Crossfade to business music
        if (AudioManager.Instance != null) {
            AudioManager.Instance.CrossfadeMusic(MusicType.BusinessMusic);
        }

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

        // Crossfade to evening music
        if (AudioManager.Instance != null) {
            AudioManager.Instance.CrossfadeMusic(MusicType.EveningMusic);
        }

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
}
