using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor tool for exporting 256x256 PNG thumbnail icons from item prefabs.
/// Renders prefabs with isometric camera view and transparent background.
/// Access via Tools → DungeonMart → Export Item Thumbnails
/// </summary>
public class ThumbnailExporter : EditorWindow
{
    private const int THUMBNAIL_SIZE = 256;
    private const string OUTPUT_FOLDER = "Assets/UI/ItemIcons";
    private const string PREFAB_SEARCH_FOLDER = "Assets/Prefabs/Items";

    [Header("Export Settings")]
    private bool autoLinkSprites = true;
    private float cameraDistance = 3f;
    private Vector3 cameraRotation = new Vector3(30f, 45f, 0f);
    private Color backgroundColor = new Color(0, 0, 0, 0); // Transparent

    [Header("Lighting Settings")]
    private float mainLightIntensity = 1f;
    private float fillLightIntensity = 0.4f;
    private float rimLightIntensity = 0.3f;

    private List<GameObject> itemPrefabs = new List<GameObject>();
    private bool isExporting = false;
    private Vector2 scrollPosition;

    [MenuItem("Tools/DungeonMart/Export Item Thumbnails")]
    public static void ShowWindow()
    {
        ThumbnailExporter window = GetWindow<ThumbnailExporter>("Thumbnail Exporter");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshPrefabList();
    }

    private void OnGUI()
    {
        GUILayout.Label("Item Thumbnail Exporter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Export Settings
        EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
        autoLinkSprites = EditorGUILayout.Toggle("Auto-Link Sprites to ItemDataSO", autoLinkSprites);
        EditorGUILayout.HelpBox($"Output: {OUTPUT_FOLDER}/[ItemName].png (256x256 PNG)", MessageType.Info);
        EditorGUILayout.Space();

        // Camera Settings
        EditorGUILayout.LabelField("Camera Settings", EditorStyles.boldLabel);
        cameraDistance = EditorGUILayout.Slider("Camera Distance", cameraDistance, 1f, 10f);
        cameraRotation = EditorGUILayout.Vector3Field("Camera Rotation", cameraRotation);
        EditorGUILayout.Space();

        // Lighting Settings
        EditorGUILayout.LabelField("Lighting Settings", EditorStyles.boldLabel);
        mainLightIntensity = EditorGUILayout.Slider("Main Light Intensity", mainLightIntensity, 0f, 2f);
        fillLightIntensity = EditorGUILayout.Slider("Fill Light Intensity", fillLightIntensity, 0f, 2f);
        rimLightIntensity = EditorGUILayout.Slider("Rim Light Intensity", rimLightIntensity, 0f, 2f);
        EditorGUILayout.Space();

        // Prefab List
        EditorGUILayout.LabelField($"Found Prefabs ({itemPrefabs.Count})", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Prefab List"))
        {
            RefreshPrefabList();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        foreach (var prefab in itemPrefabs)
        {
            EditorGUILayout.LabelField($"  • {prefab.name}");
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // Export Button
        GUI.enabled = !isExporting && itemPrefabs.Count > 0;
        if (GUILayout.Button("Export All Thumbnails", GUILayout.Height(40)))
        {
            ExportAllThumbnails();
        }
        GUI.enabled = true;

        if (isExporting)
        {
            EditorGUILayout.HelpBox("Exporting thumbnails... Please wait.", MessageType.Warning);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This tool will:\n" +
            "1. Create thumbnails for all item prefabs\n" +
            "2. Save 256x256 PNG files with transparent background\n" +
            "3. Apply proper sprite import settings\n" +
            "4. Optionally auto-link sprites to ItemDataSO files",
            MessageType.Info
        );
    }

    private void RefreshPrefabList()
    {
        itemPrefabs.Clear();

        // Find all prefabs in the Items folder
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { PREFAB_SEARCH_FOLDER });

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                // Verify it has an Item component
                Item itemComponent = prefab.GetComponent<Item>();
                if (itemComponent != null)
                {
                    itemPrefabs.Add(prefab);
                }
            }
        }

        itemPrefabs = itemPrefabs.OrderBy(p => p.name).ToList();
        Debug.Log($"[ThumbnailExporter] Found {itemPrefabs.Count} item prefabs");
    }

    private void ExportAllThumbnails()
    {
        if (itemPrefabs.Count == 0)
        {
            EditorUtility.DisplayDialog("No Prefabs", "No item prefabs found to export.", "OK");
            return;
        }

        isExporting = true;

        // Create output directory if it doesn't exist
        if (!Directory.Exists(OUTPUT_FOLDER))
        {
            Directory.CreateDirectory(OUTPUT_FOLDER);
            AssetDatabase.Refresh();
        }

        // Export each prefab
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            GameObject prefab = itemPrefabs[i];
            float progress = (float)i / itemPrefabs.Count;

            EditorUtility.DisplayProgressBar(
                "Exporting Thumbnails",
                $"Processing {prefab.name} ({i + 1}/{itemPrefabs.Count})",
                progress
            );

            try
            {
                ExportThumbnail(prefab);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ThumbnailExporter] Failed to export {prefab.name}: {e.Message}");
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        // Auto-link sprites if enabled
        if (autoLinkSprites)
        {
            LinkSpritesToItemData();
        }

        isExporting = false;

        EditorUtility.DisplayDialog(
            "Export Complete",
            $"Successfully exported {itemPrefabs.Count} thumbnails to {OUTPUT_FOLDER}",
            "OK"
        );

        Debug.Log($"[ThumbnailExporter] Export complete! {itemPrefabs.Count} thumbnails saved to {OUTPUT_FOLDER}");
    }

    private void ExportThumbnail(GameObject prefab)
    {
        // Use a dedicated layer for thumbnail rendering (layer 31 - typically unused)
        int thumbnailLayer = 31;

        // Create temporary scene objects
        GameObject tempRoot = new GameObject("ThumbnailExport_Temp");
        GameObject itemInstance = Instantiate(prefab, tempRoot.transform);
        itemInstance.transform.localPosition = Vector3.zero;
        itemInstance.transform.localRotation = Quaternion.identity;

        // Set all objects to thumbnail layer
        SetLayerRecursively(itemInstance, thumbnailLayer);

        // Calculate bounds
        Bounds bounds = CalculateBounds(itemInstance);
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        // Create camera
        GameObject cameraObj = new GameObject("ThumbnailCamera");
        cameraObj.transform.SetParent(tempRoot.transform);
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
        camera.orthographic = true;
        camera.orthographicSize = maxSize * 0.6f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
        camera.cullingMask = 1 << thumbnailLayer; // Only render thumbnail layer

        // Position camera with isometric view
        Vector3 cameraOffset = Quaternion.Euler(cameraRotation) * (Vector3.back * cameraDistance);
        cameraObj.transform.position = bounds.center + cameraOffset;
        cameraObj.transform.LookAt(bounds.center);

        // Create lights (all targeting thumbnail layer only)
        GameObject mainLight = new GameObject("MainLight");
        mainLight.transform.SetParent(tempRoot.transform);
        Light mainLightComponent = mainLight.AddComponent<Light>();
        mainLightComponent.type = LightType.Directional;
        mainLightComponent.intensity = mainLightIntensity;
        mainLightComponent.cullingMask = 1 << thumbnailLayer;
        mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        GameObject fillLight = new GameObject("FillLight");
        fillLight.transform.SetParent(tempRoot.transform);
        Light fillLightComponent = fillLight.AddComponent<Light>();
        fillLightComponent.type = LightType.Directional;
        fillLightComponent.intensity = fillLightIntensity;
        fillLightComponent.cullingMask = 1 << thumbnailLayer;
        fillLight.transform.rotation = Quaternion.Euler(-20f, 150f, 0f);

        GameObject rimLight = new GameObject("RimLight");
        rimLight.transform.SetParent(tempRoot.transform);
        Light rimLightComponent = rimLight.AddComponent<Light>();
        rimLightComponent.type = LightType.Directional;
        rimLightComponent.intensity = rimLightIntensity;
        rimLightComponent.cullingMask = 1 << thumbnailLayer;
        rimLight.transform.rotation = Quaternion.Euler(10f, 180f, 0f);

        // Render to texture
        RenderTexture renderTexture = new RenderTexture(THUMBNAIL_SIZE, THUMBNAIL_SIZE, 24, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 4;
        camera.targetTexture = renderTexture;
        camera.Render();

        // Read pixels
        RenderTexture.active = renderTexture;
        Texture2D thumbnail = new Texture2D(THUMBNAIL_SIZE, THUMBNAIL_SIZE, TextureFormat.ARGB32, false);
        thumbnail.ReadPixels(new Rect(0, 0, THUMBNAIL_SIZE, THUMBNAIL_SIZE), 0, 0);
        thumbnail.Apply();
        RenderTexture.active = null;

        // Save PNG
        byte[] bytes = thumbnail.EncodeToPNG();
        string outputPath = $"{OUTPUT_FOLDER}/{prefab.name}.png";
        File.WriteAllBytes(outputPath, bytes);

        // Cleanup
        DestroyImmediate(renderTexture);
        DestroyImmediate(thumbnail);
        DestroyImmediate(tempRoot);

        // Configure sprite import settings
        AssetDatabase.ImportAsset(outputPath);
        TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = 256;
            importer.SaveAndReimport();
        }
    }

    private Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return new Bounds(obj.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private void LinkSpritesToItemData()
    {
        int linkedCount = 0;

        // Find all ItemDataSO files
        string[] itemDataGUIDs = AssetDatabase.FindAssets("t:ItemDataSO");

        foreach (string guid in itemDataGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDataSO itemData = AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);

            if (itemData != null && itemData.itemPrefab != null)
            {
                string prefabName = itemData.itemPrefab.name;
                string spritePath = $"{OUTPUT_FOLDER}/{prefabName}.png";

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                if (sprite != null && itemData.itemSprite != sprite)
                {
                    itemData.itemSprite = sprite;
                    EditorUtility.SetDirty(itemData);
                    linkedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ThumbnailExporter] Auto-linked {linkedCount} sprites to ItemDataSO files");
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
