using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    private const string GAME_SCENE_NAME = "GameScene";

    /// <summary>
    /// Loads the main game scene.
    /// Connect this to the Play button's onClick event in the Inspector.
    /// </summary>
    public void LoadGameScene() {
        Debug.Log("Loading game scene...");

        // Play UI click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.UIClick);
        }

        SceneManager.LoadScene(GAME_SCENE_NAME);
    }

    /// <summary>
    /// Opens the settings menu (placeholder for future implementation).
    /// Connect this to the Settings button's onClick event in the Inspector.
    /// </summary>
    public void OpenSettings() {
        Debug.Log("Settings menu - TBD (To Be Determined)");

        // Play UI click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.UIClick);
        }

        // TODO: Implement settings menu in future iteration
    }

    /// <summary>
    /// Quits the application.
    /// Connect this to the Quit button's onClick event in the Inspector.
    /// </summary>
    public void QuitGame() {
        Debug.Log("Quitting game...");

        // Play UI click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(SoundType.UIClick);
        }

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
