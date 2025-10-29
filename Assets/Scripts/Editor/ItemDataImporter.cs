using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Unity Editor tool to import/update ItemDataSO assets from CSV file.
/// Preserves existing sprites while updating item data.
/// </summary>
public class ItemDataImporter : EditorWindow {
    private const string CSV_FILE_PATH = "Assets/DungeonMart_Economy_Balance.csv";
    private const string ITEMS_FOLDER = "Assets/Resources/Items";

    private string csvPath = CSV_FILE_PATH;
    private string outputFolder = ITEMS_FOLDER;
    private Vector2 scrollPosition;
    private string lastImportLog = "";

    [MenuItem("Tools/DungeonMart/Import Items from CSV")]
    private static void ShowWindow() {
        var window = GetWindow<ItemDataImporter>("Item Data Importer");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnGUI() {
        GUILayout.Label("Item Data Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool imports item data from a CSV file and creates/updates ItemDataSO assets.\n" +
            "Existing sprites will be preserved during updates.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // CSV file path
        EditorGUILayout.LabelField("CSV File Path:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        csvPath = EditorGUILayout.TextField(csvPath);
        if (GUILayout.Button("Browse", GUILayout.Width(70))) {
            string path = EditorUtility.OpenFilePanel("Select CSV File", "Assets", "csv");
            if (!string.IsNullOrEmpty(path)) {
                csvPath = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Output folder
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Output Folder:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField(outputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(70))) {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path)) {
                outputFolder = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Import button
        GUI.enabled = File.Exists(csvPath);
        if (GUILayout.Button("Import Items", GUILayout.Height(30))) {
            ImportItems();
        }
        GUI.enabled = true;

        // Display log
        if (!string.IsNullOrEmpty(lastImportLog)) {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Last Import Log:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            EditorGUILayout.TextArea(lastImportLog, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    private void ImportItems() {
        if (!File.Exists(csvPath)) {
            EditorUtility.DisplayDialog("Error", $"CSV file not found at: {csvPath}", "OK");
            return;
        }

        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder(outputFolder)) {
            Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }

        try {
            var items = ParseCSV(csvPath);
            int createdCount = 0;
            int updatedCount = 0;
            int errorCount = 0;
            var log = new System.Text.StringBuilder();

            log.AppendLine($"Starting import from: {csvPath}");
            log.AppendLine($"Output folder: {outputFolder}");
            log.AppendLine($"Found {items.Count} items in CSV\n");

            foreach (var itemData in items) {
                try {
                    string fileName = ConvertToPascalCase(itemData.itemName);
                    string assetPath = $"{outputFolder}/{fileName}.asset";

                    ItemDataSO existingAsset = AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath);

                    if (existingAsset != null) {
                        // Update existing asset, preserve sprite
                        Sprite existingSprite = existingAsset.itemSprite;
                        UpdateItemData(existingAsset, itemData);
                        existingAsset.itemSprite = existingSprite; // Preserve sprite

                        EditorUtility.SetDirty(existingAsset);
                        log.AppendLine($"✓ UPDATED: {itemData.itemName} ({fileName}.asset)");
                        updatedCount++;
                    } else {
                        // Create new asset
                        ItemDataSO newAsset = CreateInstance<ItemDataSO>();
                        UpdateItemData(newAsset, itemData);

                        AssetDatabase.CreateAsset(newAsset, assetPath);
                        log.AppendLine($"✓ CREATED: {itemData.itemName} ({fileName}.asset)");
                        createdCount++;
                    }
                } catch (System.Exception e) {
                    log.AppendLine($"✗ ERROR processing {itemData.itemName}: {e.Message}");
                    errorCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            log.AppendLine($"\n--- Import Complete ---");
            log.AppendLine($"Created: {createdCount}");
            log.AppendLine($"Updated: {updatedCount}");
            log.AppendLine($"Errors: {errorCount}");

            lastImportLog = log.ToString();

            EditorUtility.DisplayDialog(
                "Import Complete",
                $"Import finished!\n\nCreated: {createdCount}\nUpdated: {updatedCount}\nErrors: {errorCount}",
                "OK"
            );
        } catch (System.Exception e) {
            lastImportLog = $"ERROR: {e.Message}\n{e.StackTrace}";
            EditorUtility.DisplayDialog("Import Error", e.Message, "OK");
        }
    }

    private List<ItemDataCSV> ParseCSV(string filePath) {
        var items = new List<ItemDataCSV>();
        var lines = File.ReadAllLines(filePath);

        if (lines.Length < 2) {
            throw new System.Exception("CSV file is empty or has no data rows");
        }

        // Parse header to find column indices
        string[] headers = lines[0].Split(',');
        int nameIdx = FindColumnIndex(headers, "ItemName", "Name", "Item Name");
        int sellPriceIdx = FindColumnIndex(headers, "SellPrice", "Sell Price");
        int restockCostIdx = FindColumnIndex(headers, "RestockCost", "Restock Cost");
        int categoryIdx = FindColumnIndex(headers, "ItemCategory", "Category");
        int tierIdx = FindColumnIndex(headers, "RequiredTier", "Tier", "Required Tier");
        int unlockedIdx = FindColumnIndex(headers, "IsUnlockedByDefault", "Unlocked By Default", "Default Unlocked");

        // Parse data rows (skip header)
        for (int i = 1; i < lines.Length; i++) {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            try {
                var item = new ItemDataCSV {
                    itemName = GetValue(values, nameIdx).Trim(),
                    sellPrice = ParseInt(GetValue(values, sellPriceIdx), 10),
                    restockCost = ParseInt(GetValue(values, restockCostIdx), 5),
                    itemCategory = ParseItemCategory(GetValue(values, categoryIdx)),
                    requiredTier = ParseInt(GetValue(values, tierIdx), 0),
                    isUnlockedByDefault = ParseBool(GetValue(values, unlockedIdx), false)
                };

                if (!string.IsNullOrEmpty(item.itemName)) {
                    items.Add(item);
                }
            } catch (System.Exception e) {
                Debug.LogWarning($"Error parsing row {i + 1}: {e.Message}");
            }
        }

        return items;
    }

    private int FindColumnIndex(string[] headers, params string[] possibleNames) {
        for (int i = 0; i < headers.Length; i++) {
            string header = headers[i].Trim();
            foreach (string name in possibleNames) {
                if (header.Equals(name, System.StringComparison.OrdinalIgnoreCase)) {
                    return i;
                }
            }
        }
        return -1;
    }

    private string GetValue(string[] values, int index) {
        if (index < 0 || index >= values.Length) return "";
        return values[index].Trim();
    }

    private int ParseInt(string value, int defaultValue) {
        if (int.TryParse(value, out int result)) {
            return result;
        }
        return defaultValue;
    }

    private bool ParseBool(string value, bool defaultValue) {
        if (string.IsNullOrEmpty(value)) return defaultValue;

        value = value.Trim().ToLower();
        if (value == "true" || value == "1" || value == "yes") return true;
        if (value == "false" || value == "0" || value == "no") return false;

        return defaultValue;
    }

    private ItemCategory ParseItemCategory(string value) {
        if (string.IsNullOrEmpty(value)) return ItemCategory.Weapons;

        value = value.Trim();
        if (System.Enum.TryParse<ItemCategory>(value, true, out ItemCategory result)) {
            return result;
        }
        return ItemCategory.Weapons;
    }

    private void UpdateItemData(ItemDataSO asset, ItemDataCSV data) {
        asset.itemName = data.itemName;
        asset.sellPrice = data.sellPrice;
        asset.restockCost = data.restockCost;
        asset.itemCategory = data.itemCategory;
        asset.requiredTier = data.requiredTier;
        asset.isUnlockedByDefault = data.isUnlockedByDefault;
    }

    private string ConvertToPascalCase(string text) {
        if (string.IsNullOrEmpty(text)) return text;

        // Remove special characters and split by spaces
        text = Regex.Replace(text, @"[^a-zA-Z0-9\s]", "");
        string[] words = text.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Capitalize first letter of each word and concatenate
        return string.Join("", words.Select(w =>
            char.ToUpper(w[0]) + w.Substring(1).ToLower()
        ));
    }

    private struct ItemDataCSV {
        public string itemName;
        public int sellPrice;
        public int restockCost;
        public ItemCategory itemCategory;
        public int requiredTier;
        public bool isUnlockedByDefault;
    }
}
