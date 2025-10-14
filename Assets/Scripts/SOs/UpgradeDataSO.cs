using UnityEngine;

/// <summary>
/// Defines a shop upgrade that players can purchase to improve their business.
/// PLACEHOLDER: Will be fully implemented in Phase 1.2.
/// </summary>
[CreateAssetMenu(fileName = "New Upgrade", menuName = "DungeonMart/Upgrade")]
public class UpgradeDataSO : ScriptableObject
{
    [Header("Upgrade Identity")]
    public string upgradeName;

    [TextArea(2, 3)]
    public string description;

    public Sprite upgradeIcon;

    [Header("Cost & Requirements")]
    public int cost;
    public int tierRequirement;

    // Additional fields will be added in Phase 1.2
}
