using UnityEngine;

/// <summary>
/// Interface for all objects that can be interacted with via mouse clicks.
/// Provides callbacks for hover and click events.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the mouse cursor enters the object's collider.
    /// Use this to show visual feedback (outline, highlight, etc.).
    /// </summary>
    void OnHoverEnter();

    /// <summary>
    /// Called when the mouse cursor exits the object's collider.
    /// Use this to hide visual feedback.
    /// </summary>
    void OnHoverExit();

    /// <summary>
    /// Called when the object is clicked with the left mouse button.
    /// Implement the primary interaction behavior here.
    /// </summary>
    void OnClick();

    /// <summary>
    /// Returns the GameObject this interactable is attached to.
    /// Used by the MouseInteractionManager for reference.
    /// </summary>
    GameObject GetGameObject();
}
