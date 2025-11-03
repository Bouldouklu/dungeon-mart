using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying a single trending item's demand statistics in the end of day report.
/// Shows item icon, name, wanted/sold/missed counts, and highlights missed opportunities.
/// </summary>
public class DemandItemRow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI wantedText;
    [SerializeField] private TextMeshProUGUI soldText;
    [SerializeField] private TextMeshProUGUI missedText;
    [SerializeField] private Image backgroundImage;

    [Header("Highlight Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color missedOpportunityColor = new Color(1f, 0.7f, 0.7f); // Light red

    /// <summary>
    /// Initializes the demand item row with item data and statistics.
    /// </summary>
    /// <param name="itemData">The item's data</param>
    /// <param name="wanted">How many customers wanted this item</param>
    /// <param name="sold">How many were actually sold</param>
    /// <param name="missed">How many sales were missed (wanted - sold)</param>
    /// <param name="highlightMissed">Whether to highlight this row as a missed opportunity</param>
    public void Initialize(ItemDataSO itemData, int wanted, int sold, int missed, bool highlightMissed)
    {
        if (itemData == null)
        {
            Debug.LogWarning("DemandItemRow initialized with null item data!");
            return;
        }

        // Set item icon
        if (itemIcon != null && itemData.itemSprite != null)
        {
            itemIcon.sprite = itemData.itemSprite;
            itemIcon.enabled = true;
        }
        else if (itemIcon != null)
        {
            itemIcon.enabled = false;
        }

        // Set item name
        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName;
        }

        // Set statistics
        if (wantedText != null)
        {
            wantedText.text = $"Wanted: {wanted}";
        }

        if (soldText != null)
        {
            soldText.text = $"Sold: {sold}";
        }

        if (missedText != null)
        {
            missedText.text = $"Missed: {missed}";

            // Color-code missed text
            if (missed > 0)
            {
                missedText.color = Color.red;
            }
            else
            {
                missedText.color = Color.green;
            }
        }

        // Highlight background if missed opportunity
        if (backgroundImage != null)
        {
            backgroundImage.color = highlightMissed ? missedOpportunityColor : normalColor;
        }
    }
}
