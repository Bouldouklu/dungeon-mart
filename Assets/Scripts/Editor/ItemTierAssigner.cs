using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Unity Editor tool to automatically assign quality tiers to items based on sell price.
/// Tier 1 (Cheap): $1-29
/// Tier 2 (Normal): $30-79
/// Tier 3 (Premium): $80+
/// </summary>
public class ItemTierAssigner : EditorWindow {
    private const string ITEMS_FOLDER = "Assets/Resources/Items";
    private const int TIER_1_MAX = 29;
    private const int TIER_2_MAX = 79;

    private Vector2 scrollPosition;
    private string lastAssignmentLog = "";
    private bool autoSave = true;

    [MenuItem("Tools/DungeonMart/Assign Item Tiers")]
    private static void ShowWindow() {
        var window = GetWindow<ItemTierAssigner>("Item Tier Assigner");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }

    private void OnGUI() {
        GUILayout.Label("Item Tier Assigner", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool automatically assigns quality tiers to all items based on their sell price:\n\n" +
            "• Tier 1 (Cheap/Starting): $1-29\n" +
            "• Tier 2 (Normal/Mid-game): $30-79\n" +
            "• Tier 3 (Premium/Late-game): $80+\n\n" +
            "All ItemDataSO assets in Resources/Items/ will be updated.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        autoSave = EditorGUILayout.Toggle("Auto-save after assignment", autoSave);

        EditorGUILayout.Space();

        if (GUILayout.Button("Assign Tiers to All Items", GUILayout.Height(30))) {
            AssignTiersToAllItems();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Assignment Log:", EditorStyles.boldLabel);

        // Scrollable log area
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        EditorGUILayout.TextArea(lastAssignmentLog, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void AssignTiersToAllItems() {
        lastAssignmentLog = "Starting tier assignment...\n\n";

        // Find all ItemDataSO assets
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { ITEMS_FOLDER });

        if (guids.Length == 0) {
            lastAssignmentLog += $"ERROR: No ItemDataSO assets found in {ITEMS_FOLDER}\n";
            return;
        }

        lastAssignmentLog += $"Found {guids.Length} items to process.\n\n";

        int tier1Count = 0;
        int tier2Count = 0;
        int tier3Count = 0;
        int updatedCount = 0;
        int unchangedCount = 0;

        var itemsByTier = new Dictionary<int, List<string>> {
            { 1, new List<string>() },
            { 2, new List<string>() },
            { 3, new List<string>() }
        };

        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDataSO item = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);

            if (item == null) {
                lastAssignmentLog += $"WARNING: Could not load asset at {path}\n";
                continue;
            }

            // Calculate tier based on sell price
            int oldTier = item.tier;
            int newTier = CalculateTier(item.sellPrice);

            item.tier = newTier;

            // Track statistics
            if (oldTier != newTier) {
                updatedCount++;
                lastAssignmentLog += $"UPDATED: {item.itemName} (${item.sellPrice}) - Tier {oldTier} → Tier {newTier}\n";
            } else {
                unchangedCount++;
            }

            // Count tiers
            switch (newTier) {
                case 1: tier1Count++; break;
                case 2: tier2Count++; break;
                case 3: tier3Count++; break;
            }

            itemsByTier[newTier].Add($"{item.itemName} (${item.sellPrice})");

            // Mark as dirty and save
            EditorUtility.SetDirty(item);
        }

        if (autoSave) {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // Summary
        lastAssignmentLog += "\n" + new string('-', 50) + "\n";
        lastAssignmentLog += "ASSIGNMENT COMPLETE\n";
        lastAssignmentLog += new string('-', 50) + "\n\n";
        lastAssignmentLog += $"Total Items Processed: {guids.Length}\n";
        lastAssignmentLog += $"Updated: {updatedCount}\n";
        lastAssignmentLog += $"Unchanged: {unchangedCount}\n\n";

        lastAssignmentLog += "Tier Distribution:\n";
        lastAssignmentLog += $"• Tier 1 (Cheap): {tier1Count} items\n";
        lastAssignmentLog += $"• Tier 2 (Normal): {tier2Count} items\n";
        lastAssignmentLog += $"• Tier 3 (Premium): {tier3Count} items\n\n";

        // Show items by tier
        foreach (var tierGroup in itemsByTier.OrderBy(x => x.Key)) {
            lastAssignmentLog += $"\nTier {tierGroup.Key} Items:\n";
            foreach (var itemName in tierGroup.Value.OrderBy(x => x)) {
                lastAssignmentLog += $"  • {itemName}\n";
            }
        }

        if (autoSave) {
            lastAssignmentLog += "\n✓ Changes saved to disk.\n";
        } else {
            lastAssignmentLog += "\n⚠ Changes NOT saved. Enable auto-save or save manually.\n";
        }

        Debug.Log($"Item tier assignment complete! {updatedCount} items updated, {unchangedCount} unchanged. " +
                  $"Distribution: T1={tier1Count}, T2={tier2Count}, T3={tier3Count}");
    }

    private int CalculateTier(int sellPrice) {
        if (sellPrice <= TIER_1_MAX) return 1;
        if (sellPrice <= TIER_2_MAX) return 2;
        return 3;
    }
}
