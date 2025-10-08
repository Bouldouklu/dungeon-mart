using UnityEngine;

/// <summary>
/// Manages game pause state and provides centralized pause/resume functionality.
/// Handles timescale management and pause state events.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    private bool isPaused = false;
    private float previousTimeScale = 1f;

    /// <summary>
    /// Event triggered when pause state changes.
    /// Parameter: new pause state (true = paused, false = unpaused)
    /// </summary>
    public event System.Action<bool> OnPauseStateChanged;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Pauses the game by setting timescale to 0.
    /// Stores the previous timescale for restoration on resume.
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        isPaused = true;

        Debug.Log("Game paused");
        OnPauseStateChanged?.Invoke(true);
    }

    /// <summary>
    /// Resumes the game by restoring the previous timescale.
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;

        Time.timeScale = previousTimeScale;
        isPaused = false;

        Debug.Log("Game resumed");
        OnPauseStateChanged?.Invoke(false);
    }

    /// <summary>
    /// Toggles between paused and unpaused states.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
}
