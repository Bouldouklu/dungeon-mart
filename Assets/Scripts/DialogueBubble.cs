using UnityEngine;
using TMPro;

/// <summary>
/// Individual dialogue bubble component that follows a speaker and auto-destroys.
/// Attach to dialogue bubble UI prefab.
/// </summary>
public class DialogueBubble : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private Transform speakerTransform;
    private Vector3 offset;
    private float lifetime;
    private float timer;

    private void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// Initializes the dialogue bubble with text and tracking information.
    /// </summary>
    public void Initialize(string text, Transform speaker, Vector3 bubbleOffset, float duration)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
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
