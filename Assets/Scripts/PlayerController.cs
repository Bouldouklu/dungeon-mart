using UnityEngine;

/// <summary>
/// Player state manager for mouse-based gameplay.
/// Since movement is handled via mouse clicks on interactive objects,
/// this component now only manages player state and can be used for future player-related functionality.
///
/// NOTE: This script no longer requires Rigidbody. You may remove the Rigidbody component
/// from the Player GameObject in the Unity Editor.
/// </summary>
public class PlayerController : MonoBehaviour {
    // Kept for backwards compatibility with UI managers that may call SetCanMove
    // Can be repurposed for other state management in the future
    private bool isInteractionAllowed = true;

    /// <summary>
    /// Enable or disable player interactions.
    /// Originally used for movement, now kept for backwards compatibility with UI managers.
    /// Can be repurposed to block all interactions when UIs are open.
    /// </summary>
    public void SetCanMove(bool canInteract) {
        isInteractionAllowed = canInteract;
    }

    /// <summary>
    /// Check if player can currently interact with objects.
    /// </summary>
    public bool CanInteract() {
        return isInteractionAllowed;
    }
}