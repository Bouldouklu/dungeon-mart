using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactionRange = 1.5f;

    private Rigidbody2D rb2d;
    private Vector2 moveInput;
    private Shelf nearestShelf;
    private bool canMove = true;

    private void Awake() {
        rb2d = GetComponent<Rigidbody2D>();

        // Enforce proper Rigidbody2D settings for collision detection
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb2d.gravityScale = 0f;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;

        Debug.Log("PlayerController: Rigidbody2D configured for collision detection");
    }

    private void Update() {
        HandleMovement();
        FindNearestShelf();
        HandleInteraction();
    }

    private void HandleMovement() {
        if (!canMove) {
            // Stop movement when player cannot move (e.g., UI is open)
            rb2d.linearVelocity = Vector2.zero;
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        // Use velocity for Dynamic Rigidbody2D
        rb2d.linearVelocity = moveInput * moveSpeed;
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

        // Debug key to add test inventory
        if (Input.GetKeyDown(KeyCode.I)) {
            InventoryManager.Instance.AddDebugInventory();
        }
    }

    /// <summary>
    /// Enable or disable player movement (called by UI managers)
    /// </summary>
    public void SetCanMove(bool canMove) {
        this.canMove = canMove;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("Collision detected with: " + collision.gameObject.name);
    }
}