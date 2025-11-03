using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Demand bubble that shows item icons above customers to indicate wanted items.
/// Displays different states: Wanted (with ?), Positive (with checkmark), Negative (with X).
/// Follows the customer and auto-destroys after duration.
/// </summary>
public class DemandBubble : MonoBehaviour
{
    public enum BubbleState
    {
        Wanted,     // Show item icon with "?" overlay (customer wants this)
        Positive,   // Show item icon with checkmark (customer got it!)
        Negative    // Show item icon with "X" overlay (item not found)
    }

    [Header("UI References")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private Image overlayIconImage;

    [Header("Overlay Icons")]
    [SerializeField] private Sprite wantedIcon; // "?" icon
    [SerializeField] private Sprite positiveIcon; // Checkmark icon
    [SerializeField] private Sprite negativeIcon; // "X" icon

    private Transform speakerTransform;
    private Vector3 offset;
    private float lifetime;
    private float timer;

    /// <summary>
    /// Initializes the demand bubble with item sprite and state.
    /// </summary>
    /// <param name="itemSprite">The item's icon to display</param>
    /// <param name="state">The bubble state (Wanted, Positive, or Negative)</param>
    /// <param name="speaker">The transform to follow (customer)</param>
    /// <param name="bubbleOffset">Offset from speaker position in world space</param>
    /// <param name="duration">How long the bubble stays visible</param>
    public void Initialize(Sprite itemSprite, BubbleState state, Transform speaker, Vector3 bubbleOffset, float duration)
    {
        // Set item icon
        if (itemIconImage != null && itemSprite != null)
        {
            itemIconImage.sprite = itemSprite;
            itemIconImage.enabled = true;
        }
        else if (itemIconImage != null)
        {
            itemIconImage.enabled = false;
        }

        // Set overlay icon based on state
        if (overlayIconImage != null)
        {
            switch (state)
            {
                case BubbleState.Wanted:
                    overlayIconImage.sprite = wantedIcon;
                    overlayIconImage.color = Color.white;
                    break;
                case BubbleState.Positive:
                    overlayIconImage.sprite = positiveIcon;
                    overlayIconImage.color = Color.green;
                    break;
                case BubbleState.Negative:
                    overlayIconImage.sprite = negativeIcon;
                    overlayIconImage.color = Color.red;
                    break;
            }

            overlayIconImage.enabled = overlayIconImage.sprite != null;
        }

        speakerTransform = speaker;
        offset = bubbleOffset;
        lifetime = duration;
        timer = 0f;
    }

    private void Update()
    {
        // Follow speaker if they still exist
        if (speakerTransform != null)
        {
            // Convert world position to screen position for UI
            Vector3 worldPosition = speakerTransform.position + offset;
            transform.position = Camera.main.WorldToScreenPoint(worldPosition);
        }

        // Count down lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
