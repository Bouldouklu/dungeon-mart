using UnityEngine;

/// <summary>
/// Phase 1: Provides visual urgency feedback for shelves based on stock levels.
/// Displays a red glow material when shelf capacity drops below 30%.
/// </summary>
[RequireComponent(typeof(Shelf))]
public class ShelfUrgencyVisual : MonoBehaviour {
    [Header("Urgency Visual Settings")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material urgencyMaterial; // Red glow material for low stock
    [SerializeField] private Renderer targetRenderer; // The renderer to apply material to

    [Header("Optional Settings")]
    [SerializeField] private bool debugMode = false;

    private Shelf shelf;
    private Material currentMaterial;

    private void Awake() {
        shelf = GetComponent<Shelf>();

        if (shelf == null) {
            Debug.LogError($"ShelfUrgencyVisual on {gameObject.name} requires a Shelf component!");
            enabled = false;
            return;
        }

        // Auto-find renderer if not assigned
        if (targetRenderer == null) {
            targetRenderer = GetComponentInChildren<Renderer>();
            if (targetRenderer == null) {
                Debug.LogWarning($"ShelfUrgencyVisual on {gameObject.name}: No renderer found. Visual feedback will not work.");
                enabled = false;
                return;
            }
        }

        // Store original material
        if (normalMaterial == null && targetRenderer != null) {
            normalMaterial = targetRenderer.sharedMaterial;
        }
    }

    private void OnEnable() {
        if (shelf != null) {
            shelf.OnLowStock += HandleLowStock;
            shelf.OnStockNormal += HandleStockNormal;
            shelf.OnShelfEmpty += HandleShelfEmpty;
        }
    }

    private void OnDisable() {
        if (shelf != null) {
            shelf.OnLowStock -= HandleLowStock;
            shelf.OnStockNormal -= HandleStockNormal;
            shelf.OnShelfEmpty -= HandleShelfEmpty;
        }
    }

    /// <summary>
    /// Called when shelf capacity drops below 30%.
    /// Applies red urgency material.
    /// </summary>
    private void HandleLowStock() {
        if (urgencyMaterial != null && targetRenderer != null) {
            targetRenderer.material = urgencyMaterial;
            currentMaterial = urgencyMaterial;

            if (debugMode) {
                Debug.Log($"[ShelfUrgencyVisual] {gameObject.name}: LOW STOCK - Applying urgency material");
            }
        }
    }

    /// <summary>
    /// Called when shelf capacity returns above 30%.
    /// Restores normal material.
    /// </summary>
    private void HandleStockNormal() {
        if (normalMaterial != null && targetRenderer != null) {
            targetRenderer.material = normalMaterial;
            currentMaterial = normalMaterial;

            if (debugMode) {
                Debug.Log($"[ShelfUrgencyVisual] {gameObject.name}: Stock normal - Restoring normal material");
            }
        }
    }

    /// <summary>
    /// Called when shelf becomes completely empty.
    /// Applies urgency material (stronger visual cue).
    /// </summary>
    private void HandleShelfEmpty() {
        if (urgencyMaterial != null && targetRenderer != null) {
            targetRenderer.material = urgencyMaterial;
            currentMaterial = urgencyMaterial;

            if (debugMode) {
                Debug.Log($"[ShelfUrgencyVisual] {gameObject.name}: EMPTY - Applying urgency material");
            }
        }
    }

    /// <summary>
    /// Manually refresh visual state based on current shelf capacity.
    /// Useful for debugging or when shelf is initialized with items.
    /// </summary>
    [ContextMenu("Refresh Visual State")]
    public void RefreshVisualState() {
        if (shelf == null) return;

        float capacity = shelf.CapacityPercentage;

        if (capacity < 30f) {
            HandleLowStock();
        } else {
            HandleStockNormal();
        }
    }

    private void OnDestroy() {
        // Restore original material when destroyed
        if (targetRenderer != null && normalMaterial != null) {
            targetRenderer.material = normalMaterial;
        }
    }
}
