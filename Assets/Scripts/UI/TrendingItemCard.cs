using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying a single trending item in the morning banner.
/// Shows item icon and name.
/// </summary>
public class TrendingItemCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI demandLabelText;

    /// <summary>
    /// Initializes the trending item card with item data.
    /// </summary>
    public void Initialize(ItemDataSO itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("TrendingItemCard initialized with null item data!");
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

        // Set demand label (optional)
        if (demandLabelText != null)
        {
            demandLabelText.text = "High Demand!";
        }
    }
}
