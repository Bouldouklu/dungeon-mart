using UnityEngine;

/// <summary>
/// Manages mouse-based interactions with game objects.
/// Handles raycasting, hover detection, and click handling for IInteractable objects.
/// </summary>
public class MouseInteractionManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask interactableLayerMask = ~0; // Default: all layers
    [SerializeField] private float maxRaycastDistance = 100f;

    [Header("Cursor Settings")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    private IInteractable currentHoveredObject;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("MouseInteractionManager: No main camera found! Make sure your camera is tagged as 'MainCamera'.");
        }
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        // Only process mouse input if no UI is blocking
        if (IsPointerOverUI())
        {
            ClearHover();
            return;
        }

        // Perform raycast from mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRaycastDistance, interactableLayerMask))
        {
            // Check if the hit object is interactable
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // Handle hover state
                HandleHover(interactable);

                // Handle click
                if (Input.GetMouseButtonDown(0)) // Left mouse button
                {
                    HandleClick(interactable);
                }
            }
            else
            {
                ClearHover();
            }
        }
        else
        {
            ClearHover();
        }
    }

    private void HandleHover(IInteractable interactable)
    {
        // If we're hovering over a new object
        if (currentHoveredObject != interactable)
        {
            // Clear previous hover
            if (currentHoveredObject != null)
            {
                currentHoveredObject.OnHoverExit();
            }

            // Set new hover
            currentHoveredObject = interactable;
            currentHoveredObject.OnHoverEnter();

            // Change cursor if custom cursors are set
            if (hoverCursor != null)
            {
                Cursor.SetCursor(hoverCursor, cursorHotspot, CursorMode.Auto);
            }
        }
    }

    private void ClearHover()
    {
        if (currentHoveredObject != null)
        {
            currentHoveredObject.OnHoverExit();
            currentHoveredObject = null;

            // Reset cursor to default
            if (defaultCursor != null)
            {
                Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    private void HandleClick(IInteractable interactable)
    {
        if (interactable != null)
        {
            interactable.OnClick();
        }
    }

    /// <summary>
    /// Checks if the mouse pointer is over a UI element.
    /// Prevents clicking through UI elements.
    /// </summary>
    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Sets the interactable layer mask at runtime
    /// </summary>
    public void SetLayerMask(LayerMask mask)
    {
        interactableLayerMask = mask;
    }

    /// <summary>
    /// Sets custom cursor textures
    /// </summary>
    public void SetCursors(Texture2D defaultCursorTexture, Texture2D hoverCursorTexture, Vector2 hotspot)
    {
        defaultCursor = defaultCursorTexture;
        hoverCursor = hoverCursorTexture;
        cursorHotspot = hotspot;
    }

    private void OnDestroy()
    {
        // Reset cursor when manager is destroyed
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
