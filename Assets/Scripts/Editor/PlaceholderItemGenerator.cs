using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Unity Editor tool to generate placeholder item prefabs (color-coded primitives).
/// Creates simple cube/cylinder prefabs for rapid prototyping without 3D models.
/// </summary>
public class PlaceholderItemGenerator : EditorWindow {
    private const string PREFABS_FOLDER = "Assets/Prefabs/Items";
    private const string MATERIALS_FOLDER = "Assets/Materials/ItemPlaceholders";

    // Color-coded by category
    private static readonly Color WEAPONS_COLOR = new Color(1f, 0.27f, 0.27f);       // Red #FF4444
    private static readonly Color SHIELDS_COLOR = new Color(0.27f, 0.27f, 1f);       // Blue #4444FF
    private static readonly Color POTIONS_COLOR = new Color(0.27f, 1f, 0.27f);       // Green #44FF44
    private static readonly Color ARMOR_COLOR = new Color(1f, 0.53f, 0.27f);         // Orange #FF8844
    private static readonly Color TRAPS_COLOR = new Color(0.67f, 0.27f, 1f);         // Purple #AA44FF
    private static readonly Color MAGIC_COLOR = new Color(0.27f, 1f, 1f);            // Cyan #44FFFF

    [MenuItem("Tools/DungeonMart/Generate Placeholder Item Prefabs")]
    private static void ShowWindow() {
        var window = GetWindow<PlaceholderItemGenerator>("Placeholder Item Generator");
        window.minSize = new Vector2(400, 200);
        window.Show();
    }

    private void OnGUI() {
        GUILayout.Label("Placeholder Item Prefab Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool generates 35 placeholder item prefabs from the CSV file.\n" +
            "Each prefab will be a color-coded primitive shape with the Item component attached.\n\n" +
            "Colors:\n" +
            "• Weapons: Red\n" +
            "• Shields: Blue\n" +
            "• Potions: Green (cylinders)\n" +
            "• Armor & Apparel: Orange\n" +
            "• Traps: Purple\n" +
            "• Magic Items: Cyan",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate All Placeholder Prefabs", GUILayout.Height(30))) {
            GeneratePlaceholders();
        }
    }

    private static void GeneratePlaceholders() {
        // Ensure folders exist
        if (!AssetDatabase.IsValidFolder(PREFABS_FOLDER)) {
            Directory.CreateDirectory(PREFABS_FOLDER);
        }
        if (!AssetDatabase.IsValidFolder(MATERIALS_FOLDER)) {
            Directory.CreateDirectory(MATERIALS_FOLDER);
        }
        AssetDatabase.Refresh();

        var itemDefinitions = GetItemDefinitions();
        int createdCount = 0;
        int skippedCount = 0;

        EditorUtility.DisplayProgressBar("Generating Placeholders", "Creating prefabs...", 0f);

        try {
            for (int i = 0; i < itemDefinitions.Count; i++) {
                var item = itemDefinitions[i];
                float progress = (float)i / itemDefinitions.Count;
                EditorUtility.DisplayProgressBar("Generating Placeholders", $"Creating {item.prefabName}...", progress);

                string prefabPath = $"{PREFABS_FOLDER}/{item.prefabName}.prefab";

                // Skip if prefab already exists
                if (File.Exists(prefabPath)) {
                    Debug.Log($"Skipped (already exists): {item.prefabName}");
                    skippedCount++;
                    continue;
                }

                // Create placeholder prefab
                CreatePlaceholderPrefab(item);
                createdCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(
                "Generation Complete",
                $"Placeholder prefabs created!\n\nCreated: {createdCount}\nSkipped (existing): {skippedCount}",
                "OK"
            );
        } catch (System.Exception e) {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Generation Error", e.Message, "OK");
            Debug.LogError($"Error generating placeholders: {e.Message}\n{e.StackTrace}");
        }
    }

    private static void CreatePlaceholderPrefab(ItemDefinition item) {
        // Determine primitive type and color based on category
        PrimitiveType primitiveType = item.category == ItemCategory.Potions ? PrimitiveType.Cylinder : PrimitiveType.Cube;
        Color color = GetColorForCategory(item.category);

        // Create GameObject with primitive mesh
        GameObject obj = GameObject.CreatePrimitive(primitiveType);
        obj.name = item.prefabName;

        // Scale appropriately (smaller for items on shelves)
        obj.transform.localScale = Vector3.one * 0.3f;

        // Create or get material
        Material mat = GetOrCreateMaterial(item.category, color);
        obj.GetComponent<MeshRenderer>().material = mat;

        // Remove default collider (Item component will add its own if needed)
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null) {
            Object.DestroyImmediate(collider);
        }

        // Add Item component
        Item itemComponent = obj.AddComponent<Item>();
        // Note: ItemDataSO will be assigned when the item is instantiated by the game systems

        // Save as prefab
        string prefabPath = $"{PREFABS_FOLDER}/{item.prefabName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(obj, prefabPath);

        // Clean up scene object
        Object.DestroyImmediate(obj);

        Debug.Log($"Created placeholder prefab: {item.prefabName} ({item.category})");
    }

    private static Material GetOrCreateMaterial(ItemCategory category, Color color) {
        string materialName = $"{category}Material";
        string materialPath = $"{MATERIALS_FOLDER}/{materialName}.mat";

        // Try to load existing material
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (mat == null) {
            // Create new material
            mat = new Material(Shader.Find("Standard"));
            mat.name = materialName;
            mat.color = color;

            // Make it slightly emissive for better visibility
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.2f);

            AssetDatabase.CreateAsset(mat, materialPath);
            Debug.Log($"Created material: {materialName}");
        }

        return mat;
    }

    private static Color GetColorForCategory(ItemCategory category) {
        switch (category) {
            case ItemCategory.Weapons: return WEAPONS_COLOR;
            case ItemCategory.Shields: return SHIELDS_COLOR;
            case ItemCategory.Potions: return POTIONS_COLOR;
            case ItemCategory.ArmorApparel: return ARMOR_COLOR;
            case ItemCategory.Traps: return TRAPS_COLOR;
            case ItemCategory.MagicItems: return MAGIC_COLOR;
            default: return Color.white;
        }
    }

    private static List<ItemDefinition> GetItemDefinitions() {
        // Hardcoded list matching CSV (35 items)
        return new List<ItemDefinition> {
            // Weapons (7)
            new ItemDefinition("RustyDagger", ItemCategory.Weapons),
            new ItemDefinition("WoodenStaff", ItemCategory.Weapons),
            new ItemDefinition("IronSword", ItemCategory.Weapons),
            new ItemDefinition("SteelAxe", ItemCategory.Weapons),
            new ItemDefinition("Crossbow", ItemCategory.Weapons),
            new ItemDefinition("EnchantedBlade", ItemCategory.Weapons),
            new ItemDefinition("LegendaryWarhammer", ItemCategory.Weapons),

            // Shields (5)
            new ItemDefinition("WoodenBuckler", ItemCategory.Shields),
            new ItemDefinition("IronShield", ItemCategory.Shields),
            new ItemDefinition("TowerShield", ItemCategory.Shields),
            new ItemDefinition("EnchantedShield", ItemCategory.Shields),
            new ItemDefinition("DragonScaleShield", ItemCategory.Shields),

            // Potions (6)
            new ItemDefinition("MinorHealthPotion", ItemCategory.Potions),
            new ItemDefinition("MinorManaPotion", ItemCategory.Potions),
            new ItemDefinition("HealthPotion", ItemCategory.Potions),
            new ItemDefinition("ManaPotion", ItemCategory.Potions),
            new ItemDefinition("GreaterHealthPotion", ItemCategory.Potions),
            new ItemDefinition("ElixirOfPower", ItemCategory.Potions),

            // Armor & Apparel (6)
            new ItemDefinition("LeatherTunic", ItemCategory.ArmorApparel),
            new ItemDefinition("IronHelmet", ItemCategory.ArmorApparel),
            new ItemDefinition("ChainmailVest", ItemCategory.ArmorApparel),
            new ItemDefinition("PlateArmor", ItemCategory.ArmorApparel),
            new ItemDefinition("EnchantedRobes", ItemCategory.ArmorApparel),
            new ItemDefinition("CrownOfKings", ItemCategory.ArmorApparel),

            // Traps (5)
            new ItemDefinition("SpikeTrap", ItemCategory.Traps),
            new ItemDefinition("NetTrap", ItemCategory.Traps),
            new ItemDefinition("BearTrap", ItemCategory.Traps),
            new ItemDefinition("PoisonGasTrap", ItemCategory.Traps),
            new ItemDefinition("DeathTrap", ItemCategory.Traps),

            // Magic Items (6)
            new ItemDefinition("ScrollOfFireball", ItemCategory.MagicItems),
            new ItemDefinition("RingOfProtection", ItemCategory.MagicItems),
            new ItemDefinition("AmuletOfLife", ItemCategory.MagicItems),
            new ItemDefinition("CrystalBall", ItemCategory.MagicItems),
            new ItemDefinition("StaffOfPower", ItemCategory.MagicItems),
            new ItemDefinition("DragonEgg", ItemCategory.MagicItems)
        };
    }

    private struct ItemDefinition {
        public string prefabName;
        public ItemCategory category;

        public ItemDefinition(string prefabName, ItemCategory category) {
            this.prefabName = prefabName;
            this.category = category;
        }
    }
}
