using UnityEngine;

/// <summary>
/// ScriptableObject defining customer type characteristics and behavior.
/// Based on GDD customer archetypes: Quick Shoppers, Browsers, Big Spenders.
/// </summary>
[CreateAssetMenu(fileName = "New Customer Type", menuName = "DungeonMart/Customer Type")]
public class CustomerTypeDataSO : ScriptableObject {
    [Header("Customer Identity")]
    public string customerTypeName;

    [Tooltip("Archetype: Quick Shopper, Browser, Big Spender, etc.")]
    public string archetype;

    [Header("Behavior Parameters")]
    [Tooltip("Movement speed in units per second")]
    public float moveSpeed = 3f;

    [Tooltip("Time spent browsing at each shelf in seconds")]
    public float browseTime = 1f;

    [Tooltip("Minimum number of items this customer wants to buy")]
    public int minItemCount = 1;

    [Tooltip("Maximum number of items this customer wants to buy")]
    public int maxItemCount = 1;

    [Tooltip("Initial patience value (decreases while waiting)")]
    public float initialPatience = 100f;

    [Tooltip("Patience drain rate per second while waiting")]
    public float patienceDrainRate = 5f;

    [Header("Demand System")]
    [Tooltip("Ratio of basket filled with specific trending items (0.0 = all random, 1.0 = all specific). Quick Shopper = 1.0, Browser = 0.3, Big Spender = 0.6")]
    [Range(0f, 1f)]
    public float specificItemRatio = 0.5f;

    [Header("Corporate Humor Dialogues")]
    [Tooltip("Dialogue shown when entering the store")]
    [TextArea(2, 3)]
    public string[] entryDialogues;

    [Tooltip("Dialogue shown while browsing shelves")]
    [TextArea(2, 3)]
    public string[] browsingDialogues;

    [Tooltip("Dialogue shown at checkout")]
    [TextArea(2, 3)]
    public string[] checkoutDialogues;

    [Tooltip("Dialogue shown when leaving satisfied")]
    [TextArea(2, 3)]
    public string[] exitDialogues;

    [Tooltip("Dialogue shown when leaving disappointed (no items)")]
    [TextArea(2, 3)]
    public string[] disappointedDialogues;

    /// <summary>
    /// Gets a random item count within the defined range.
    /// </summary>
    public int GetRandomItemCount() {
        return Random.Range(minItemCount, maxItemCount + 1);
    }

    /// <summary>
    /// Gets a random dialogue from the specified category.
    /// </summary>
    public string GetRandomDialogue(string[] dialogues) {
        if (dialogues == null || dialogues.Length == 0) {
            return "";
        }

        return dialogues[Random.Range(0, dialogues.Length)];
    }
}