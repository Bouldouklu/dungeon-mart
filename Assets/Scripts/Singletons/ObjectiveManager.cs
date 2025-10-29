using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Singleton manager that tracks all objectives and player progress
/// Replaces the old tier-based ProgressionManager
/// </summary>
public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Objective Configuration")]
    [Tooltip("All objectives in the game (load from Resources/Objectives/)")]
    [SerializeField] private ObjectiveDataSO[] allObjectives;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Lifetime Statistics Tracking
    private int lifetimeRevenue = 0;
    private int customersServedTotal = 0;
    private Dictionary<ItemCategory, int> itemsSoldByCategory = new Dictionary<ItemCategory, int>();
    private int itemsSoldTotal = 0;

    // Objective Completion Tracking
    private HashSet<string> completedObjectives = new HashSet<string>();
    private Dictionary<string, int> objectiveProgress = new Dictionary<string, int>();

    // Events
    public event Action<ObjectiveDataSO> OnObjectiveCompleted;
    public event Action<ObjectiveDataSO, int, int> OnObjectiveProgressChanged; // (objective, current, target)
    public event Action<ObjectiveDataSO> OnObjectiveRevealed;

    // Public Properties
    public int LifetimeRevenue => lifetimeRevenue;
    public int CustomersServedTotal => customersServedTotal;
    public int ItemsSoldTotal => itemsSoldTotal;
    public int CompletedObjectiveCount => completedObjectives.Count;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize category tracking dictionary
        foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
        {
            itemsSoldByCategory[category] = 0;
        }

        // Load all objectives from Resources if not assigned
        if (allObjectives == null || allObjectives.Length == 0)
        {
            allObjectives = Resources.LoadAll<ObjectiveDataSO>("Objectives");
            if (showDebugLogs)
                Debug.Log($"[ObjectiveManager] Loaded {allObjectives.Length} objectives from Resources/Objectives/");
        }
    }

    private void Start()
    {
        SubscribeToEvents();

        if (showDebugLogs)
            Debug.Log($"[ObjectiveManager] Initialized with {allObjectives.Length} total objectives");
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Subscribe to game events for stat tracking
    /// </summary>
    private void SubscribeToEvents()
    {
        // Revenue tracking
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyAdded += OnMoneyAdded;
        }

        // Customer & day tracking
        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded += OnDayEnded;
        }

        // Item sold tracking
        if (CheckoutCounter.Instance != null)
        {
            CheckoutCounter.Instance.OnItemSold += OnItemSold;
        }
    }

    /// <summary>
    /// Unsubscribe from game events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyAdded -= OnMoneyAdded;
        }

        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnDayEnded -= OnDayEnded;
        }

        if (CheckoutCounter.Instance != null)
        {
            CheckoutCounter.Instance.OnItemSold -= OnItemSold;
        }
    }

    #region Event Handlers

    /// <summary>
    /// Handle money earned (revenue tracking)
    /// </summary>
    private void OnMoneyAdded(int amountAdded, int newTotal)
    {
        lifetimeRevenue += amountAdded;
        EvaluateRevenueObjectives();
    }

    /// <summary>
    /// Handle day ended (customer and days played tracking)
    /// </summary>
    private void OnDayEnded(int day, int customersServed, int revenue)
    {
        customersServedTotal += customersServed;
        EvaluateCustomerObjectives();
        EvaluateDaysPlayedObjectives(day);
    }

    /// <summary>
    /// Handle item sold (item tracking)
    /// </summary>
    private void OnItemSold(ItemDataSO item, int quantity)
    {
        // Track total items sold
        itemsSoldTotal += quantity;

        // Track by category
        if (item != null)
        {
            ItemCategory category = item.itemCategory;
            if (itemsSoldByCategory.ContainsKey(category))
            {
                itemsSoldByCategory[category] += quantity;
            }
        }

        EvaluateItemSoldObjectives();
    }

    #endregion

    #region Objective Evaluation

    /// <summary>
    /// Evaluate all revenue-based objectives
    /// </summary>
    private void EvaluateRevenueObjectives()
    {
        foreach (var objective in allObjectives)
        {
            if (objective == null) continue;

            if (objective.objectiveType == ObjectiveType.Revenue)
            {
                EvaluateObjective(objective, lifetimeRevenue, objective.targetValue);
            }
            else if (objective.objectiveType == ObjectiveType.Hybrid && objective.requiredRevenue > 0)
            {
                EvaluateHybridObjective(objective);
            }
        }
    }

    /// <summary>
    /// Evaluate all customer-based objectives
    /// </summary>
    private void EvaluateCustomerObjectives()
    {
        foreach (var objective in allObjectives)
        {
            if (objective == null) continue;

            if (objective.objectiveType == ObjectiveType.CustomersServed)
            {
                EvaluateObjective(objective, customersServedTotal, objective.targetValue);
            }
            else if (objective.objectiveType == ObjectiveType.Hybrid && objective.requiredCustomers > 0)
            {
                EvaluateHybridObjective(objective);
            }
        }
    }

    /// <summary>
    /// Evaluate all item-sold objectives
    /// </summary>
    private void EvaluateItemSoldObjectives()
    {
        foreach (var objective in allObjectives)
        {
            if (objective == null) continue;

            if (objective.objectiveType == ObjectiveType.ItemsSold)
            {
                int currentCount = 0;

                // Check if category filter is set
                if (objective.categoryFilter != ItemCategory.None)
                {
                    currentCount = GetItemsSoldInCategory(objective.categoryFilter);
                }
                else
                {
                    currentCount = itemsSoldTotal;
                }

                EvaluateObjective(objective, currentCount, objective.targetValue);
            }
            else if (objective.objectiveType == ObjectiveType.Hybrid && objective.requiredItemsSold > 0)
            {
                EvaluateHybridObjective(objective);
            }
        }
    }

    /// <summary>
    /// Evaluate all days-played objectives
    /// </summary>
    private void EvaluateDaysPlayedObjectives(int currentDay)
    {
        foreach (var objective in allObjectives)
        {
            if (objective == null) continue;

            if (objective.objectiveType == ObjectiveType.DaysPlayed)
            {
                EvaluateObjective(objective, currentDay, objective.targetValue);
            }
            else if (objective.objectiveType == ObjectiveType.Hybrid && objective.requiredDays > 0)
            {
                EvaluateHybridObjective(objective);
            }
        }
    }

    /// <summary>
    /// Evaluate a single objective against current/target values
    /// </summary>
    private void EvaluateObjective(ObjectiveDataSO objective, int currentValue, int targetValue)
    {
        if (objective == null) return;

        // Skip if already completed
        if (IsObjectiveCompleted(objective)) return;

        // Check prerequisites
        if (!ArePrerequisitesMet(objective)) return;

        // Check if visible
        if (!IsObjectiveVisible(objective))
        {
            // Check if should be revealed now
            CheckRevealConditions(objective);
            return;
        }

        // Update progress
        int previousProgress = GetObjectiveCurrentValue(objective);
        if (previousProgress != currentValue)
        {
            objectiveProgress[objective.ObjectiveID] = currentValue;
            OnObjectiveProgressChanged?.Invoke(objective, currentValue, targetValue);
        }

        // Check completion
        if (currentValue >= targetValue)
        {
            CompleteObjective(objective);
        }
    }

    /// <summary>
    /// Evaluate hybrid objectives (multiple conditions)
    /// </summary>
    private void EvaluateHybridObjective(ObjectiveDataSO objective)
    {
        if (objective == null || objective.objectiveType != ObjectiveType.Hybrid) return;

        // Skip if already completed
        if (IsObjectiveCompleted(objective)) return;

        // Check prerequisites
        if (!ArePrerequisitesMet(objective)) return;

        // Check if visible
        if (!IsObjectiveVisible(objective))
        {
            CheckRevealConditions(objective);
            return;
        }

        // Check all hybrid conditions
        bool allConditionsMet = true;

        if (objective.requiredRevenue > 0 && lifetimeRevenue < objective.requiredRevenue)
            allConditionsMet = false;

        if (objective.requiredCustomers > 0 && customersServedTotal < objective.requiredCustomers)
            allConditionsMet = false;

        if (objective.requiredDays > 0 && DayManager.Instance.CurrentDay < objective.requiredDays)
            allConditionsMet = false;

        if (objective.requiredItemsSold > 0)
        {
            int itemCount = objective.hybridCategoryFilter != ItemCategory.None
                ? GetItemsSoldInCategory(objective.hybridCategoryFilter)
                : itemsSoldTotal;

            if (itemCount < objective.requiredItemsSold)
                allConditionsMet = false;
        }

        // Fire progress events (use revenue as progress indicator for hybrid)
        int progressValue = lifetimeRevenue;
        int targetValue = objective.requiredRevenue > 0 ? objective.requiredRevenue : objective.targetValue;
        OnObjectiveProgressChanged?.Invoke(objective, progressValue, targetValue);

        // Complete if all conditions met
        if (allConditionsMet)
        {
            CompleteObjective(objective);
        }
    }

    /// <summary>
    /// Check if objective should be revealed based on reveal conditions
    /// </summary>
    private void CheckRevealConditions(ObjectiveDataSO objective)
    {
        if (objective == null) return;

        bool shouldReveal = false;

        switch (objective.revealCondition)
        {
            case RevealCondition.AlwaysVisible:
                shouldReveal = true;
                break;

            case RevealCondition.AfterObjectiveCount:
                shouldReveal = completedObjectives.Count >= objective.revealThreshold;
                break;

            case RevealCondition.AfterSpecificObjective:
                if (objective.revealAfterObjective != null)
                {
                    shouldReveal = IsObjectiveCompleted(objective.revealAfterObjective);
                }
                break;
        }

        if (shouldReveal)
        {
            OnObjectiveRevealed?.Invoke(objective);
            if (showDebugLogs)
                Debug.Log($"[ObjectiveManager] Objective revealed: {objective.objectiveName}");
        }
    }

    /// <summary>
    /// Mark an objective as completed
    /// </summary>
    private void CompleteObjective(ObjectiveDataSO objective)
    {
        if (objective == null) return;

        completedObjectives.Add(objective.ObjectiveID);
        OnObjectiveCompleted?.Invoke(objective);

        if (showDebugLogs)
        {
            Debug.Log($"[ObjectiveManager] <color=green>Objective Completed:</color> {objective.objectiveName}");
            if (!string.IsNullOrEmpty(objective.darkHumorText))
                Debug.Log($"[ObjectiveManager] {objective.darkHumorText}");
        }

        // Check if this completion reveals new objectives
        CheckAllRevealConditions();
    }

    /// <summary>
    /// Check reveal conditions for all objectives (after a completion)
    /// </summary>
    private void CheckAllRevealConditions()
    {
        foreach (var objective in allObjectives)
        {
            if (objective != null && !IsObjectiveVisible(objective))
            {
                CheckRevealConditions(objective);
            }
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Check if an objective is completed
    /// </summary>
    public bool IsObjectiveCompleted(ObjectiveDataSO objective)
    {
        if (objective == null) return false;
        return completedObjectives.Contains(objective.ObjectiveID);
    }

    /// <summary>
    /// Check if an objective is visible to the player
    /// </summary>
    public bool IsObjectiveVisible(ObjectiveDataSO objective)
    {
        if (objective == null) return false;

        bool specificObjectiveCompleted = objective.revealAfterObjective != null
            ? IsObjectiveCompleted(objective.revealAfterObjective)
            : false;

        return objective.ShouldBeVisible(completedObjectives.Count, specificObjectiveCompleted);
    }

    /// <summary>
    /// Get all visible objectives
    /// </summary>
    public List<ObjectiveDataSO> GetVisibleObjectives()
    {
        return allObjectives.Where(obj => obj != null && IsObjectiveVisible(obj)).ToList();
    }

    /// <summary>
    /// Get objective progress (current value, target value, completion status)
    /// </summary>
    public (int current, int target, bool complete) GetObjectiveProgress(ObjectiveDataSO objective)
    {
        if (objective == null)
            return (0, 0, false);

        bool complete = IsObjectiveCompleted(objective);
        int current = GetObjectiveCurrentValue(objective);
        int target = GetObjectiveTargetValue(objective);

        return (current, target, complete);
    }

    /// <summary>
    /// Get current progress value for an objective
    /// </summary>
    private int GetObjectiveCurrentValue(ObjectiveDataSO objective)
    {
        if (objective == null) return 0;

        switch (objective.objectiveType)
        {
            case ObjectiveType.Revenue:
                return lifetimeRevenue;

            case ObjectiveType.CustomersServed:
                return customersServedTotal;

            case ObjectiveType.ItemsSold:
                if (objective.categoryFilter != ItemCategory.None)
                    return GetItemsSoldInCategory(objective.categoryFilter);
                else
                    return itemsSoldTotal;

            case ObjectiveType.DaysPlayed:
                return DayManager.Instance != null ? DayManager.Instance.CurrentDay : 0;

            case ObjectiveType.Hybrid:
                // Return revenue as primary progress indicator
                return lifetimeRevenue;

            default:
                return 0;
        }
    }

    /// <summary>
    /// Get target value for an objective
    /// </summary>
    private int GetObjectiveTargetValue(ObjectiveDataSO objective)
    {
        if (objective == null) return 0;

        if (objective.objectiveType == ObjectiveType.Hybrid)
        {
            // Return revenue requirement as primary target for hybrid
            return objective.requiredRevenue > 0 ? objective.requiredRevenue : objective.targetValue;
        }

        return objective.targetValue;
    }

    /// <summary>
    /// Get items sold in a specific category
    /// </summary>
    public int GetItemsSoldInCategory(ItemCategory category)
    {
        return itemsSoldByCategory.ContainsKey(category) ? itemsSoldByCategory[category] : 0;
    }

    /// <summary>
    /// Check if an objective's prerequisites are met
    /// </summary>
    private bool ArePrerequisitesMet(ObjectiveDataSO objective)
    {
        if (objective == null) return false;

        // Check prerequisite objectives
        bool objectivePrereqsMet = objective.ArePrerequisiteObjectivesMet(IsObjectiveCompleted);

        // Check prerequisite upgrades
        bool upgradePrereqsMet = objective.ArePrerequisiteUpgradesMet(
            upgrade => UpgradeManager.Instance != null && UpgradeManager.Instance.HasUpgrade(upgrade.upgradeID)
        );

        return objectivePrereqsMet && upgradePrereqsMet;
    }

    #endregion

    #region Debug Methods

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// DEBUG: Force complete an objective (for testing)
    /// </summary>
    public void DebugCompleteObjective(ObjectiveDataSO objective)
    {
        if (objective == null) return;

        if (IsObjectiveCompleted(objective))
        {
            Debug.LogWarning($"[ObjectiveManager] Objective already completed: {objective.objectiveName}");
            return;
        }

        CompleteObjective(objective);
    }

    /// <summary>
    /// DEBUG: Add items sold to a random category
    /// </summary>
    public void DebugAddItemsSold(int count)
    {
        // Pick a random category
        var categories = System.Enum.GetValues(typeof(ItemCategory));
        ItemCategory randomCategory = (ItemCategory)categories.GetValue(UnityEngine.Random.Range(0, categories.Length));

        itemsSoldByCategory[randomCategory] += count;
        itemsSoldTotal += count;

        Debug.Log($"[ObjectiveManager] DEBUG: Added {count} items to {randomCategory} category");
        EvaluateItemSoldObjectives();
    }
#endif

    #endregion
}
