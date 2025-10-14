using UnityEngine;

/// <summary>
/// Defines a progression tier with revenue requirements and unlock rewards.
/// Used to track player's business growth from Street Vendor to Tycoon.
/// </summary>
[CreateAssetMenu(fileName = "New Progression Tier", menuName = "DungeonMart/Progression Tier")]
public class ProgressionDataSO : ScriptableObject
{
    [Header("Tier Identity")]
    [Tooltip("Display name of this tier (e.g., 'Street Vendor', 'Shop Owner')")]
    public string tierName;

    [Tooltip("Tier index (0 = starting tier, 1 = first upgrade, etc.)")]
    public int tierIndex;

    [Tooltip("Icon representing this tier (crown, star, diamond, etc.)")]
    public Sprite tierIcon;

    [Header("Requirements")]
    [Tooltip("Total lifetime revenue required to reach this tier")]
    public int requiredLifetimeRevenue;

    [Header("Rewards & Description")]
    [Tooltip("Flavor text describing this tier's status")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("List of upgrades that unlock when reaching this tier")]
    public UpgradeDataSO[] unlockedUpgrades;

    /// <summary>
    /// Checks if the player has reached the revenue requirement for this tier.
    /// </summary>
    public bool IsUnlocked(int currentLifetimeRevenue)
    {
        return currentLifetimeRevenue >= requiredLifetimeRevenue;
    }
}
