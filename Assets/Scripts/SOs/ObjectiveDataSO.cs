using UnityEngine;

/// <summary>
/// Defines the type of objective for tracking progress
/// </summary>
public enum ObjectiveType
{
    Revenue,           // Track lifetime revenue earned
    CustomersServed,   // Track total customers served
    ItemsSold,         // Track items sold (all or specific category)
    DaysPlayed,        // Track number of days survived
    Hybrid             // Multiple conditions must be met simultaneously
}

/// <summary>
/// Defines when an objective should be revealed to the player
/// </summary>
public enum RevealCondition
{
    AlwaysVisible,              // Visible from game start
    AfterObjectiveCount,        // Reveal after X objectives completed
    AfterSpecificObjective      // Reveal after specific objective completed
}

/// <summary>
/// ScriptableObject that defines a single objective in the progression system
/// </summary>
[CreateAssetMenu(fileName = "NewObjective", menuName = "DungeonMart/Objective")]
public class ObjectiveDataSO : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Display name of the objective")]
    public string objectiveName = "New Objective";

    [Tooltip("Unique identifier (auto-generated from name)")]
    [SerializeField] private string objectiveID;

    [Tooltip("Description shown to player")]
    [TextArea(2, 4)]
    public string description = "Complete this objective to unlock rewards.";

    [Tooltip("Icon displayed on objective card")]
    public Sprite objectiveIcon;

    [Header("Objective Requirements")]
    [Tooltip("Type of objective to track")]
    public ObjectiveType objectiveType = ObjectiveType.Revenue;

    [Tooltip("Target value to reach (money, customers, items, days)")]
    public int targetValue = 100;

    [Tooltip("For ItemsSold objectives: filter by specific category (set to None for all items)")]
    public ItemCategory categoryFilter = ItemCategory.None;

    [Header("Hybrid Objective (Multiple Conditions)")]
    [Tooltip("For Hybrid type: Required revenue amount")]
    public int requiredRevenue = 0;

    [Tooltip("For Hybrid type: Required customers served")]
    public int requiredCustomers = 0;

    [Tooltip("For Hybrid type: Required items sold")]
    public int requiredItemsSold = 0;

    [Tooltip("For Hybrid type: Required days played")]
    public int requiredDays = 0;

    [Tooltip("For Hybrid type: Filter items by category (set to None for all items)")]
    public ItemCategory hybridCategoryFilter = ItemCategory.None;

    [Header("Prerequisites")]
    [Tooltip("Objectives that must be completed first")]
    public ObjectiveDataSO[] prerequisiteObjectives;

    [Tooltip("Upgrades that must be owned first")]
    public UpgradeDataSO[] prerequisiteUpgrades;

    [Header("Reveal Conditions")]
    [Tooltip("When should this objective be revealed to the player?")]
    public RevealCondition revealCondition = RevealCondition.AlwaysVisible;

    [Tooltip("For AfterObjectiveCount: Number of objectives to complete before revealing")]
    public int revealThreshold = 0;

    [Tooltip("For AfterSpecificObjective: Specific objective that must complete first")]
    public ObjectiveDataSO revealAfterObjective;

    [Header("Rewards")]
    [Tooltip("Upgrade unlocked when this objective is completed")]
    public UpgradeDataSO unlocksUpgrade;

    [Tooltip("Item tier unlocked when completed (0 = no tier unlock, 1-3 = unlocks that tier)")]
    [Range(0, 3)]
    public int unlocksTier = 0;

    [Header("Flavor")]
    [Tooltip("Dark humor text shown on objective completion")]
    [TextArea(2, 3)]
    public string darkHumorText = "Capitalism prevails! Your exploitation skills are improving.";

    /// <summary>
    /// Unique identifier for this objective (auto-generated)
    /// </summary>
    public string ObjectiveID
    {
        get
        {
            if (string.IsNullOrEmpty(objectiveID))
            {
                GenerateID();
            }
            return objectiveID;
        }
    }

    /// <summary>
    /// Generate unique ID from objective name (PascalCase, no spaces)
    /// </summary>
    private void GenerateID()
    {
        if (string.IsNullOrEmpty(objectiveName)) return;

        // Remove spaces and special characters, convert to PascalCase
        objectiveID = objectiveName.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("'", "");
    }

    /// <summary>
    /// Validate ID on asset creation/modification
    /// </summary>
    private void OnValidate()
    {
        GenerateID();
    }

    /// <summary>
    /// Check if this objective should be visible based on reveal conditions
    /// </summary>
    public bool ShouldBeVisible(int completedObjectivesCount, bool specificObjectiveCompleted)
    {
        switch (revealCondition)
        {
            case RevealCondition.AlwaysVisible:
                return true;

            case RevealCondition.AfterObjectiveCount:
                return completedObjectivesCount >= revealThreshold;

            case RevealCondition.AfterSpecificObjective:
                return specificObjectiveCompleted;

            default:
                return false;
        }
    }

    /// <summary>
    /// Check if all prerequisite objectives are completed
    /// </summary>
    public bool ArePrerequisiteObjectivesMet(System.Func<ObjectiveDataSO, bool> isObjectiveCompleted)
    {
        if (prerequisiteObjectives == null || prerequisiteObjectives.Length == 0)
            return true;

        foreach (var prerequisite in prerequisiteObjectives)
        {
            if (prerequisite != null && !isObjectiveCompleted(prerequisite))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Check if all prerequisite upgrades are owned
    /// </summary>
    public bool ArePrerequisiteUpgradesMet(System.Func<UpgradeDataSO, bool> isUpgradeOwned)
    {
        if (prerequisiteUpgrades == null || prerequisiteUpgrades.Length == 0)
            return true;

        foreach (var prerequisite in prerequisiteUpgrades)
        {
            if (prerequisite != null && !isUpgradeOwned(prerequisite))
                return false;
        }

        return true;
    }
}
