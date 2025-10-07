using UnityEngine;

/// <summary>
/// Manages customer dialogue bubbles and corporate humor text display.
/// Shows timed dialogue above customers during key shopping moments.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogueBubblePrefab;
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private float dialogueDuration = 2.5f;
    [SerializeField] private Vector3 bubbleOffset = new Vector3(0, 1.5f, 0);

    private static DialogueManager instance;
    public static DialogueManager Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    /// <summary>
    /// Shows a dialogue bubble above the specified transform for a duration.
    /// </summary>
    /// <param name="text">The dialogue text to display</param>
    /// <param name="speakerTransform">Transform of the character speaking</param>
    /// <param name="duration">How long to show the dialogue (0 = use default)</param>
    public void ShowDialogue(string text, Transform speakerTransform, float duration = 0f)
    {
        if (string.IsNullOrEmpty(text) || speakerTransform == null)
        {
            return;
        }

        float displayDuration = duration > 0 ? duration : dialogueDuration;

        // If we have a prefab, instantiate it
        if (dialogueBubblePrefab != null && dialogueCanvas != null)
        {
            GameObject bubble = Instantiate(dialogueBubblePrefab, dialogueCanvas.transform);
            DialogueBubble bubbleScript = bubble.GetComponent<DialogueBubble>();

            if (bubbleScript != null)
            {
                bubbleScript.Initialize(text, speakerTransform, bubbleOffset, displayDuration);
            }
            else
            {
                Debug.LogWarning("DialogueBubble prefab missing DialogueBubble component!");
                Destroy(bubble);
            }
        }
        else
        {
            // Fallback: Just log to console for now
            Debug.Log($"[Customer Dialogue]: {text}");
        }
    }

    /// <summary>
    /// Shows a random dialogue from an array above the specified transform.
    /// </summary>
    public void ShowRandomDialogue(string[] dialogues, Transform speakerTransform, float duration = 0f)
    {
        if (dialogues == null || dialogues.Length == 0 || speakerTransform == null)
        {
            return;
        }

        string randomDialogue = dialogues[Random.Range(0, dialogues.Length)];
        ShowDialogue(randomDialogue, speakerTransform, duration);
    }
}