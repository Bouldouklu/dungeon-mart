using UnityEngine;

/// <summary>
/// Adds visual feedback to a GameObject to indicate it's interactive.
/// Uses color tint and optional scale pulse for reliable, shader-independent highlighting.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class OutlineEffect : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.42f, 0.616f, 1f); // #FF6B9D
    [SerializeField] private float colorIntensity = 1.5f;
    [Tooltip("Enable scale pulsing animation when hovering")]
    [SerializeField] private bool enableScalePulse = true;
    [SerializeField] private float pulseScale = 1.05f;

    [Header("Animation Settings")]
    [SerializeField] private float fadeSpeed = 10f;
    [SerializeField] private float pulseSpeed = 2f;

    private Renderer objectRenderer;
    private Material[] materials;
    private Color[] originalColors;
    private Vector3 originalScale;
    private bool isHighlightActive = false;
    private float currentHighlightAmount = 0f;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();

        // Store original materials and colors
        materials = objectRenderer.materials;
        originalColors = new Color[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].HasProperty("_Color"))
            {
                originalColors[i] = materials[i].color;
            }
            else
            {
                // Fallback for materials without _Color property
                originalColors[i] = Color.white;
            }
        }

        // Store original scale
        originalScale = transform.localScale;
    }

    private void Update()
    {
        // Smoothly fade highlight in/out
        float targetAmount = isHighlightActive ? 1f : 0f;
        currentHighlightAmount = Mathf.MoveTowards(currentHighlightAmount, targetAmount, fadeSpeed * Time.deltaTime);

        // Apply color tint based on highlight amount
        UpdateColorTint();

        // Apply scale pulse if enabled
        if (enableScalePulse)
        {
            UpdateScalePulse();
        }
    }

    private void UpdateColorTint()
    {
        if (materials == null || originalColors == null) return;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null || !materials[i].HasProperty("_Color")) continue;

            // Lerp between original color and tinted color
            Color targetColor = Color.Lerp(
                originalColors[i],
                originalColors[i] * highlightColor * colorIntensity,
                currentHighlightAmount
            );

            materials[i].color = targetColor;
        }
    }

    private void UpdateScalePulse()
    {
        if (currentHighlightAmount > 0.01f)
        {
            // Create pulsing effect using sine wave
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f; // 0 to 1
            float scaleMultiplier = Mathf.Lerp(1f, pulseScale, pulse * currentHighlightAmount);
            transform.localScale = originalScale * scaleMultiplier;
        }
        else
        {
            // Reset to original scale when not highlighted
            transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Shows the highlight effect
    /// </summary>
    public void ShowOutline()
    {
        isHighlightActive = true;
    }

    /// <summary>
    /// Hides the highlight effect
    /// </summary>
    public void HideOutline()
    {
        isHighlightActive = false;
    }

    /// <summary>
    /// Sets the highlight color
    /// </summary>
    public void SetOutlineColor(Color color)
    {
        highlightColor = color;
    }

    /// <summary>
    /// Sets the color intensity multiplier
    /// </summary>
    public void SetColorIntensity(float intensity)
    {
        colorIntensity = intensity;
    }

    /// <summary>
    /// Enable or disable scale pulse effect
    /// </summary>
    public void SetScalePulseEnabled(bool enabled)
    {
        enableScalePulse = enabled;
        if (!enabled)
        {
            transform.localScale = originalScale;
        }
    }

    private void OnDestroy()
    {
        // Restore original colors before destroying
        if (materials != null && originalColors != null)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].HasProperty("_Color"))
                {
                    materials[i].color = originalColors[i];
                }
            }
        }

        // Restore original scale
        transform.localScale = originalScale;
    }

    private void OnDisable()
    {
        // Reset when disabled
        isHighlightActive = false;
        currentHighlightAmount = 0f;

        if (materials != null && originalColors != null)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].HasProperty("_Color"))
                {
                    materials[i].color = originalColors[i];
                }
            }
        }

        transform.localScale = originalScale;
    }
}
