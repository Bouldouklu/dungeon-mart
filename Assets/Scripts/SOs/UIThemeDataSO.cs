using UnityEngine;

/// <summary>
/// Centralized UI theme configuration for consistent styling across all UI elements.
/// Defines colors for buttons, text, backgrounds, and interactive states.
/// </summary>
[CreateAssetMenu(fileName = "UITheme", menuName = "DungeonMart/UI Theme")]
public class UIThemeDataSO : ScriptableObject
{
    [Header("Button Colors")]
    [Tooltip("Default button background color")]
    public Color buttonNormalColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray

    [Tooltip("Button color when hovered")]
    public Color buttonHighlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Light gray

    [Tooltip("Button color when clicked")]
    public Color buttonPressedColor = new Color(0.15f, 0.15f, 0.15f, 1f); // Darker gray

    [Tooltip("Button color when disabled")]
    public Color buttonDisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Transparent gray

    [Tooltip("Button color for positive actions (confirm, purchase)")]
    public Color buttonPositiveColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green

    [Tooltip("Button color for negative actions (cancel, close)")]
    public Color buttonNegativeColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Red

    [Header("Text Colors")]
    [Tooltip("Default text color")]
    public Color textPrimaryColor = Color.white;

    [Tooltip("Secondary/subtitle text color")]
    public Color textSecondaryColor = new Color(0.7f, 0.7f, 0.7f, 1f); // Light gray

    [Tooltip("Disabled text color")]
    public Color textDisabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Medium gray

    [Tooltip("Text color for positive values (profit, gains)")]
    public Color textPositiveColor = Color.green;

    [Tooltip("Text color for negative values (costs, losses)")]
    public Color textNegativeColor = Color.red;

    [Tooltip("Text color for warnings")]
    public Color textWarningColor = new Color(1f, 0.65f, 0f, 1f); // Orange

    [Header("Background Colors")]
    [Tooltip("Main panel background color")]
    public Color backgroundPrimaryColor = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark with transparency

    [Tooltip("Secondary panel background color")]
    public Color backgroundSecondaryColor = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Slightly lighter

    [Tooltip("Overlay/modal background color")]
    public Color backgroundOverlayColor = new Color(0f, 0f, 0f, 0.75f); // Semi-transparent black

    [Header("Interactive Colors")]
    [Tooltip("Hover highlight color (for shelves, boxes)")]
    public Color interactiveHighlightColor = new Color(1f, 0.42f, 0.62f, 1f); // Pink (#FF6B9D)

    [Tooltip("Selection color")]
    public Color selectionColor = new Color(1f, 0.84f, 0f, 1f); // Gold

    [Tooltip("Error/invalid state color")]
    public Color errorColor = new Color(1f, 0.2f, 0.2f, 1f); // Bright red

    [Header("Progress/Status Colors")]
    [Tooltip("Progress bar fill color")]
    public Color progressFillColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green

    [Tooltip("Progress bar background color")]
    public Color progressBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Dark gray

    [Tooltip("Completed state color")]
    public Color completedColor = new Color(0f, 0.5f, 0f, 1f); // Dark green

    [Tooltip("Pending state color")]
    public Color pendingColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
}