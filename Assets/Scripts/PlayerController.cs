using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactionRange = 1.5f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Shelf nearestShelf;
    private bool canMove = true;

    private void Awake() {
        rb = GetComponent<Rigidbody>();

        // Enforce proper Rigidbody settings for 3D top-down collision detection
        // Note: 3D Rigidbody is dynamic by default (no bodyType property)
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = false; // No gravity for top-down
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY; // Lock to ground plane

        Debug.Log("PlayerController: Rigidbody configured for 3D collision detection");
    }

    private void Update() {
        HandleMovement();
        FindNearestShelf();
        HandleInteraction();
    }

    private void HandleMovement() {
        if (!canMove) {
            // Stop movement when player cannot move (e.g., UI is open)
            rb.linearVelocity = Vector3.zero;
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        // Convert 2D input to 3D movement (XZ plane for top-down)
        Vector3 moveInput3D = new Vector3(moveInput.x, 0, moveInput.y);

        // Use velocity for Dynamic Rigidbody
        rb.linearVelocity = moveInput3D * moveSpeed;
    }

    private void FindNearestShelf() {
        Shelf[] shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);
        nearestShelf = null;
        float closestDistance = interactionRange;

        foreach (Shelf shelf in shelves) {
            float distance = Vector3.Distance(transform.position, shelf.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                nearestShelf = shelf;
            }
        }
    }

    private void HandleInteraction() {
        if (Input.GetKeyDown(KeyCode.E)) {
            // Check if Restock UI is already open
            if (RestockUIManager.Instance != null && RestockUIManager.Instance.IsUIOpen()) {
                // Close UI if already open
                RestockUIManager.Instance.HideRestockUI();
            } else if (nearestShelf != null) {
                // Open restock UI to select item
                if (RestockUIManager.Instance != null) {
                    RestockUIManager.Instance.ShowRestockUI(nearestShelf);
                } else {
                    Debug.LogWarning("RestockUIManager not found in scene!");
                }
            }
        }

        // Debug input handling has been moved to DebugInputManager.cs
    }

    /// <summary>
    /// Enable or disable player movement (called by UI managers)
    /// </summary>
    public void SetCanMove(bool canMove) {
        this.canMove = canMove;
    }

    private void OnCollisionEnter(Collision collision) {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
    }
}