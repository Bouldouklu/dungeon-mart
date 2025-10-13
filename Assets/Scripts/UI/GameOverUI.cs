using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the Game Over UI screen.
/// Displays failure reason, statistics, and restart/quit options.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI reasonText;
    [SerializeField] private TextMeshProUGUI statsDaysSurvivedText;
    [SerializeField] private TextMeshProUGUI statsTotalRevenueText;
    [SerializeField] private TextMeshProUGUI statsAmountOwedText;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        // Hide panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Subscribe to game over event
        if (FailStateManager.Instance != null)
        {
            FailStateManager.Instance.OnGameOver += OnGameOver;
        }

        // Wire up button events
        if (tryAgainButton != null)
        {
            tryAgainButton.onClick.AddListener(OnTryAgainClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void OnDestroy()
    {
        if (FailStateManager.Instance != null)
        {
            FailStateManager.Instance.OnGameOver -= OnGameOver;
        }
    }

    /// <summary>
    /// Called when game over is triggered.
    /// </summary>
    private void OnGameOver(GameOverReason reason, int daysSurvived, int totalRevenue, int amountOwed)
    {
        ShowGameOverScreen(reason, daysSurvived, totalRevenue, amountOwed);
    }

    /// <summary>
    /// Shows the game over screen with statistics.
    /// </summary>
    private void ShowGameOverScreen(GameOverReason reason, int daysSurvived, int totalRevenue, int amountOwed)
    {
        if (gameOverPanel == null)
        {
            Debug.LogError("Game Over panel is not assigned!");
            return;
        }

        Debug.Log("=== SHOWING GAME OVER SCREEN ===");

        // Activate panel
        gameOverPanel.SetActive(true);
        Debug.Log($"Game Over panel activated: {gameOverPanel.activeSelf}");

        // Check EventSystem state
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            Debug.Log($"EventSystem found: {UnityEngine.EventSystems.EventSystem.current.gameObject.name}");
            Debug.Log($"EventSystem enabled: {UnityEngine.EventSystems.EventSystem.current.enabled}");
            Debug.Log($"EventSystem GameObject active: {UnityEngine.EventSystems.EventSystem.current.gameObject.activeInHierarchy}");

            // Deselect any currently selected UI element
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("Deselected all UI elements");
        }
        else
        {
            Debug.LogError("EventSystem not found! UI input will not work!");
        }

        // Ensure buttons are interactable
        if (tryAgainButton != null)
        {
            tryAgainButton.interactable = true;
            Debug.Log($"Try Again button: interactable={tryAgainButton.interactable}, active={tryAgainButton.gameObject.activeInHierarchy}");
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.interactable = true;
            Debug.Log($"Main Menu button: interactable={mainMenuButton.interactable}, active={mainMenuButton.gameObject.activeInHierarchy}");
        }
        if (quitButton != null)
        {
            quitButton.interactable = true;
            Debug.Log($"Quit button: interactable={quitButton.interactable}, active={quitButton.gameObject.activeInHierarchy}");
        }

        // Update title
        if (titleText != null)
        {
            titleText.text = "GAME OVER";
        }

        // Update reason text with corporate humor
        if (reasonText != null && FailStateManager.Instance != null)
        {
            reasonText.text = FailStateManager.Instance.GetGameOverMessage();
        }

        // Update statistics
        if (statsDaysSurvivedText != null)
        {
            statsDaysSurvivedText.text = $"Days Survived: {daysSurvived}";
        }

        if (statsTotalRevenueText != null)
        {
            statsTotalRevenueText.text = $"Total Revenue: ${totalRevenue}";
        }

        if (statsAmountOwedText != null)
        {
            if (amountOwed > 0)
            {
                string owedReason = reason == GameOverReason.RentUnpaid ? "Rent Owed" : "Loan Defaulted";
                statsAmountOwedText.text = $"{owedReason}: ${amountOwed}";
                statsAmountOwedText.gameObject.SetActive(true);
            }
            else
            {
                statsAmountOwedText.gameObject.SetActive(false);
            }
        }

        Debug.Log("Game Over screen displayed - buttons should be clickable now");
    }

    /// <summary>
    /// Called when Try Again button is clicked.
    /// Reloads the game scene.
    /// </summary>
    private void OnTryAgainClicked()
    {
        Debug.Log("=== TRY AGAIN BUTTON CLICKED ===");
        Debug.Log("Restarting game...");

        // Resume time before loading scene
        Time.timeScale = 1f;

        // Reload game scene
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Called when Main Menu button is clicked.
    /// Returns to main menu.
    /// </summary>
    private void OnMainMenuClicked()
    {
        Debug.Log("=== MAIN MENU BUTTON CLICKED ===");
        Debug.Log("Returning to main menu...");

        // Resume time before loading scene
        Time.timeScale = 1f;

        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Called when Quit button is clicked.
    /// Quits the application.
    /// </summary>
    private void OnQuitClicked()
    {
        Debug.Log("=== QUIT BUTTON CLICKED ===");
        Debug.Log("Quitting game...");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
