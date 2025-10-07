using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the pause menu UI overlay and user interactions.
/// Handles ESC key input, menu visibility, and button actions.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    private const string MAIN_MENU_SCENE_NAME = "MainMenu";

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button returnToMainMenuButton;
    [SerializeField] private Button quitButton;

    private OrderMenu orderMenu;

    private void Start()
    {
        // Find OrderMenu reference
        orderMenu = FindFirstObjectByType<OrderMenu>();

        // Hide pause menu on start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Wire up button events
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenuClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        // Subscribe to pause state changes
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPauseStateChanged += OnPauseStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPauseStateChanged -= OnPauseStateChanged;
        }
    }

    private void Update()
    {
        // Listen for ESC key to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC key pressed");

            // Check if PauseManager exists
            if (PauseManager.Instance == null)
            {
                Debug.LogError("PauseManager.Instance is null! Make sure PauseManager GameObject exists in the scene.");
                return;
            }

            // Don't allow pausing if order menu is open
            // Note: OrderMenu component is always active, but its panel may be hidden
            // This check is optional - you can remove it if you want ESC to work always

            Debug.Log("Toggling pause...");
            PauseManager.Instance.TogglePause();
        }
    }

    /// <summary>
    /// Called when pause state changes to show/hide menu.
    /// </summary>
    private void OnPauseStateChanged(bool isPaused)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }
    }

    /// <summary>
    /// Resume button callback - resumes the game.
    /// </summary>
    private void OnResumeClicked()
    {
        Debug.Log("Resume clicked");
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ResumeGame();
        }
    }

    /// <summary>
    /// Settings button callback - placeholder for future implementation.
    /// </summary>
    private void OnSettingsClicked()
    {
        Debug.Log("Settings menu - TBD (To Be Determined)");
        // TODO: Implement settings menu in future iteration
    }

    /// <summary>
    /// Return to Main Menu button callback - loads main menu scene.
    /// </summary>
    private void OnReturnToMainMenuClicked()
    {
        Debug.Log("Returning to main menu...");

        // Resume time before scene change to avoid time scale issues
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.ResumeGame();
        }

        SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
    }

    /// <summary>
    /// Quit button callback - quits the application.
    /// </summary>
    private void OnQuitClicked()
    {
        Debug.Log("Quitting game...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
