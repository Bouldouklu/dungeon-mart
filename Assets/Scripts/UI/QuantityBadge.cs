using UnityEngine;
using TMPro;

/// <summary>
/// Quantity badge component that displays item count above a shelf slot.
/// Follows the slot transform and shows/hides based on quantity.
/// Similar pattern to DialogueBubble.cs for consistent UI approach.
/// </summary>
public class QuantityBadge : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private Transform slotTransform;
    private Vector3 offset;

    private void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent == null)
        {
            Debug.LogError("QuantityBadge requires a TextMeshProUGUI component in children!");
        }
    }

    /// <summary>
    /// Initializes the quantity badge with slot tracking information.
    /// </summary>
    /// <param name="slot">The slot transform to follow</param>
    /// <param name="badgeOffset">Offset from slot position for badge placement</param>
    public void Initialize(Transform slot, Vector3 badgeOffset)
    {
        slotTransform = slot;
        offset = badgeOffset;
        gameObject.SetActive(false); // Start hidden until UpdateQuantity is called
    }

    /// <summary>
    /// Updates the displayed quantity and visibility.
    /// Shows badge only when quantity > 1.
    /// </summary>
    /// <param name="quantity">Number of items in the slot</param>
    public void UpdateQuantity(int quantity)
    {
        if (quantity > 1)
        {
            if (textComponent != null)
            {
                textComponent.text = $"x{quantity}";
            }
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        // Follow slot position if it still exists
        if (slotTransform != null && gameObject.activeSelf)
        {
            // Convert world position to screen position for UI
            Vector3 worldPosition = slotTransform.position + offset;
            transform.position = Camera.main.WorldToScreenPoint(worldPosition);
        }
    }

    /// <summary>
    /// Cleanup when badge is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        slotTransform = null;
    }
}
