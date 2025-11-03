#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Linq;
using UnityEngine;

/// <summary>
/// Centralized manager for all debug input keys.
/// Wrapped in compilation directive to exclude from release builds.
/// </summary>
public class DebugInputManager : MonoBehaviour
{
    public static DebugInputManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        HandleDayControlKeys();
        HandleTimeScaleKeys();
        HandleMoneyKeys();
        HandleTierKeys();
        HandleUpgradeKeys();
        HandleInventoryKeys();
    }

    #region Day Control Keys

    /// <summary>
    /// Handles debug keys for manual day and phase transitions.
    /// M - Start next day
    /// O - Open shop
    /// K - End day
    /// </summary>
    private void HandleDayControlKeys()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (DayManager.Instance != null)
            {
                DayManager.Instance.StartNextDay();
            }
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (DayManager.Instance != null)
            {
                DayManager.Instance.OpenShop();
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (DayManager.Instance != null)
            {
                DayManager.Instance.EndDay();
            }
        }
    }

    #endregion

    #region Time Scale Keys

    /// <summary>
    /// Handles debug keys for time scale adjustments.
    /// 1 - Normal speed (1x)
    /// 2 - Double speed (2x)
    /// 3 - Triple speed (3x)
    /// 5 - Quintuple speed (5x)
    /// </summary>
    private void HandleTimeScaleKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1f;
            Debug.Log("DEBUG: Time scale set to 1x (Normal)");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 2f;
            Debug.Log("DEBUG: Time scale set to 2x");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 3f;
            Debug.Log("DEBUG: Time scale set to 3x");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Time.timeScale = 5f;
            Debug.Log("DEBUG: Time scale set to 5x");
        }
    }

    #endregion

    #region Money Keys

    /// <summary>
    /// Handles debug keys for adding money for progression testing.
    /// 7 - Add $500
    /// 8 - Add $1500 (Tier 1 threshold)
    /// 9 - Add $5000
    /// </summary>
    private void HandleMoneyKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(500);
                Debug.Log("DEBUG: Added $500 for progression testing");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(1500);
                Debug.Log("DEBUG: Added $1500 for progression testing (Tier 1 threshold)");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(5000);
                Debug.Log("DEBUG: Added $5000 for progression testing");
            }
        }
    }

    #endregion

    #region Tier Testing Keys

    /// <summary>
    /// Handles debug keys for testing item tier unlocking.
    /// U - Unlock next tier (cycles 1→2→3)
    /// Shift+U - Reset to Tier 1
    /// </summary>
    private void HandleTierKeys()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (SupplyChainManager.Instance != null)
            {
                // Check if Shift is held - reset to Tier 1
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    // Manually reset tier (direct assignment since there's no reset method)
                    int currentTier = SupplyChainManager.Instance.CurrentTier;
                    if (currentTier > 1)
                    {
                        // Unlock tier 1 to reset
                        Debug.Log($"DEBUG: Reset tier from {currentTier} to 1 (NOT SUPPORTED - tiers only go up!)");
                        Debug.LogWarning("Tier reset not supported in current implementation. Restart scene to reset tiers.");
                    }
                    else
                    {
                        Debug.Log("DEBUG: Already at Tier 1");
                    }
                }
                else
                {
                    // Unlock next tier
                    int currentTier = SupplyChainManager.Instance.CurrentTier;
                    int nextTier = currentTier + 1;

                    if (nextTier <= 3)
                    {
                        SupplyChainManager.Instance.UnlockTier(nextTier);
                        Debug.Log($"DEBUG: Unlocked Tier {nextTier}! Items up to tier {nextTier} now available.");
                    }
                    else
                    {
                        Debug.Log("DEBUG: Already at maximum tier (3)!");
                    }
                }
            }
            else
            {
                Debug.LogError("DEBUG: SupplyChainManager.Instance is null!");
            }
        }
    }

    #endregion

    #region Upgrade Testing Keys

    /// <summary>
    /// Handles debug keys for testing shop upgrades and segments.
    /// F4 - Unlock shop segment 1
    /// F5 - Unlock shop segment 2
    /// F6 - Unlock shop segment 3
    /// F7 - Increase all shelf capacity by 2
    /// F8 - Add 3 bonus customers
    /// F9 - Log rent contribution debug info
    /// F10 - Pay rent immediately (if due and affordable)
    /// F11 - Complete next incomplete objective
    /// F12 - Add 10 items sold to random category
    /// </summary>
    private void HandleUpgradeKeys()
    {
        // Shop segment unlocks
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

        // Shelf capacity upgrade test
        if (Input.GetKeyDown(KeyCode.F7))
        {
            if (UpgradeManager.Instance != null)
            {
                Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
                foreach (Shelf shelf in shelves)
                {
                    shelf.IncreaseCapacity(2);
                }
                Debug.Log($"DEBUG: Increased capacity for {shelves.Length} shelves by 2");
            }
        }

        // Customer bonus test
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (CustomerSpawner.Instance != null)
            {
                CustomerSpawner.Instance.AddBonusCustomers(3);
                Debug.Log($"DEBUG: Added 3 bonus customers. New total: {CustomerSpawner.Instance.CustomersPerDay} customers/day");
            }
        }

        // Rent contribution debug info
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

        // Force rent payment test
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (FinancialManager.Instance != null)
            {
                Debug.Log($"DEBUG: Rent Status - Due: {FinancialManager.Instance.RentIsDueNow}, Days Until: {FinancialManager.Instance.DaysUntilRentDue}, Month: {FinancialManager.Instance.CurrentMonth}");
                if (FinancialManager.Instance.RentIsDueNow && FinancialManager.Instance.CanAffordRent())
                {
                    Debug.Log("DEBUG: Attempting to pay rent...");
                    FinancialManager.Instance.PayRent();
                }
            }
        }

        // F11 - Complete next incomplete objective
        if (Input.GetKeyDown(KeyCode.F11))
        {
            if (ObjectiveManager.Instance != null)
            {
                var visibleObjectives = ObjectiveManager.Instance.GetVisibleObjectives();
                var incompleteObjective = visibleObjectives.FirstOrDefault(obj => !ObjectiveManager.Instance.IsObjectiveCompleted(obj));

                if (incompleteObjective != null)
                {
                    ObjectiveManager.Instance.DebugCompleteObjective(incompleteObjective);
                    Debug.Log($"DEBUG: Completed objective '{incompleteObjective.objectiveName}'");
                }
                else
                {
                    Debug.Log("DEBUG: No incomplete objectives found!");
                }
            }
        }

        // F12 - Add 10 items sold to random category
        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.DebugAddItemsSold(10);
            }
        }
    }

    #endregion

    #region Inventory Keys

    /// <summary>
    /// Handles debug keys for inventory testing.
    /// I - Add debug inventory items
    /// </summary>
    private void HandleInventoryKeys()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddDebugInventory();
                Debug.Log("DEBUG: Added debug inventory items");
            }
        }
    }

    #endregion
}
#endif
